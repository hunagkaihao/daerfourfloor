<template>
  <div>
    <BasicTable @register="registerTable" :clickToRowSelect="false" size="small">
      <template #toolbar>
        <a-button
          type="primary"
          @click="MoveWall"
        >
          {{ t('调拨下架') }}
        </a-button>
        <a-button
          type="primary"
          @click="openModal"
        >
          {{ t('Excel导出') }}
        </a-button>
      </template>
      <template #isActive="{ record }">
        <Tag :color="record.isActive ? 'green' : 'red'">
          {{ record.isActive ? t('common.enabled') : t('common.disEnabled') }}
        </Tag>
      </template>

    </BasicTable>


    <ExpExcelModal @register="register" @success="defaultHeader" />
    <StockAdjustmentModal @register="registerAdjustmentModal" />

  </div>
</template>

<script lang="ts" setup>
import { onMounted, onUnmounted } from 'vue';
import { useMessage } from '/@/hooks/web/useMessage';
import { BasicTable, useTable, TableAction } from '/@/components/Table';
import {
  tableColumns,
  searchFormSchema,
  gettable
} from './Good';
import { useI18n } from '/@/hooks/web/useI18n';
import { ExpExcelModal } from '/@/components/Excel';
import type { ExportModalResult } from '/@/components/Excel/src/typing';
//import ImportOut from './ExcelOut.vue';
import { useModal } from '/@/components/Modal';
import { StockDto } from '/@/services/ServiceProxies';
import { message } from 'ant-design-vue';
import StockAdjustmentModal from './StockAdjustmentModal.vue';
const [register, { openModal }] = useModal();
const [registerAdjustmentModal, { openModal: openAdjustmentModal }] = useModal();
const { createConfirm } = useMessage();
const { t } = useI18n();
// table配置
const [registerTable, {getDataSource, reload,getSelectRows,clearSelectedRowKeys, setSearchFormValues}] = useTable({
  columns: tableColumns,
  formConfig: {
    labelWidth: 70,
    schemas: searchFormSchema,
    fieldMapToTime: [['time', ['stockInDateStart', 'stockInDateEnd'], 'YYYY-MM-DD HH:mm:ss'],
    ['productiontime', ['supplierProductionDateStart', 'supplierProductionDateEnd'], 'YYYY-MM-DD HH:mm:ss'],
    ['bztime', ['expiryDateStart', 'expiryDateEnd'], 'YYYY-MM-DD HH:mm:ss'],],
    autoSubmitOnEnter: true, // 启用回车键自动提交
  },
  api: gettable, // 启用API调用
  showTableSetting: true,
  useSearchForm: true,
  bordered: true,
  canResize: true,
  rowKey: 'id', //设置选择项的key
  showIndexColumn: false,
  rowSelection: { type: 'checkbox' },
});
async function MoveWall(){
  let b = getSelectRows() 
      console.log('Selected Rows:', b); // 打印选中行数据
      if (b.length == 0) {
        message.warn(t('请先选择物料'));
        return;
      }
      
      // 这里可以添加调拨下架的API调用
      // 例如: await _stockServiceProxy.moveWall(b.map(item => item.id));
      message.info('调拨下架功能已禁用，请等待后续更新');
      clearSelectedRowKeys();
}


async function defaultHeader({ filename, bookType }: ExportModalResult) {
        // 直接获取当前页面数据进行导出
        const data = getDataSource();
        if (!data || data.length === 0) {
          message.warning('没有数据可导出');
          return;
        }

        // 准备导出数据
        const exportData = data.map((item: any) => {
          return {
            物料编码: item.materialCode || '',
            物料名称: item.materialName || '',
            生产批次: item.batchCode || '',
            等级: item.grade || '',
            箱号: item.boxNumber || '',
            规格: item.specs || '',
            单位: item.unit || '',
            数量: item.receiveTotalCount || 0,
            库位: item.cellCode || '',
            容器: item.boxCode || '',
            入库日期: item.stockInDate || '',
            状态: item.status || '',
            入库类型: item.stockInType || '',
            批次号: item.batchCode || '',
            供应商: item.supplierName || ''
          };
        });

        // 导出Excel（按需加载 xlsx，避免页面初始化时加载失败）
        const { jsonToSheetXlsx } = await import('/@/components/Excel/src/Export2Excel');
        jsonToSheetXlsx({ data: exportData, filename: filename || '库存数据', bookType: bookType || 'xlsx' });
        message.success('导出成功');
      }

// 监听事件来打开调整弹窗
const handleOpenAdjustmentModal = () => {
  const recordData = sessionStorage.getItem('selectedStockRecord');
  
  if (recordData) {
    try {
      const record = JSON.parse(recordData);
      // 移除接口调用，显示提示信息
      message.info('库存调整功能已禁用，请等待后续更新');
      console.log('库存调整功能已禁用，选中记录:', record);
    } catch (error) {
      console.error('解析数据失败:', error);
    }
  }
};

// 监听库存调整成功事件，刷新数据
const handleStockAdjustmentSuccess = () => {
  // 移除数据刷新功能
  console.log('库存调整成功事件已禁用');
};

onMounted(() => {
  window.addEventListener('openStockAdjustment', handleOpenAdjustmentModal);
  window.addEventListener('stockAdjustmentSuccess', handleStockAdjustmentSuccess);
});

onUnmounted(() => {
  window.removeEventListener('openStockAdjustment', handleOpenAdjustmentModal);
  window.removeEventListener('stockAdjustmentSuccess', handleStockAdjustmentSuccess);
});

</script>