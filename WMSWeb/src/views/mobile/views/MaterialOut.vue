<template>
  <div class="page-container" :class="{ 'has-footer': selectedStocks.length > 0 }">
    <Header numb="整箱出库"></Header>
    
    <a-row class="input-row">
      <a-col :span="6">
        <div class="label">库位码:</div>
      </a-col>
      <a-col :span="17">
        <a-input v-model:value="cellCode" placeholder="扫描库位码" @keyup.enter="addStock" :allowClear="true" class="search-input" ref="cellInputRef" autofocus>
          <template #suffix>
            <search-outlined class="icon" />
          </template>
        </a-input>
      </a-col>
    </a-row>

    <a-row class="button-row">
      <a-button type="primary" @click="addStock" :loading="loading">添加出库</a-button>
    </a-row>

    <div v-if="selectedStocks.length > 0" class="batch-section">
      <a-row class="section-header">
        <a-col :span="24">
          <h3>
            出库列表 ({{ selectedStocks.length }})
            <span v-if="displayCellCode" class="cell-code-label">{{ displayCellCode }}</span>
          </h3>
        </a-col>
      </a-row>
      
      <a-table 
        :dataSource="selectedStocks" 
        :columns="stockColumns" 
        :pagination="false"
        :scroll="{ y: '200px' }"
        rowKey="id"
        :row-selection="rowSelection"
      />

      <a-row class="summary-row">
        <a-col :span="24">
          <span class="summary-text">共 {{ selectedStocks.length }} 箱，总计 {{ totalQty }} 件</span>
        </a-col>
      </a-row>
    </div>

    <div v-if="selectedStocks.length > 0" class="fixed-footer">
      <a-button type="primary" @click="executeBatchOut" :disabled="!canBatchOut" :loading="executing">批量出库</a-button>
      <a-button @click="reset">重置</a-button>
    </div>

    <div v-if="showSuccess" class="success-message">
      <a-alert type="success" message="成功" description="出库操作已完成" closable @close="showSuccess = false" />
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed, onMounted } from 'vue';
import { SearchOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
import Header from '../header/Header.vue';
import { StockServiceProxy } from '/@/services/ServiceProxies';
import { stocksGetInCell } from './Stock';

const cellCode = ref('');
const cellInputRef = ref<any>();
const displayCellCode = ref('');
const loading = ref(false);
const executing = ref(false);
const showSuccess = ref(false);
const selectedStocks = ref<any[]>([]);
const selectedRowKeys = ref<string[]>([]);

const stockService = new StockServiceProxy();

const stockColumns = [
  {
    title: '物料编号',
    dataIndex: 'materialCode',
    key: 'materialCode',
    align: 'center',
    width: 90,
  },
  {
    title: '物料名称',
    dataIndex: 'materialName',
    key: 'materialName',
    align: 'left',
    width: 160,
    ellipsis: false,
    customCell: () => ({
      style: {
        whiteSpace: 'normal',
        wordBreak: 'break-word',
        lineHeight: '1.4',
      },
    }),
  },
  {
    title: '数量',
    dataIndex: 'totalCountInTime',
    key: 'totalCountInTime',
    align: 'center',
    width: 60,
  },
  {
    title: '批次号',
    dataIndex: 'batchCode',
    key: 'batchCode',
    align: 'center',
    width: 80,
  },
];

const rowSelection = computed(() => ({
  type: 'checkbox',
  selectedRowKeys: selectedRowKeys.value,
  onChange: (selectedKeys: string[], selectedRows: any[]) => {
    selectedRowKeys.value = selectedKeys;
  }
}));

const totalQty = computed(() => {
  return selectedStocks.value.reduce((sum, item) => sum + item.totalCountInTime, 0);
});

const canBatchOut = computed(() => {
  return selectedStocks.value.length > 0 && selectedRowKeys.value.length > 0;
});

onMounted(() => {
  setTimeout(() => {
    if (cellInputRef.value) {
      cellInputRef.value.focus();
    }
  }, 100);
});

async function addStock() {
  if (!cellCode.value.trim()) {
    message.error('请输入库位码');
    return;
  }
  
  loading.value = true;
  
  try {
    const result = await stocksGetInCell(cellCode.value.trim());
    
    if (result && result.length > 0) {
      let addedCount = 0;
      for (const stock of result) {
        const existingIndex = selectedStocks.value.findIndex(s => s.id === stock.id);
        if (existingIndex < 0) {
          selectedStocks.value.push(stock);
          addedCount++;
        }
      }
      if (addedCount > 0) {
        displayCellCode.value = cellCode.value.trim();
        message.success(`已添加 ${addedCount} 条库存到出库列表`);
      } else {
        message.warning('该库位库存已在列表中');
      }
    } else {
      message.warning('该库位未找到库存');
    }
  } catch (error: any) {
    message.error(error?.error?.message || error?.message || '查询失败');
  } finally {
    loading.value = false;
    cellCode.value = '';
  }
}

function removeStock(id) {
  selectedStocks.value = selectedStocks.value.filter(s => s.id !== id);
  selectedRowKeys.value = selectedRowKeys.value.filter(k => k !== id);
}

async function executeBatchOut() {
  if (!canBatchOut.value) {
    message.error('请选择要出库的物料');
    return;
  }
  
  executing.value = true;
  
  const selectedItems = selectedStocks.value.filter(s => selectedRowKeys.value.includes(s.id));
  let successCount = 0;
  let failCount = 0;
  
  try {
    for (const item of selectedItems) {
      try {
        const response = await stockService.stockOutboundDirect(item.id, item.totalCountInTime);
        
        if (response && response.success === true) {
          successCount++;
          removeStock(item.id);
        } else {
          failCount++;
        }
      } catch {
        failCount++;
      }
    }
    
    selectedRowKeys.value = [];
    
    if (selectedStocks.value.length === 0) {
      displayCellCode.value = '';
    }
    
    if (successCount > 0) {
      showSuccess.value = true;
      message.success(`成功出库 ${successCount} 箱`);
      setTimeout(() => { showSuccess.value = false; }, 3000);
    }
    if (failCount > 0) {
      message.warning(`有 ${failCount} 箱出库失败`);
    }
  } catch (error: any) {
    message.error(error?.response?.data?.message || '出库失败');
  } finally {
    executing.value = false;
  }
}

function reset() {
  cellCode.value = '';
  displayCellCode.value = '';
  selectedStocks.value = [];
  selectedRowKeys.value = [];
}
</script>

<style scoped>
.page-container { 
  padding: 15px; 
  background: #f5f5f5; 
  min-height: 100vh; 
}

.page-container.has-footer {
  padding-bottom: 80px;
}

.input-row { 
  margin-bottom: 15px; 
}

.label { 
  text-align: right; 
  line-height: 36px; 
  font-weight: 500; 
  color: #666;
  font-size: 14px;
}

.search-input { 
  width: 100%; 
  height: 36px;
  border-radius: 6px;
}

.icon { 
  color: #1890ff; 
  font-size: 18px;
}

.button-row { 
  display: flex; 
  gap: 12px; 
  padding: 10px 0; 
}

.button-row button { 
  flex: 1; 
  height: 40px;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
}

.fixed-footer {
  display: flex;
  align-items: center;
  gap: 12px;
  position: fixed;
  left: 0;
  right: 0;
  bottom: 0;
  height: 60px;
  padding: 0 15px;
  background: #fff;
  border-top: 1px solid #f0f0f0;
  box-shadow: 0 -2px 8px rgba(0, 0, 0, 0.06);
  z-index: 1000;
}

.fixed-footer button {
  flex: 1;
  height: 40px;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
}

.batch-section { 
  margin: 15px 0; 
  background: #fff;
  border-radius: 8px;
  padding: 15px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.06);
}

.section-header { 
  margin-bottom: 12px; 
}

.section-header h3 { 
  margin: 0; 
  font-size: 14px; 
  font-weight: 500;
  color: #333;
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 8px;
}

.cell-code-label {
  font-size: 13px;
  font-weight: 600;
  color: #1890ff;
  padding: 2px 8px;
  background: #e6f7ff;
  border-radius: 4px;
}

.summary-row { 
  padding: 12px 0; 
  border-top: 1px dashed #ddd;
  margin-top: 8px;
}

.summary-text { 
  font-size: 13px; 
  color: #666;
  font-weight: 500;
}

.success-message { 
  margin: 15px 0; 
}

:deep(.ant-table-thead > tr > th) {
  padding: 8px 6px;
  font-size: 12px;
  font-weight: 500;
  background: #f8f9fa;
}

:deep(.ant-table-tbody > tr > td) {
  padding: 10px 6px;
  font-size: 12px;
}
</style>