<template>
  <div class="asn-list">
    <BasicTable @register="registerTable" size="small" :clickToRowSelect="false">
      <template #form-advanceAfter>
        <Button class="filter-toggle" :aria-expanded="searchExpanded" @click="toggleSearchConditions">
          {{ searchExpanded ? '收起条件' : '展开条件' }}
          <span class="filter-toggle__arrow" aria-hidden="true">{{ searchExpanded ? '▲' : '▼' }}</span>
        </Button>
      </template>
      <template #expandedRowRender="{ record }">
        <div class="expanded-row">
          <div class="expanded-row__title">
            ASN明细（{{ record.lineCount }} 行，待组盘总数 {{ formatQuantity(record.totalPendingStockInQuantity) }}）
          </div>
          <BasicTable
            :columns="detailTableColumns"
            :dataSource="record.asnItems || []"
            :pagination="false"
            :canResize="false"
            size="small"
            bordered
            rowKey="autoid"
            :actionColumn="{ width: 280, title: '操作', dataIndex: 'detailAction', fixed: 'right', slots: { customRender: 'detailAction' } }"
          >
            <template #detailAction="{ record: line }">
              <div class="asn-actions">
                <Button type="link" :disabled="line.status === 4 || !!pushing[record.ccode] || !getAsnOrderPushState(record.asnItems, line.cordercode).canPushArrival"
                  :loading="pushing[record.ccode] === 'arrival:' + line.cordercode" @click="handlePush(record, 'arrival', line.cordercode)">
                  {{ line.isPushedToErp ? '到货单已推送' : '推送到货单' }}
                </Button>
                <Button type="link" :disabled="line.status === 4 || !!pushing[record.ccode] || !getAsnOrderPushState(record.asnItems, line.cordercode).canPushInspection"
                  :loading="pushing[record.ccode] === 'inspection:' + line.cordercode" @click="handlePush(record, 'inspection', line.cordercode)">
                  {{ line.isInspectionReportPushed ? '报检单已推送' : '推送来料报检单' }}
                </Button>
              </div>
            </template>
          </BasicTable>
        </div>
      </template>
      <template #action="{ record }">
        <div class="asn-actions">
          <Button type="link" :disabled="!record.canPushArrival || !!pushing[record.ccode]"
            :loading="pushing[record.ccode] === 'arrival:*'" @click="handlePush(record, 'arrival')">
            {{ record.arrivalPushError || record.arrivalStatus === '部分已推送' ? '重试到货单' : '生成到货单' }}
          </Button>
          <Button type="link" :disabled="!record.canPushInspection || !!pushing[record.ccode]"
            :loading="pushing[record.ccode] === 'inspection:*'" @click="handlePush(record, 'inspection')">
            {{ record.inspectionPushError || record.inspectionStatus === '部分已推送' ? '重试报检单' : '生成报检单' }}
          </Button>
        </div>
      </template>
    </BasicTable>
  </div>
</template>

<script setup lang="ts">
import { computed, defineComponent, nextTick, reactive, ref } from 'vue';
import { BasicTable, useTable } from '/@/components/Table';
import { Button } from 'ant-design-vue';
import {
  parentTableColumns,
  detailTableColumns,
  searchFormSchema,
  getAsnListAsync,
  pushAsnArrivalAsync,
  pushAsnInspectionAsync,
} from './AsnList';
import { useI18n } from '/@/hooks/web/useI18n';
import { useMessage } from '/@/hooks/web/useMessage';
import { getAsnOrderPushState } from '/@/api/erp/asn';
import { useBreakpoint } from '/@/hooks/event/useBreakpoint';

defineComponent({
  name: 'AsnList',
});

const { t } = useI18n();
const { createConfirm } = useMessage();
const pushing = reactive<Record<string, string>>({});
/** 默认仅展示一行筛选；收起只隐藏额外条件，保留已填值和查询效果。 */
const searchExpanded = ref(false);
const { realWidthRef, screenEnum } = useBreakpoint();
/** 按屏幕宽度为查询、重置和展开按钮预留空间，超出的条件展开后显示。 */
const searchFormConfig = computed(() => {
  const visibleCount = realWidthRef.value >= screenEnum.XXL ? 3 : realWidthRef.value >= screenEnum.XL ? 2 : 1;
  const fieldSpan = visibleCount === 3 ? 6 : visibleCount === 2 ? 7 : 12;
  return {
    labelWidth: 90,
    schemas: searchFormSchema.map((schema, index) => ({
      ...schema,
      show: searchExpanded.value || index < visibleCount,
      colProps: searchExpanded.value ? schema.colProps : { span: fieldSpan },
    })),
    showAdvancedButton: false,
    actionColOptions: {
      ...(searchExpanded.value ? { xs: 24, sm: 24, xl: 12 } : { span: 24 - visibleCount * fieldSpan }),
      style: { textAlign: 'right' as const },
    },
  };
});

function formatQuantity(value?: number | null) {
  if (value === null || value === undefined || Number.isNaN(Number(value))) {
    return '0';
  }
  return String(Number(value));
}

const [registerTable, { reload, redoHeight }] = useTable({
  columns: parentTableColumns,
  formConfig: searchFormConfig,
  api: getAsnListAsync,
  showTableSetting: true,
  useSearchForm: true,
  bordered: true,
  canResize: true,
  showIndexColumn: true,
  rowKey: 'rowKey',
  expandable: true,
  // 横向滚动宽度由BasicTable按当前列宽计算，避免固定操作列挤压其他字段。
  actionColumn: {
    width: 260,
    title: t('common.action'),
    dataIndex: 'action',
    slots: {
      customRender: 'action',
    },
    fixed: 'right',
  },
});

/** 切换额外筛选条件，并在表单完成布局后重新计算表格可用高度。 */
async function toggleSearchConditions() {
  searchExpanded.value = !searchExpanded.value;
  await nextTick();
  redoHeight();
}

/** 整单与明细共用同一ASN忙状态；明细按所属采购订单判断条件并限定推送范围。 */
const handlePush = (record: any, kind: 'arrival' | 'inspection', orderCode?: string) => {
  const state = orderCode ? getAsnOrderPushState(record.asnItems || [], orderCode) : record;
  if (pushing[record.ccode] || !(kind === 'arrival' ? state.canPushArrival : state.canPushInspection)) return;
  createConfirm({
    iconType: 'info',
    title: t('common.tip'),
    content: `确定为ASN ${record.ccode}${orderCode ? ' 的采购订单 ' + orderCode + ' 全部有效明细' : ''}生成U8${kind === 'arrival' ? '到货单' : '来料报检单'}吗？${orderCode ? '其他采购订单不受影响。' : ''}`,
    onOk: async () => {
      if (pushing[record.ccode]) return;
      pushing[record.ccode] = kind + ':' + (orderCode || '*');
      try {
        const result = await (kind === 'arrival' ? pushAsnArrivalAsync : pushAsnInspectionAsync)(record.ccode, orderCode);
        await reload();
        return result;
      } finally {
        delete pushing[record.ccode];
      }
    },
  });
};
</script>

<style scoped lang="less">
.asn-list {
  padding: 20px;

  :deep(.ant-table-thead > tr > th) {
    white-space: normal;
    overflow-wrap: break-word;
  }
}

.filter-toggle {
  color: @primary-color;

  &__arrow {
    margin-left: 6px;
    font-size: 10px;
  }
}

.asn-actions {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 4px 8px;

  :deep(.ant-btn) {
    max-width: 100%;
    height: auto;
    padding: 0 4px;
    white-space: normal;
  }
}

.expanded-row {
  padding: 12px 16px 16px 48px;
  background-color: #fafafa;

  &__title {
    margin-bottom: 8px;
    font-weight: 600;
    color: #333;
  }

  :deep(.ant-table-cell) {
    white-space: normal;
    word-break: break-word;
  }
}
</style>
