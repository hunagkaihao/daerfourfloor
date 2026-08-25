<template>
  <div>
    <BasicTable @register="registerTable" :clickToRowSelect="false" size="small">
    </BasicTable>
  </div>
</template>

<script lang="ts" setup>
import { BasicTable, useTable } from '/@/components/Table';
import {
  ydStockTableColumns,
  ydStockSearchFormSchema,
  getYdStockTableListAsync,
} from './YdStockQuery';
import { useI18n } from '/@/hooks/web/useI18n';

const { t } = useI18n();

// table配置
const [registerTable] = useTable({
  columns: ydStockTableColumns,
  formConfig: {
    labelWidth: 70,
    schemas: ydStockSearchFormSchema,
    fieldMapToTime: [
      ['time', ['stockInDateStart', 'stockInDateEnd'], 'YYYY-MM-DD HH:mm:ss'],
      ['productiontime', ['supplierProductionDateStart', 'supplierProductionDateEnd'], 'YYYY-MM-DD HH:mm:ss'],
      ['bztime', ['expiryDateStart', 'expiryDateEnd'], 'YYYY-MM-DD HH:mm:ss'],
    ],
  },
  api: getYdStockTableListAsync,
  showTableSetting: true,
  useSearchForm: true,
  bordered: true,
  canResize: true,
  rowKey: 'id',
  showIndexColumn: false,
});
</script>
