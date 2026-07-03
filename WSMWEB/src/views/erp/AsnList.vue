<template>
  <div class="asn-list">
    <BasicTable @register="registerTable" size="small" :clickToRowSelect="false">
      <template #expandedRowRender="{ record }">
        <div class="expanded-row">
          <div class="expanded-row__title">
            ASN明细（{{ record.lineCount }} 行，待入库总数 {{ formatQuantity(record.totalPendingStockInQuantity) }}）
          </div>
          <BasicTable
            :columns="detailTableColumns"
            :dataSource="record.asnItems || []"
            :pagination="false"
            size="small"
            bordered
            rowKey="autoid"
            :scroll="{ x: 1800 }"
          />
        </div>
      </template>
      <template #action="{ record }">
        <Button type="link" @click="handlePushReceipt(record.ccode)">
          推送收货单
        </Button>
      </template>
    </BasicTable>
  </div>
</template>

<script setup lang="ts">
import { defineComponent } from 'vue';
import { BasicTable, useTable } from '/@/components/Table';
import { Button } from 'ant-design-vue';
import {
  parentTableColumns,
  detailTableColumns,
  searchFormSchema,
  getAsnListAsync,
  pushErpReceiptAsync,
} from './AsnList';
import { useI18n } from '/@/hooks/web/useI18n';
import { useMessage } from '/@/hooks/web/useMessage';

defineComponent({
  name: 'AsnList',
});

const { t } = useI18n();
const { createConfirm } = useMessage();

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
  scroll: { x: 1600 },
  actionColumn: {
    width: 120,
    title: t('common.action'),
    dataIndex: 'action',
    slots: {
      customRender: 'action',
    },
    fixed: 'right',
  },
});

const handlePushReceipt = async (asnCode: string) => {
  createConfirm({
    iconType: 'info',
    title: t('common.tip'),
    content: `确定要推送ASN码为 ${asnCode} 的收货单吗？`,
    onOk: async () => {
      await pushErpReceiptAsync(asnCode);
      reload();
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
