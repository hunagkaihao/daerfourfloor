<template>
  <div class="asn-list">
    <a-alert type="info" show-icon message="保存ASN后按采购订单分别生成到货单；U8审核后由PDA报检，检验合格后逐箱组盘，整单完成后生成采购入库单。" style="margin-bottom: 12px" />
    <BasicTable @register="registerTable" size="small" :clickToRowSelect="false">
      <template #expandedRowRender="{ record }">
        <div class="expanded-row">
          <div class="expanded-row__title">
            ASN明细（{{ record.lineCount }} 行，待组盘总数 {{ formatQuantity(record.totalPendingStockInQuantity) }}）
          </div>
          <BasicTable
            :columns="detailTableColumns"
            :dataSource="record.asnItems || []"
            :pagination="false"
            size="small"
            bordered
            rowKey="autoid"
            :scroll="{ x: 2400 }"
            :actionColumn="{ width: 250, title: '操作', dataIndex: 'detailAction', fixed: 'right', slots: { customRender: 'detailAction' } }"
          >
            <template #detailAction="{ record: line }">
              <Button type="link" :disabled="line.status === 4 || !!pushing[record.ccode] || !getAsnOrderPushState(record.asnItems, line.cordercode).canPushArrival"
                :loading="pushing[record.ccode] === 'arrival:' + line.cordercode" @click="handlePush(record, 'arrival', line.cordercode)">
                {{ line.isPushedToErp ? '到货单已推送' : '推送到货单' }}
              </Button>
              <Button type="link" :disabled="line.status === 4 || !!pushing[record.ccode] || !getAsnOrderPushState(record.asnItems, line.cordercode).canPushInspection"
                :loading="pushing[record.ccode] === 'inspection:' + line.cordercode" @click="handlePush(record, 'inspection', line.cordercode)">
                {{ line.isInspectionReportPushed ? '报检单已推送' : '推送来料报检单' }}
              </Button>
            </template>
          </BasicTable>
        </div>
      </template>
      <template #action="{ record }">
        <Button type="link" :disabled="!record.canPushArrival || !!pushing[record.ccode]"
          :loading="pushing[record.ccode] === 'arrival:*'" @click="handlePush(record, 'arrival')">
          {{ record.arrivalPushError || record.arrivalStatus === '部分已推送' ? '重试到货单' : '生成到货单' }}
        </Button>
        <Button type="link" :disabled="!record.canPushInspection || !!pushing[record.ccode]"
          :loading="pushing[record.ccode] === 'inspection:*'" @click="handlePush(record, 'inspection')">
          {{ record.inspectionPushError || record.inspectionStatus === '部分已推送' ? '重试报检单' : '生成报检单' }}
        </Button>
      </template>
    </BasicTable>
  </div>
</template>

<script setup lang="ts">
import { defineComponent, reactive } from 'vue';
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

defineComponent({
  name: 'AsnList',
});

const { t } = useI18n();
const { createConfirm } = useMessage();
const pushing = reactive<Record<string, string>>({});

function formatQuantity(value?: number | null) {
  if (value === null || value === undefined || Number.isNaN(Number(value))) {
    return '0';
  }
  return String(Number(value));
}

const [registerTable, { reload }] = useTable({
  columns: parentTableColumns,
  formConfig: {
    labelWidth: 80,
    schemas: searchFormSchema,
    showAdvancedButton: false,
    actionColOptions: {
      span: 4,
      style: { textAlign: 'right' },
    },
  },
  api: getAsnListAsync,
  showTableSetting: true,
  useSearchForm: true,
  bordered: true,
  canResize: true,
  showIndexColumn: true,
  rowKey: 'rowKey',
  expandable: true,
  scroll: { x: 2700 },
  actionColumn: {
    width: 230,
    title: t('common.action'),
    dataIndex: 'action',
    slots: {
      customRender: 'action',
    },
    fixed: 'right',
  },
});

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
