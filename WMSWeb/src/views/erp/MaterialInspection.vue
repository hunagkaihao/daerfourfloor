<template>
  <div class="material-inspection">
    <a-form layout="horizontal" class="search-form" @submit.prevent="load()">
      <a-row :gutter="24">
        <a-col v-bind="searchFieldCol">
          <a-form-item label="物料码">
            <a-input
              v-model:value="materialCode"
              placeholder="请输入物料码"
              allow-clear
              :disabled="busy"
              @pressEnter="load()"
            />
          </a-form-item>
        </a-col>
        <a-col v-show="searchExpanded || visibleSearchCount > 1" v-bind="searchFieldCol">
          <a-form-item label="制单开始日期">
            <input
              v-model="beginMakeTime"
              type="date"
              title="从所选日期00:00:00开始查询"
              :disabled="busy"
            />
          </a-form-item>
        </a-col>
        <a-col v-show="searchExpanded" v-bind="searchFieldCol">
          <a-form-item label="页码">
            <a-input-number
              v-model:value="queryPage"
              :min="1"
              :max="2147483647"
              :precision="0"
              placeholder="默认第1页"
              title="pagesize：查询页码，从1开始"
              :disabled="busy"
              @pressEnter="load()"
            />
          </a-form-item>
        </a-col>
        <a-col v-show="searchExpanded" v-bind="searchFieldCol">
          <a-form-item label="每页条数">
            <a-input-number
              v-model:value="queryPageSize"
              :min="1"
              :max="100"
              :precision="0"
              placeholder="默认100条，最多100条"
              title="pagecount：每页条数，范围1至100"
              :disabled="busy"
              @pressEnter="load()"
            />
          </a-form-item>
        </a-col>
        <a-col v-bind="searchActionCol">
          <a-form-item class="search-actions">
            <a-space>
              <a-button :disabled="busy" @click="reset">重置</a-button>
              <a-button type="primary" :loading="busy" :disabled="busy" @click="load()">
                查询
              </a-button>
              <a-button
                class="filter-toggle"
                :aria-expanded="searchExpanded"
                @click="searchExpanded = !searchExpanded"
              >
                {{ searchExpanded ? '收起条件' : '展开条件' }}
                <span class="filter-toggle__arrow" aria-hidden="true">{{
                  searchExpanded ? '▲' : '▼'
                }}</span>
              </a-button>
            </a-space>
          </a-form-item>
        </a-col>
      </a-row>
    </a-form>
    <a-alert v-if="notice" :message="notice" :type="noticeType" show-icon class="notice" />
    <BasicTable
      title="来料检验单"
      :columns="columns"
      :dataSource="items"
      :loading="busy"
      :pagination="false"
      :canResize="false"
      :scroll="{ x: 1600 }"
      :showTableSetting="true"
      rowKey="rowKey"
      size="small"
      bordered
    >
      <template #verified="{ record }">
        <a-tag :color="record.isVerified ? 'green' : 'orange'">
          {{ record.isVerified ? '已审核' : '未审核' }}
        </a-tag>
      </template>
      <template #rejected="{ record }">
        <a-tag v-if="record.isRejected === true" color="red">已拒收</a-tag>
        <span v-else>{{ record.isRejected === false ? '未拒收' : '未知' }}</span>
      </template>
    </BasicTable>
    <div class="query-footer">
      <span v-if="currentPage">
        已加载至第{{ currentPage }}页，显示{{ items.length }}条匹配记录，U8共{{ totalCount }}条记录
      </span>
      <span v-else>点击查询获取来料检验单，可选填条件缩小范围</span>
      <a-button v-if="hasMore" :loading="busy" :disabled="busy" @click="load(true)">
        加载更多检验单
      </a-button>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { computed, ref, watch } from 'vue';
  import { BasicTable } from '/@/components/Table';
  import type { BasicColumn } from '/@/components/Table';
  import { queryMaterialInspection } from '/@/views/mobile/views/U8InboundApi';
  import type { U8MaterialInspectionItem } from '/@/views/mobile/views/U8InboundApi';
  import { useBreakpoint } from '/@/hooks/event/useBreakpoint';

  /** 检验单列表行；同一检验单可含多个来源明细，按返回页和行号生成唯一表格键。 */
  type InspectionRow = U8MaterialInspectionItem & { rowKey: string };

  /** U8未提供的检验数量显示为空，不据此推断为零或检验合格。 */
  function formatQuantity({ text }: { text?: number | null }) {
    return text === null || text === undefined ? '-' : String(text);
  }

  /** ERP只展示真实U8检验结果，关联ASN缺失时仍保留该检验记录。 */
  const columns: BasicColumn[] = [
    { title: '来料检验单号', dataIndex: 'checkCode', width: 180 },
    { title: '物料码', dataIndex: 'materialCode', width: 180 },
    { title: '批号', dataIndex: 'batchCode', width: 150 },
    { title: '来源到货单', dataIndex: 'sourceCode', width: 180 },
    { title: '来源明细ID', dataIndex: 'sourceAutoId', width: 120 },
    { title: '审核状态', dataIndex: 'isVerified', width: 110, slots: { customRender: 'verified' } },
    { title: '审核人', dataIndex: 'verifier', width: 120 },
    { title: '合格数量', dataIndex: 'qualifiedQuantity', width: 120, customRender: formatQuantity },
    {
      title: '不合格数量',
      dataIndex: 'disqualifiedQuantity',
      width: 120,
      customRender: formatQuantity,
    },
    { title: '拒收状态', dataIndex: 'isRejected', width: 110, slots: { customRender: 'rejected' } },
    { title: '关联ASN', dataIndex: 'asnCode', width: 180 },
  ];

  /** 查询条件均为选填；全部留空时使用U8默认条件分页查询。 */
  const materialCode = ref('');
  const beginMakeTime = ref('');
  /** U8的pagesize为起始页码、pagecount为每页条数；清空时分别使用1和100。 */
  const queryPage = ref<number | null>(1);
  const queryPageSize = ref<number | null>(100);
  /** 与ASN列表一致：默认一行展示常用条件和右侧按钮，其余条件展开后显示且保留已填值。 */
  const searchExpanded = ref(false);
  const { realWidthRef, screenEnum } = useBreakpoint();
  const visibleSearchCount = computed(() => (realWidthRef.value >= screenEnum.XL ? 2 : 1));
  /** 展开时使用响应式列宽，收起时为查询、重置和展开按钮预留完整一列。 */
  const searchFieldCol = computed(() =>
    searchExpanded.value
      ? { xs: 24, sm: 12, xl: 8 }
      : { span: visibleSearchCount.value === 2 ? 8 : 12 }
  );
  const searchActionCol = computed(() =>
    searchExpanded.value
      ? { xs: 24, sm: 12, xl: 24 }
      : { span: visibleSearchCount.value === 2 ? 8 : 12 }
  );
  /** 每次只查询一页，下一页失败时保留此前成功加载的记录。 */
  const items = ref<InspectionRow[]>([]);
  const currentPage = ref(0);
  const totalCount = ref(0);
  const hasMore = ref(false);
  const busy = ref(false);
  /** 分开展示查询失败、无匹配记录和正常查询，避免把失败解释成没有单据。 */
  const notice = ref('');
  const noticeType = ref<'success' | 'warning' | 'error'>('warning');

  /** 查询条件改变即清除旧结果，防止将旧物料记录误认为新条件的结果。 */
  function clearResults() {
    items.value = [];
    currentPage.value = 0;
    totalCount.value = 0;
    hasMore.value = false;
    notice.value = '';
  }
  watch([materialCode, beginMakeTime, queryPage, queryPageSize], clearResults, {
    flush: 'sync',
  });

  /** 清空筛选与结果，由操作员再次点击查询获取默认条件下的记录。 */
  function reset() {
    if (busy.value) return;
    materialCode.value = '';
    beginMakeTime.value = '';
    queryPage.value = 1;
    queryPageSize.value = 100;
    clearResults();
  }

  /** 按指定页码和每页条数查询U8；加载更多继续下一页，追加失败可重试原页。 */
  async function load(append = false) {
    if (busy.value || (append && !hasMore.value)) return;
    const code = materialCode.value.trim();
    const begin = beginMakeTime.value;
    const startPage = queryPage.value ?? 1;
    const pageSize = queryPageSize.value ?? 100;
    const page = append ? currentPage.value + 1 : startPage;
    if (!Number.isInteger(page) || page < 1 || page > 2147483647) {
      notice.value = '页码须为1至2147483647的整数';
      noticeType.value = 'warning';
      return;
    }
    if (!Number.isInteger(pageSize) || pageSize < 1 || pageSize > 100) {
      notice.value = '每页条数须为1至100的整数';
      noticeType.value = 'warning';
      return;
    }
    const isCurrentQuery = () =>
      code === materialCode.value.trim() &&
      begin === beginMakeTime.value &&
      startPage === (queryPage.value ?? 1) &&
      pageSize === (queryPageSize.value ?? 100);
    if (!append) clearResults();
    busy.value = true;
    notice.value = '';
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
        const rows = (result.items || []).map((item, index) => ({
          ...item,
          rowKey: `${result.page}:${index}`,
        }));
        items.value = append ? [...items.value, ...rows] : rows;
        currentPage.value = result.page;
        totalCount.value = result.totalCount;
        hasMore.value = result.hasMore;
      }
      notice.value = result.message || (result.success ? '查询完成' : 'U8来料检验单查询失败');
      noticeType.value = result.success ? (items.value.length ? 'success' : 'warning') : 'error';
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
</script>

<style scoped lang="less">
  .material-inspection {
    padding: 20px;
  }
  .search-form {
    margin: 0 0 12px;
    padding: 16px;
    background: #fff;

    :deep(.ant-row) {
      row-gap: 16px;
    }

    // 沿用ASN列表的栅格排版，每组标签和控件在各自列内显示。
    :deep(.ant-form-item) {
      display: flex;
      flex-wrap: nowrap;
      min-width: 0;
      margin: 0;
    }

    :deep(.ant-form-item-label) {
      flex: 0 0 110px;
      width: 110px;
      padding: 0;
      white-space: nowrap;
    }

    :deep(.ant-form-item-control) {
      flex: 1 1 0;
      min-width: 0;
    }

    :deep(.ant-input-affix-wrapper),
    :deep(.ant-input-number) {
      width: 100%;
      min-width: 0;
    }

    input[type='date'] {
      box-sizing: border-box;
      width: 100%;
      min-width: 0;
      height: 32px;
      padding: 4px 11px;
      border: 1px solid #d9d9d9;
      border-radius: 2px;
    }

    .search-actions :deep(.ant-form-item-control-input-content) {
      display: flex;
      justify-content: flex-end;
      white-space: nowrap;
    }
  }

  .filter-toggle {
    color: @primary-color;

    &__arrow {
      margin-left: 6px;
      font-size: 10px;
    }
  }
  .notice {
    margin-bottom: 12px;
  }
  .query-footer {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    padding: 16px;
    background: #fff;
    color: #666;
  }
</style>
