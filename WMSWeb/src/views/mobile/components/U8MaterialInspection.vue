<template>
  <section class="material-inspection">
    <h3>物料检验状态</h3>
    <div class="search">
      <a-input
        v-model:value="materialCode"
        :lazy="false"
        placeholder="扫描箱码或输入物料码"
        :disabled="busy || disabled"
        allow-clear
        @pressEnter="load()"
      />
      <a-button :disabled="busy || disabled || !materialCode.trim()" :loading="busy" @click="load()"
        >查询状态</a-button
      >
    </div>
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
  </section>
</template>

<script setup lang="ts">
  import { ref, watch } from 'vue';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { confirmMaterialInspection, querySavedMaterialInspection } from '../views/U8InboundApi';
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
  const { createConfirm } = useMessage();
  const items = ref<U8MaterialInspectionItem[]>([]);
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

  /** 提取实际参与查询的物料码，箱码补发或首尾空格变化不视为切换物料。 */
  function queryMaterialCode() {
    return materialCode.value.split(',')[0].trim();
  }

  /** 实际查询条件变化时清除旧来源；输入提交和箱码格式转换不清空同一查询。 */
  watch(
    queryMaterialCode,
    () => {
      items.value = [];
      notice.value = '';
      selectedKey.value = '';
      emit('loaded', '', []);
    },
    { flush: 'sync' }
  );
  watch(busy, (value) => emit('busy-change', value), { flush: 'sync' });

  /** 明确选择一条可关联的检验单；只传递来源，不把单据存在性写成检验合格。 */
  function applySelection(item: U8MaterialInspectionItem) {
    if (!item.asnCode || !item.asnLines?.length) return;
    selectedKey.value = itemKey(item);
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

  /** 操作员确认所选检验记录；服务端核对本地已同步审核状态并返回持久化后的ASN明细。 */
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
            materialCode: queryMaterialCode(),
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

  /** 一次查询该物料全部未完成采购入库的本地检验单，丢弃物料已变化的响应。 */
  async function load(force = false) {
    if (busy.value || (props.disabled && !force) || !materialCode.value.trim()) return;
    // 查询时按箱码逗号协议提取首段，避免扫码枪逐字符输入时提前截断后续内容。
    const code = queryMaterialCode();
    materialCode.value = code;
    if (!code) {
      notice.value = '箱码首段物料码不能为空，请重新扫描或输入物料码';
      noticeType.value = 'warning';
      return;
    }
    const previousKey = selectedKey.value;
    const isCurrentQuery = () => code === queryMaterialCode();
    busy.value = true;
    notice.value = '';
    items.value = [];
    selectedKey.value = '';
    emit('loaded', '', []);
    try {
      const result = await querySavedMaterialInspection(code);
      if (!isCurrentQuery()) return;
      if (result.success) {
        items.value = result.items || [];
      }
      notice.value =
        result.message ||
        (result.success
          ? result.hasInspection
            ? '已查询到本地保存的来料检验单'
            : '本地暂无未完成采购入库的来料检验单，已完成记录已跳过'
          : '本地来料检验单查询失败');
      noticeType.value = result.success ? (items.value.length ? 'success' : 'warning') : 'error';
      const previous = items.value.find((item) => itemKey(item) === previousKey);
      if (previous) applySelection(previous);
      else if (!previousKey && items.value.length === 1) applySelection(items.value[0]);
    } catch (error: any) {
      if (!isCurrentQuery()) return;
      notice.value =
        error?.code === 'ECONNABORTED' || error?.code === 'ETIMEDOUT'
          ? '本地来料检验单查询超时，请稍后重试'
          : error?.message || '本地来料检验单查询失败';
      noticeType.value = 'error';
    } finally {
      busy.value = false;
    }
  }

  /** 组盘或取消后刷新本地单据与剩余数量，内部刷新允许在父页提交锁内调用。 */
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
</style>
