<template>
  <div>
    <BasicTable @register="registerTable" :clickToRowSelect="false" size="small">
      <template #toolbar>
        <a-button
          type="primary"
          @click="beforeOpenOutstockModal"
        >
          {{ t('出库') }}
        </a-button>
      </template>
    </BasicTable>

    <OutstockModal @register="registerOutstockModal" @success="handleOutstockSuccess" />
  </div>
</template>

<script lang="ts" setup>
import { onMounted, ref } from 'vue';
import { useMessage } from '/@/hooks/web/useMessage';
import { BasicTable, useTable } from '/@/components/Table';
import {
  tableColumns,
  searchFormSchema,
  getStocks
} from './Outstock';
import { useI18n } from '/@/hooks/web/useI18n';
import { useModal } from '/@/components/Modal';
import { PagedStockQueryDto } from '/@/services/ServiceProxies';
import { message } from 'ant-design-vue';
import OutstockModal from './OutstockModal.vue';

const [registerOutstockModal, { openModal: openOutstockModal }] = useModal();
const { createConfirm } = useMessage();
const { t } = useI18n();

// 打开出库模态框
const beforeOpenOutstockModal = () => {
  const selectedRows = getSelectRows();
  if (selectedRows.length === 0) {
    message.warning('请选择要出库的库存');
    return;
  }
  openOutstockModal(true, {
    selectedStocks: selectedRows
  });
};

// table配置
const [registerTable, { getDataSource, reload, getSelectRows, clearSelectedRowKeys }] = useTable({
  columns: tableColumns,
  formConfig: {
    labelWidth: 70,
    schemas: searchFormSchema,
    autoSubmitOnEnter: true, // 启用回车键自动提交
  },
  api: async (params) => {
    const queryParams = new PagedStockQueryDto();
    Object.assign(queryParams, params);
    const data = await getStocks(queryParams);
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
  rowSelection: { type: 'checkbox' },
});

// 处理出库成功
const handleOutstockSuccess = () => {
  message.success('出库任务创建成功');
  reload();
};

onMounted(() => {
  // 页面加载时初始化数据
  reload();
});
</script>

<style scoped lang="less">
</style>
