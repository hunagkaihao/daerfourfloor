<template>
  <div class="page-container">
    <Header numb="出库单出库"></Header>
    
    <a-row class="input-row">
      <a-col :span="6">
        <div class="label">出库单号:</div>
      </a-col>
      <a-col :span="17">
        <a-input v-model:value="orderCode" placeholder="扫描出库单号" @keyup.enter="scanOrder" :allowClear="true" class="search-input" autofocus>
          <template #suffix>
            <search-outlined class="icon" />
          </template>
        </a-input>
      </a-col>
    </a-row>

    <a-row class="button-row">
      <a-button type="primary" @click="scanOrder" :loading="loading">查询</a-button>
    </a-row>

    <div v-if="orderData" class="info-card">
      <a-descriptions title="出库单信息" :column="2" bordered size="small">
        <a-descriptions-item label="单号">{{ orderData.outboundOrderNo }}</a-descriptions-item>
        <a-descriptions-item label="类型">{{ getOrderType(orderData.outboundReason) }}</a-descriptions-item>
        <a-descriptions-item label="仓库">{{ orderData.warehouseCode }}</a-descriptions-item>
        <a-descriptions-item label="状态">{{ getStatusText(orderData.status) }}</a-descriptions-item>
      </a-descriptions>
    </div>

    <div v-if="items.length" class="items-table">
      <h3>出库明细</h3>
      <a-table :columns="columns" :data-source="items" :pagination="false" size="small" />
    </div>

    <a-row v-if="orderData && orderData.status === 0" class="button-row">
      <a-button type="primary" @click="executeOut" :loading="executing">执行出库</a-button>
      <a-button @click="printOrder">打印</a-button>
    </a-row>
  </div>
</template>

<script lang="ts" setup>
import { ref } from 'vue';
import { SearchOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
import Header from '../header/Header.vue';
import { ErpOutboundOrderServiceProxy } from '/@/services/ServiceProxies';

const orderCode = ref('');
const loading = ref(false);
const executing = ref(false);
const orderData = ref<any>(null);

const outboundOrderService = new ErpOutboundOrderServiceProxy();

const columns = [
  { title: '物料编码', dataIndex: 'materialCode' },
  { title: '物料名称', dataIndex: 'materialName' },
  { title: '计划数量', dataIndex: 'planOutboundQty' },
  { title: '实际数量', dataIndex: 'actualOutboundQty' },
  { title: '批次号', dataIndex: 'lotNo' },
];

const items = ref<any[]>([]);

function getStatusText(status: number): string {
  const statusMap: Record<number, string> = {
    0: '待处理',
    1: '已完成',
    2: '已取消'
  };
  return statusMap[status] || '未知';
}

function getOrderType(reason?: string): string {
  if (!reason) return '销售出库';
  return reason;
}

async function scanOrder() {
  if (!orderCode.value.trim()) {
    message.error('请输入出库单号');
    return;
  }
  
  loading.value = true;
  
  try {
    const result = await outboundOrderService.byOrderNo(orderCode.value);
    
    if (result) {
      orderData.value = result;
      items.value = result.outboundItems || [];
      message.success('查询成功');
    } else {
      message.warning('未找到该出库单');
      orderData.value = null;
      items.value = [];
    }
  } catch (error: any) {
    message.error(error?.response?.data?.message || '查询失败');
    orderData.value = null;
    items.value = [];
  } finally {
    loading.value = false;
  }
}

async function executeOut() {
  if (!orderData.value) return;
  
  executing.value = true;
  
  try {
    const result = await outboundOrderService.status(orderData.value.id, 1);
    
    if (result) {
      message.success('出库成功');
      orderData.value.status = 1;
    } else {
      message.error('出库失败');
    }
  } catch (error: any) {
    message.error(error?.response?.data?.message || '出库失败');
  } finally {
    executing.value = false;
  }
}

function printOrder() {
  message.info('打印功能开发中');
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
.items-table { margin: 10px 0; }
.items-table h3 { font-size: 14px; font-weight: 600; margin-bottom: 10px; }
</style>