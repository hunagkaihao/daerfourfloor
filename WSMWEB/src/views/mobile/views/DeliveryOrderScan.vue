<template>
  <div class="page-container">
    <Header numb="发货单扫码"></Header>
    
    <a-row class="input-row">
      <a-col :span="6">
        <div class="label">发货单号:</div>
      </a-col>
      <a-col :span="17">
        <a-input v-model:value="deliveryOrderNo" placeholder="扫描发货单号" @keyup.enter="scanDeliveryOrder" :allowClear="true" class="search-input" autofocus>
          <template #suffix>
            <search-outlined class="icon" />
          </template>
        </a-input>
      </a-col>
    </a-row>

    <a-row class="button-row">
      <a-button type="primary" @click="scanDeliveryOrder" :loading="loading">查询</a-button>
    </a-row>

    <div v-if="deliveryOrder" class="info-card">
      <a-descriptions title="发货单信息" :column="2" bordered size="small">
        <a-descriptions-item label="单号">{{ deliveryOrder.deliveryOrderNo }}</a-descriptions-item>
        <a-descriptions-item label="仓库">{{ deliveryOrder.warehouseName || deliveryOrder.warehouseCode }}</a-descriptions-item>
        <a-descriptions-item label="发货日期">{{ formatDate(deliveryOrder.deliveryDate) }}</a-descriptions-item>
        <a-descriptions-item label="状态">{{ getStatusText(deliveryOrder.status) }}</a-descriptions-item>
      </a-descriptions>
    </div>

    <div v-if="stockItems.length" class="stock-section">
      <h3>库存匹配结果</h3>
      <a-table :columns="stockColumns" :data-source="stockItems" :pagination="false" size="small">
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'select'">
            <a-checkbox 
              :checked="selectedStockIds.includes(record.id)" 
              @change="() => toggleSelect(record)"
              :disabled="record.status !== 0"
            />
          </template>
          <template v-if="column.key === 'action'">
            <a-button size="small" type="text" @click="scanBoxCode(record)">扫描箱号</a-button>
          </template>
        </template>
      </a-table>
    </div>

    <a-row v-if="deliveryOrder && deliveryOrder.status === 'Created'" class="button-row">
      <a-button type="primary" @click="executeDelivery" :disabled="selectedStockIds.length === 0" :loading="executing">执行发货</a-button>
      <a-button @click="reset">重置</a-button>
    </a-row>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed } from 'vue';
import { SearchOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
import Header from '../header/Header.vue';
import { ERP_Delivery_OrderServiceProxy, StockServiceProxy, PagedStockQueryDto } from '/@/services/ServiceProxies';
import moment from 'moment';

const deliveryOrderNo = ref('');
const loading = ref(false);
const executing = ref(false);
const deliveryOrder = ref<any>(null);
const stockItems = ref<any[]>([]);
const selectedStockIds = ref<string[]>([]);

const deliveryOrderService = new ERP_Delivery_OrderServiceProxy();
const stockService = new StockServiceProxy();

const stockColumns = [
  {
    title: '选择',
    key: 'select',
    width: 50,
    align: 'center',
  },
  { title: '物料编码', dataIndex: 'materialCode', key: 'materialCode' },
  { title: '物料名称', dataIndex: 'materialName', key: 'materialName' },
  { title: '规格', dataIndex: 'specs', key: 'specs' },
  { title: '批次号', dataIndex: 'batchCode', key: 'batchCode' },
  { title: '库存数量', dataIndex: 'totalCountInTime', key: 'totalCountInTime' },
  { title: '库位', dataIndex: 'cellCode', key: 'cellCode' },
  { title: '容器', dataIndex: 'boxCode', key: 'boxCode' },
  {
    title: '操作',
    key: 'action',
    width: 80,
    align: 'center',
  },
];

function formatDate(date: any): string {
  return date ? moment(date).format('YYYY-MM-DD') : '';
}

function getStatusText(status?: string): string {
  const statusMap: Record<string, string> = {
    'Created': '待发货',
    'Processing': '处理中',
    'Completed': '已完成',
    'Cancelled': '已取消'
  };
  return statusMap[status || ''] || '未知';
}

async function scanDeliveryOrder() {
  if (!deliveryOrderNo.value.trim()) {
    message.error('请输入发货单号');
    return;
  }
  
  loading.value = true;
  
  try {
    const result = await deliveryOrderService.list(1, 100, deliveryOrderNo.value, undefined, undefined, undefined);
    
    if (result && result.items && result.items.length > 0) {
      deliveryOrder.value = result.items[0];
      await findStockForDeliveryOrder(deliveryOrder.value);
      message.success('查询成功');
    } else {
      message.warning('未找到该发货单');
      deliveryOrder.value = null;
      stockItems.value = [];
    }
  } catch (error: any) {
    message.error(error?.response?.data?.message || '查询失败');
    deliveryOrder.value = null;
    stockItems.value = [];
  } finally {
    loading.value = false;
  }
}

async function findStockForDeliveryOrder(order: any) {
  if (!order.items || order.items.length === 0) {
    message.warning('发货单没有明细项');
    return;
  }
  
  stockItems.value = [];
  
  for (const item of order.items) {
    try {
      const queryDto = new PagedStockQueryDto();
      queryDto.materialCode = item.materialCode;
      queryDto.batchCode = item.batchCode;
      queryDto.page = 1;
      queryDto.pageSize = 50;
      
      const stockResult = await stockService.pagedStocksQuery(queryDto);
      
      if (stockResult && stockResult.items && stockResult.items.length > 0) {
        stockItems.value.push(...stockResult.items.map((stock: any) => ({
          ...stock,
          deliveryItemId: item.id,
          requiredQty: item.deliveryQuantity,
          matched: true
        })));
      } else {
        stockItems.value.push({
          id: item.id,
          materialCode: item.materialCode,
          materialName: item.materialName,
          specs: item.specs,
          batchCode: item.batchCode,
          totalCountInTime: 0,
          cellCode: '-',
          boxCode: '-',
          deliveryItemId: item.id,
          requiredQty: item.deliveryQuantity,
          matched: false,
          status: -1
        });
      }
    } catch (error) {
      console.error('查询库存失败:', error);
    }
  }
}

function toggleSelect(record: any) {
  const index = selectedStockIds.value.indexOf(record.id);
  if (index >= 0) {
    selectedStockIds.value.splice(index, 1);
  } else {
    selectedStockIds.value.push(record.id);
  }
}

function scanBoxCode(record: any) {
  const boxCode = prompt('请扫描箱号:', record.boxCode || '');
  if (boxCode) {
    record.boxCode = boxCode;
    message.success('箱号已更新');
  }
}

async function executeDelivery() {
  if (selectedStockIds.value.length === 0) {
    message.error('请选择要发货的库存');
    return;
  }
  
  executing.value = true;
  
  try {
    const selectedItems = stockItems.value.filter(s => selectedStockIds.value.includes(s.id));
    let successCount = 0;
    
    for (const item of selectedItems) {
      try {
        const response = await stockService.stockOutboundDirect(item.id, item.totalCountInTime);
        
        if (response && response.success === true) {
          successCount++;
          item.status = 1;
        }
      } catch {
        console.error('出库失败:', item);
      }
    }
    
    if (successCount === selectedItems.length) {
      await deliveryOrderService.complete(deliveryOrder.value.id);
      deliveryOrder.value.status = 'Completed';
      message.success('发货完成');
    } else {
      message.warning(`部分发货成功: ${successCount}/${selectedItems.length}`);
    }
    
    selectedStockIds.value = [];
  } catch (error: any) {
    message.error(error?.response?.data?.message || '发货失败');
  } finally {
    executing.value = false;
  }
}

function reset() {
  deliveryOrderNo.value = '';
  deliveryOrder.value = null;
  stockItems.value = [];
  selectedStockIds.value = [];
}
</script>

<style scoped>
.page-container { padding: 10px; background: #fff; min-height: 100vh; }
.input-row { margin: 10px 0; }
.label { text-align: center; line-height: 32px; font-weight: 500; }
.search-input { width: 100%; }
.icon { color: #1890ff; }
.button-row { display: flex; gap: 10px; padding: 10px 0; }
.button-row button { flex: 1; }
.info-card { margin: 10px 0; }
.stock-section { margin: 10px 0; }
.stock-section h3 { font-size: 14px; font-weight: 600; margin-bottom: 10px; }
</style>