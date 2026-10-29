<template>
  <section class="material-inspection">
    <h3>物料检验状态</h3>
    <div class="search">
      <a-input
        v-model:value="materialCode"
        placeholder="扫描箱码或输入物料码"
        :disabled="busy || disabled"
        allow-clear
        @pressEnter="load()"
      />
      <a-button :disabled="busy || disabled || !materialCode.trim()" :loading="busy" @click="load()"
        >查询状态</a-button
      >
    </div>
    <details class="filters">
      <summary>更多查询条件（选填）</summary>
      <div class="filter-fields">
        <label
          >制单开始日期
          <input
            v-model="beginMakeTime"
            type="date"
            title="从所选日期00:00:00开始查询"
            :disabled="busy || disabled"
          />
        </label>
        <label
          >页码
          <a-input-number
            v-model:value="queryPage"
            :min="1"
            :max="2147483647"
            :precision="0"
            placeholder="默认第1页"
            title="pagesize：查询页码，从1开始"
            :disabled="busy || disabled"
            @pressEnter="load()"
          />
        </label>
        <label
          >每页条数
          <a-input-number
            v-model:value="queryPageSize"
            :min="1"
            :max="100"
            :precision="0"
            placeholder="默认100条"
            title="pagecount：每页条数，范围1至100"
            :disabled="busy || disabled"
            @pressEnter="load()"
          />
        </label>
      </div>
    </details>
    <a-alert v-if="notice" :message="notice" :type="noticeType" show-icon />
    <a-card
      v-for="(item, index) in items"
      :key="itemKey(item) + ':' + index"
      :title="`来料检验单：${item.checkCode || '未返回单号'}`"
      size="small"
    >
      <div>物料码：{{ item.materialCode }}；批号：{{ item.batchCode || '-' }}</div>
      <div>来源到货单：{{ item.sourceCode || '-' }}</div>
      <div
        >审核状态：{{ item.isVerified ? '已审核' : '未审核' }}；审核人：{{
          item.verifier || '-'
        }}</div
      >
      <div
        >合格数量：{{ item.qualifiedQuantity ?? '-' }}；不合格数量：{{
          item.disqualifiedQuantity ?? '-'
        }}</div
      >
      <div v-if="item.isRejected === true">拒收状态：已拒收</div>
      <div v-if="item.asnCode">组盘来源ASN：{{ item.asnCode }}</div>
      <div v-if="item.asnLines?.length">检验状态：{{ inspectionStateText(item) }}</div>
      <p v-else>该检验单尚未匹配到本地ASN来源，暂不能用于组盘。</p>
      <a-button
        v-if="item.asnCode && selectedKey !== itemKey(item)"
        type="primary"
        :disabled="busy || disabled"
        @click="select(item)"
      >
        选择此来源组盘
      </a-button>
      <div v-if="selectedKey === itemKey(item)" class="inspection-actions">
        <p v-if="!item.isVerified">该检验单尚未审核，审核后可确认检验结果。</p>
        <a-button type="primary" @click="confirm(item, 2)">合格</a-button>
        <a-button danger @click="confirm(item, 3)">不合格</a-button>
      </div>
    </a-card>
    <div v-if="hasMore" class="load-more">
      <a-button :loading="busy" :disabled="busy || disabled" @click="loadMore"
        >加载更多检验单</a-button
      >
      <span>已加载至第{{ currentPage }}页，U8共{{ totalCount }}条记录</span>
    </div>
  </section>
</template>

<script setup lang="ts">
  import { ref, watch } from 'vue';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { confirmMaterialInspection, queryMaterialInspection } from '../views/U8InboundApi';
  import type { AsnMaterialInspectionLine, U8MaterialInspectionItem } from '../views/U8InboundApi';

  const props = defineProps<{
    /** 组盘提交或待提交箱码存在时，禁止切换物料及检验单来源。 */
    disabled?: boolean;
  }>();
  const emit = defineEmits<{
    /** 当前选择检验单对应的本地ASN；未选择或查询失效时清空来源。 */
    (event: 'loaded', asnCode: string, lines: AsnMaterialInspectionLine[]): void;
    /** 查询、检验确认与下方组盘、取消及搬运共用互斥状态。 */
    (event: 'busy-change', busy: boolean): void;
  }>();
  const materialCode = ref('');
  /** 制单开始日期为选填，后端按U8本地日期零点查询当天及之后的检验单。 */
  const beginMakeTime = ref('');
  /** U8的pagesize为页码、pagecount为每页条数；留空分别使用1和100。 */
  const queryPage = ref<number | null>(1);
  const queryPageSize = ref<number | null>(100);
  const { createConfirm } = useMessage();
  const items = ref<U8MaterialInspectionItem[]>([]);
  /** 每次只加载一页，后续页由操作员按需请求，避免长时间连续查询。 */
  const currentPage = ref(0);
  const totalCount = ref(0);
  const hasMore = ref(false);
  /** 保存已选来源所在页，组盘刷新时只重新读取该页。 */
  const selectedPage = ref(1);
  const itemPages = new Map<string, number>();
  const busy = ref(false);
  const notice = ref('');
  const noticeType = ref<'success' | 'warning' | 'error'>('warning');
  /** 记录已选U8来源，内部刷新后仅恢复同一条来源，不自动切换到其他托盘来源。 */
  const selectedKey = ref('');

  /** 用检验单和来源明细共同标识记录，避免同物料多单据时选择错误来源。 */
  function itemKey(item: U8MaterialInspectionItem) {
    return JSON.stringify([
      item.checkId,
      item.checkCode,
      item.sourceCode,
      item.sourceAutoId,
      item.materialCode,
      item.batchCode,
      item.asnCode,
    ]);
  }

  /** 查询条件变化时立刻清除旧结果及下方组盘来源。 */
  watch(
    [materialCode, beginMakeTime, queryPage, queryPageSize],
    () => {
      items.value = [];
      notice.value = '';
      selectedKey.value = '';
      currentPage.value = 0;
      totalCount.value = 0;
      hasMore.value = false;
      selectedPage.value = 1;
      itemPages.clear();
      emit('loaded', '', []);
    },
    { flush: 'sync' }
  );
  watch(busy, (value) => emit('busy-change', value), { flush: 'sync' });

  /** 明确选择一条可关联的检验单；只传递来源，不把单据存在性写成检验合格。 */
  function applySelection(item: U8MaterialInspectionItem) {
    if (!item.asnCode || !item.asnLines?.length) return;
    selectedKey.value = itemKey(item);
    selectedPage.value = itemPages.get(selectedKey.value) || (queryPage.value ?? 1);
    emit('loaded', item.asnCode, item.asnLines);
  }

  /** 操作员切换来源时遵守父页组盘锁。 */
  function select(item: U8MaterialInspectionItem) {
    if (!busy.value && !props.disabled) applySelection(item);
  }

  /** 已确认的合格、不合格均显示为已检验，并保留具体结论。 */
  function inspectionStateText(item: U8MaterialInspectionItem) {
    return Array.from(
      new Set(
        item.asnLines.map((line) =>
          line.materialInspectionStatus === 2
            ? '已检验（合格）'
            : line.materialInspectionStatus === 3
            ? '已检验（不合格）'
            : line.materialInspectionStatusName || '待检'
        )
      )
    ).join('、');
  }

  /** 按钮始终可点击；提交时提示来源或作业锁问题，审核及检验结论由服务端校验。 */
  function canSubmitConfirmation(item: U8MaterialInspectionItem) {
    if (busy.value) return false;
    const reason = props.disabled
      ? '当前正在组盘或存在待提交箱码，请完成或清空后再确认检验结果'
      : selectedKey.value !== itemKey(item) || !items.value.includes(item)
      ? '所选检验单来源已变化，请重新选择'
      : !item.asnCode || !item.checkCode || !(item.checkId! > 0) || !(item.sourceAutoId! > 0)
      ? '检验单来源信息不完整，请重新查询后核对'
      : '';
    if (!reason) return true;
    notice.value = reason;
    noticeType.value = 'warning';
    return false;
  }

  /** 操作员确认所选检验记录；服务端重查审核状态并返回持久化后的本地明细。 */
  function confirm(item: U8MaterialInspectionItem, target: 2 | 3) {
    if (!canSubmitConfirmation(item)) return;
    const key = itemKey(item);
    const label = target === 2 ? '合格' : '不合格';
    createConfirm({
      title: `确认${label}`,
      content: `确认将检验单${item.checkCode}对应的物料标记为已检验（${label}）？`,
      onOk: async () => {
        if (!canSubmitConfirmation(item)) return;
        busy.value = true;
        notice.value = '';
        try {
          const result = await confirmMaterialInspection({
            materialCode: materialCode.value.trim(),
            beginMakeTime: beginMakeTime.value || undefined,
            page: itemPages.get(key) || (queryPage.value ?? 1),
            pageSize: queryPageSize.value ?? 100,
            checkId: item.checkId!,
            checkCode: item.checkCode!,
            sourceAutoId: item.sourceAutoId!,
            asnCode: item.asnCode!,
            inspectionStatus: target,
          });
          if (selectedKey.value !== key || !items.value.includes(item)) return;
          if (result.success && result.data?.length) {
            const updated = new Map(result.data.map((line) => [line.autoid, line]));
            // 相同来源可能出现在多条检验记录中，所有已显示卡片同步同一份本地结论。
            for (const record of items.value) {
              record.asnLines = record.asnLines.map((line) => updated.get(line.autoid) || line);
            }
            applySelection(item);
            notice.value = result.message || `已检验（${label}）`;
            noticeType.value = 'success';
          } else {
            notice.value = result.message || '确认检验结果失败，请重新查询后核对';
            noticeType.value = 'error';
          }
        } catch (error: any) {
          if (selectedKey.value !== key || !items.value.includes(item)) return;
          notice.value = error?.message || '确认检验结果失败，请重新查询后核对';
          noticeType.value = 'error';
        } finally {
          busy.value = false;
        }
      },
    });
  }

  /** POST查询物料检验单，区分未找到和查询失败，并丢弃条件已变化的响应。 */
  async function load(force = false, append = false) {
    if (busy.value || (props.disabled && !force) || !materialCode.value.trim()) return;
    // 查询时按箱码逗号协议提取首段，避免扫码枪逐字符输入时提前截断后续内容。
    const code = materialCode.value.split(',')[0].trim();
    materialCode.value = code;
    if (!code) {
      notice.value = '箱码首段物料码不能为空，请重新扫描或输入物料码';
      noticeType.value = 'warning';
      return;
    }
    const begin = beginMakeTime.value;
    const previousKey = selectedKey.value;
    const startPage = queryPage.value ?? 1;
    const pageSize = queryPageSize.value ?? 100;
    const page = append
      ? currentPage.value + 1
      : force && previousKey
      ? selectedPage.value
      : startPage;
    if (
      !Number.isInteger(page) ||
      page < 1 ||
      page > 2147483647 ||
      !Number.isInteger(pageSize) ||
      pageSize < 1 ||
      pageSize > 100
    ) {
      notice.value = '页码须为正整数，每页条数须为1至100的整数';
      noticeType.value = 'warning';
      return;
    }
    const isCurrentQuery = () =>
      code === materialCode.value.trim() &&
      begin === beginMakeTime.value &&
      startPage === (queryPage.value ?? 1) &&
      pageSize === (queryPageSize.value ?? 100);
    busy.value = true;
    notice.value = '';
    if (!append) {
      items.value = [];
      selectedKey.value = '';
      hasMore.value = false;
      currentPage.value = 0;
      totalCount.value = 0;
      itemPages.clear();
      emit('loaded', '', []);
    }
    try {
      const result = await queryMaterialInspection(
        code,
        undefined,
        begin || undefined,
        page,
        pageSize
      );
      if (!isCurrentQuery()) return;
      if (result.success) {
        const rows = result.items || [];
        const existing = new Set(items.value.map(itemKey));
        // 只追加新来源，U8返回重复行时不重复展示或切换已选来源。
        for (const item of rows) {
          const key = itemKey(item);
          if (!itemPages.has(key)) itemPages.set(key, result.page);
          if (!existing.has(key)) {
            items.value.push(item);
            existing.add(key);
          }
        }
        currentPage.value = result.page;
        totalCount.value = result.totalCount;
        hasMore.value = result.hasMore;
      }
      notice.value =
        result.message ||
        (result.success
          ? result.hasInspection
            ? '已查询到对应的U8来料检验单'
            : '未查询到对应的U8来料检验单'
          : 'U8来料检验单查询失败');
      noticeType.value = result.success ? (items.value.length ? 'success' : 'warning') : 'error';
      const previous = items.value.find((item) => itemKey(item) === previousKey);
      if (previous) applySelection(previous);
      else if (!previousKey && items.value.length === 1) applySelection(items.value[0]);
    } catch (error: any) {
      if (!isCurrentQuery()) return;
      notice.value =
        error?.code === 'ECONNABORTED' || error?.code === 'ETIMEDOUT'
          ? 'U8来料检验单查询超时，请稍后重试或补充筛选条件'
          : error?.message || 'U8来料检验单查询失败';
      noticeType.value = 'error';
    } finally {
      busy.value = false;
    }
  }

  /** 按需继续读取下一页；失败保留已显示记录及当前选择，允许重新加载该页。 */
  function loadMore() {
    if (hasMore.value) return load(false, true);
  }

  /** 组盘或取消后刷新U8单据与本地剩余数量，内部刷新允许在父页提交锁内调用。 */
  function reload() {
    return load(true);
  }
  defineExpose({ reload });
</script>

<style scoped>
  .material-inspection {
    margin: 12px;
    padding: 12px;
    border: 1px solid #eee;
    border-radius: 8px;
  }
  .material-inspection h3 {
    margin: 0 0 8px;
  }
  .material-inspection p {
    color: #666;
    font-size: 12px;
  }
  .search {
    display: flex;
    gap: 8px;
    margin: 8px 0;
  }
  .search .ant-input-affix-wrapper {
    flex: 1;
    min-width: 0;
  }
  .filters {
    margin: 8px 0;
  }
  .filters summary {
    cursor: pointer;
    color: #666;
  }
  .filter-fields {
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
    margin-top: 8px;
  }
  .filter-fields > * {
    flex: 1;
    min-width: 220px;
  }
  .filter-fields label {
    display: flex;
    align-items: center;
    gap: 8px;
  }
  .filter-fields .ant-input-number {
    flex: 1;
    min-width: 0;
  }
  .filter-fields input[type='date'] {
    flex: 1;
    min-width: 0;
    border: 1px solid #d9d9d9;
    padding: 4px;
  }
  .ant-card {
    margin-top: 10px;
    overflow-wrap: anywhere;
  }
  .ant-card .ant-btn {
    margin-top: 8px;
  }
  .inspection-actions .ant-btn + .ant-btn {
    margin-left: 8px;
  }
  .load-more {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 8px;
    margin-top: 12px;
  }
  .load-more span {
    color: #666;
    font-size: 12px;
  }
</style>
