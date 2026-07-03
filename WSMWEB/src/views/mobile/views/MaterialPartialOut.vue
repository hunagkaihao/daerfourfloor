<template>
  <div class="page-container">
    <Header numb="拆箱出库"></Header>
    
    <a-row class="input-row">
      <a-col :span="6">
        <div class="label">库位码:</div>
      </a-col>
      <a-col :span="17">
        <a-input v-model:value="cellCode" placeholder="扫描库位码" @keyup.enter="scanCellCode" :allowClear="true" class="search-input" ref="cellInputRef" autofocus>
          <template #suffix>
            <search-outlined class="icon" />
          </template>
        </a-input>
      </a-col>
    </a-row>

    <a-row class="button-row">
      <a-button type="primary" @click="scanCellCode" :loading="loading">查询库存</a-button>
    </a-row>

    <div v-if="stockList.length > 1 && !stockData" class="stock-select-section">
      <h3>
        请选择出库物料
        <span v-if="displayCellCode" class="cell-code-label">{{ displayCellCode }}</span>
      </h3>
      <a-table
        :dataSource="stockList"
        :columns="stockColumns"
        :pagination="false"
        :scroll="{ y: '200px' }"
        rowKey="id"
        :customRow="customRow"
      />
    </div>

    <div v-if="stockData" class="info-card">
      <a-descriptions :column="1" bordered size="small">
        <template #title>
          <span>
            库存信息
            <span v-if="displayCellCode" class="cell-code-label">{{ displayCellCode }}</span>
          </span>
        </template>
        <a-descriptions-item label="物料编号">{{ stockData.materialCode }}</a-descriptions-item>
        <a-descriptions-item label="物料名称">{{ stockData.materialName }}</a-descriptions-item>
        <a-descriptions-item label="规格型号">{{ stockData.specs }}</a-descriptions-item>
        <a-descriptions-item label="批次号">{{ stockData.batchCode || '无' }}</a-descriptions-item>
        <a-descriptions-item label="箱号">{{ stockData.boxNumber || '无' }}</a-descriptions-item>
        <a-descriptions-item label="库存数量">{{ stockData.totalCountInTime }} {{ stockData.unit }}</a-descriptions-item>
      </a-descriptions>
    </div>

    <div v-if="stockData" class="qty-section">
      <a-row class="qty-row">
        <a-col :span="8">
          <div class="label">出库数量:</div>
        </a-col>
        <a-col :span="16">
          <a-input-number 
            v-model:value="outQty" 
            :min="1" 
            :max="stockData.totalCountInTime" 
            class="qty-input"
            :step="1"
          />
        </a-col>
      </a-row>
      <div class="qty-hint">当前库存: {{ stockData.totalCountInTime }} {{ stockData.unit }}</div>
    </div>

    <a-row v-if="stockData" class="button-row">
      <a-button type="primary" @click="executeOut" :disabled="!canOut" :loading="executing">确认出库</a-button>
      <a-button @click="reset">重置</a-button>
    </a-row>

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
const outQty = ref(1);
const loading = ref(false);
const executing = ref(false);
const showSuccess = ref(false);
const stockData = ref<any>(null);
const stockList = ref<any[]>([]);

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
    width: 140,
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

const canOut = computed(() => {
  return stockData.value !== null && outQty.value > 0 && outQty.value <= stockData.value.totalCountInTime;
});

onMounted(() => {
  setTimeout(() => {
    if (cellInputRef.value) {
      cellInputRef.value.focus();
    }
  }, 100);
});

function customRow(record: any) {
  return {
    onClick: () => selectStock(record),
  };
}

function selectStock(stock: any) {
  stockData.value = stock;
  outQty.value = 1;
  message.success('已选择物料');
}

async function scanCellCode() {
  if (!cellCode.value.trim()) {
    message.error('请输入库位码');
    return;
  }
  
  loading.value = true;
  
  try {
    const result = await stocksGetInCell(cellCode.value.trim());
    
    if (result && result.length > 0) {
      displayCellCode.value = cellCode.value.trim();
      if (result.length === 1) {
        stockData.value = result[0];
        stockList.value = [];
        outQty.value = 1;
        message.success('查询成功');
      } else {
        stockList.value = result;
        stockData.value = null;
        outQty.value = 1;
        message.success(`该库位共 ${result.length} 条库存，请选择出库物料`);
      }
    } else {
      message.warning('该库位未找到库存');
      stockData.value = null;
      stockList.value = [];
      displayCellCode.value = '';
    }
  } catch (error: any) {
    message.error(error?.error?.message || error?.message || '查询失败');
    stockData.value = null;
    stockList.value = [];
    displayCellCode.value = '';
  } finally {
    loading.value = false;
    cellCode.value = '';
  }
}

async function executeOut() {
  if (!canOut.value) {
    message.error('出库数量无效');
    return;
  }
  
  executing.value = true;
  
  try {
    const response = await stockService.stockOutboundDirect(stockData.value.id, outQty.value);
    
    if (response && response.success === true) {
      stockData.value.totalCountInTime -= outQty.value;
      
      showSuccess.value = true;
      message.success(`成功出库 ${outQty.value} ${stockData.value.unit}`);
      
      if (stockData.value.totalCountInTime <= 0) {
        setTimeout(() => {
          reset();
        }, 2000);
      } else {
        outQty.value = 1;
      }
      
      setTimeout(() => { showSuccess.value = false; }, 3000);
    } else {
      message.error(response?.message || '出库失败');
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
  outQty.value = 1;
  stockData.value = null;
  stockList.value = [];
}
</script>

<style scoped>
.page-container { 
  padding: 15px; 
  background: #f5f5f5; 
  min-height: 100vh; 
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

.stock-select-section {
  margin: 15px 0;
  background: #fff;
  border-radius: 8px;
  padding: 15px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.06);
}

.stock-select-section h3 {
  margin: 0 0 12px;
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

.info-card { 
  margin: 15px 0; 
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.06);
}

.info-card :deep(.ant-descriptions-item-label) {
  font-weight: 500;
  color: #666;
}

.info-card :deep(.ant-descriptions-item-content) {
  color: #333;
}

.qty-section { 
  margin: 15px 0; 
  background: #fff;
  border-radius: 8px;
  padding: 15px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.06);
}

.qty-row { 
  align-items: center; 
}

.qty-input { 
  width: 100%; 
  height: 40px;
  border-radius: 6px;
}

.qty-hint { 
  font-size: 12px; 
  color: #999; 
  margin-top: 8px; 
  text-align: right; 
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

:deep(.ant-table-tbody > tr) {
  cursor: pointer;
}
</style>
