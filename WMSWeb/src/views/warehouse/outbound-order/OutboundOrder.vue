<template>
  <div>
    <BasicTable @register="registerTable" :clickToRowSelect="false" size="small">
      <template #toolbar>
        <a-button
          type="primary"
          @click="handleRefresh"
        >
          {{ t('刷新') }}
        </a-button>
      </template>
      <template #expandedRowRender="{ record }">
        <div class="expanded-row">
          <h4>物料明细</h4>
          <BasicTable 
            :columns="itemColumns" 
            :dataSource="record.outboundItems || []" 
            :pagination="false" 
            size="small"
          />
        </div>
      </template>
    </BasicTable>
  </div>
</template>

<script lang="ts" setup>
import { onMounted, ref } from 'vue';
import { useMessage } from '/@/hooks/web/useMessage';
import { BasicTable, useTable, BasicColumn } from '/@/components/Table';
import {
  tableColumns,
  searchFormSchema,
  getOutboundOrders
} from './OutboundOrder';
import { useI18n } from '/@/hooks/web/useI18n';
import { message } from 'ant-design-vue';

const { createConfirm } = useMessage();
const { t } = useI18n();

// 物料明细列配置
const itemColumns: BasicColumn[] = [
  {
    title: '物料编号',
    dataIndex: 'materialCode',
  },
  {
    title: '物料名称',
    dataIndex: 'materialName',
  },
  {
    title: '规格',
    dataIndex: 'specs',
  },
  {
    title: '计划出库数量',
    dataIndex: 'planOutboundQty',
  },
  {
    title: '实际出库数量',
    dataIndex: 'actualOutboundQty',
  },
];

// table配置
const [registerTable, { getDataSource, reload, getSelectRows, clearSelectedRowKeys }] = useTable({
  columns: tableColumns,
  formConfig: {
    labelWidth: 70,
    schemas: searchFormSchema,
    autoSubmitOnEnter: true, // 启用回车键自动提交
  },
  api: async (params) => {
    const data = await getOutboundOrders(params);
    return {
      items: data,
      total: data.length
    };
  },
  showTableSetting: true,
  useSearchForm: true,
  bordered: true,
  canResize: true,
  rowKey: 'id', //设置选择项的key
  showIndexColumn: false,
  expandable: true,
});

// 刷新数据
const handleRefresh = () => {
  reload();
};

onMounted(() => {
  // 页面加载时初始化数据
  reload();
});
</script>

<style scoped lang="less">
.expanded-row {
  padding: 16px;
  background-color: #f5f5f5;
  margin: 0 -16px;
  
  h4 {
    margin-bottom: 12px;
    color: #333;
  }
}
</style>
