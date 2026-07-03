<template>
  <div>
    <BasicTable @register="registerTable" :clickToRowSelect="false" size="small">
    </BasicTable>
  </div>
</template>

<script lang="ts" setup>
import { BasicTable, useTable } from '/@/components/Table';
import { useMessage } from '/@/hooks/web/useMessage';
import moment from 'moment';
import {
  tableColumns,
  searchFormSchema
} from './WorkshopReceiptStatistics';
import { useI18n } from '/@/hooks/web/useI18n';

const { t } = useI18n();
const { createError } = useMessage();

// 修改搜索表单配置，添加时间验证
const modifiedSearchFormSchema = searchFormSchema.map(schema => {
  if (schema.field === 'time') {
    return {
      ...schema,
      componentProps: {
        ...schema.componentProps,
        onChange: (dates: any) => {
          if (dates && dates.length === 2) {
            const startDate = moment(dates[0]);
            const endDate = moment(dates[1]);
            const daysDiff = endDate.diff(startDate, 'days');
            
            if (daysDiff > 3) {
              createError(t('timeRangeLimit'));
              return false;
            }
          }
        }
      }
    };
  }
  return schema;
});

// table配置
const [registerTable, { reload, getForm }] = useTable({
  columns: tableColumns,
  formConfig: {
    labelWidth: 70,
    schemas: modifiedSearchFormSchema,
    fieldMapToTime: [['time', ['StartTime', 'EndTime'], 'YYYY-MM-DD']],
    resetFunc: () => {
      // 重置时设置正确的默认时间范围（3天内）
      const form = getForm();
      form?.setFieldsValue({
        time: [moment().subtract(2, 'days'), moment()]
      });
    },
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
