<template>
  <div>
    <BasicTable @register="registerTable" :clickToRowSelect="false" size="small">
    </BasicTable>
  </div>
</template>

<script lang="ts" setup>
import { BasicTable, useTable } from '/@/components/Table';
import {
  tableColumns,
  searchFormSchema
} from './MaterialStockStatistics';
import { useI18n } from '/@/hooks/web/useI18n';

const { t } = useI18n();

// table配置
const [registerTable, { reload }] = useTable({
  columns: tableColumns,
  formConfig: {
    labelWidth: 70,
    schemas: searchFormSchema,
    fieldMapToTime: [['time', ['stockInTimeStart', 'stockInTimeEnd'], 'YYYY-MM-DD HH:mm:ss']],
  },
  // api: gettable, // 移除API调用
  dataSource: [], // 使用静态数据
  showTableSetting: true,
  useSearchForm: true,
  bordered: true,
  canResize: true,
  showIndexColumn: false,
  rowSelection: { type: 'checkbox' },
});

// 移除gettable函数，不再需要接口调用
</script>
