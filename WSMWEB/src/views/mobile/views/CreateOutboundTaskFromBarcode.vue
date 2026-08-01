<template>
  <div class="page-container">
    <Header numb="创建出库任务">
      <template #action>
        <a-button
          size="small"
          :type="showAllStocks ? 'primary' : 'default'"
          @click="toggleShowAll"
        >
          {{ showAllStocks ? '只看未下任务' : '所有库存信息' }}
        </a-button>
      </template>
    </Header>

    <a-row class="input-row">
      <a-col :span="6">
        <div class="label">单据行条码:</div>
      </a-col>
      <a-col :span="17">
        <a-input v-model:value="barcode" placeholder="扫描单据行条码" @keyup.enter="scanBarcode" :allowClear="true" class="search-input" ref="barcodeInputRef" autofocus>
          <template #suffix>
            <scan-outlined class="icon" />
          </template>
        </a-input>
      </a-col>
    </a-row>

    <a-row class="button-row">
      <a-button type="primary" @click="scanBarcode" :loading="loading">查询库存</a-button>
    </a-row>

    <div v-if="parsed" class="parsed-card">
      <a-descriptions title="条码解析" :column="2" bordered size="small">
        <a-descriptions-item label="发货单号" :span="2"><b>{{ parsed.deliveryOrderNo }}</b></a-descriptions-item>
        <a-descriptions-item label="存货编码"><b>{{ parsed.materialCode }}</b></a-descriptions-item>
        <a-descriptions-item label="条码数量">{{ parsed.quantity }}</a-descriptions-item>
      </a-descriptions>
    </div>

    <div v-if="stockList.length > 0" class="stock-section">
      <h3>请选择出库物料 (共 {{ stockList.length }} 条库存)</h3>
      <div v-if="hiddenCount > 0" class="hidden-hint">已隐藏 {{ hiddenCount }} 条已下出库任务的库存</div>
      <a-table
        :dataSource="stockList"
        :columns="stockColumns"
        :pagination="false"
        :scroll="{ y: '220px' }"
        rowKey="id"
        :customRow="customRow"
        :rowClassName="(record) => selectedRowId === record.id ? 'selected-row' : ''"
      />
    </div>

    <div v-if="stockData" class="info-card">
      <a-descriptions :column="1" bordered size="small">
        <template #title>选中库存</template>
        <a-descriptions-item label="物料编号">{{ stockData.materialCode }}</a-descriptions-item>
        <a-descriptions-item label="物料名称">{{ stockData.materialName }}</a-descriptions-item>
        <a-descriptions-item label="库位">{{ stockData.cellCode }}</a-descriptions-item>
        <a-descriptions-item label="容器">{{ stockData.boxCode }}</a-descriptions-item>
        <a-descriptions-item label="等级">{{ stockData.grade || '无' }}</a-descriptions-item>
        <a-descriptions-item label="箱号">{{ stockData.processNo || '无'  }}</a-descriptions-item>
        <a-descriptions-item label="数量">{{ stockData.totalCountInTime }} {{ stockData.unit }}</a-descriptions-item>
        <a-descriptions-item label="批次号">{{ stockData.batchCode || '无' }}</a-descriptions-item>
      </a-descriptions>
    </div>

    <div v-if="stockData" class="qty-section">
      <a-row class="input-row">
        <a-col :span="8">
          <div class="label">出库库位:</div>
        </a-col>
        <a-col :span="16">
          <a-input v-model:value="outCellCode" placeholder="扫描出库库位" :allowClear="true" class="search-input" ref="outCellInputRef" @keyup.enter="focusQuantity" />
        </a-col>
      </a-row>
      <!-- <a-row class="input-row">
        <a-col :span="8">
          <div class="label">实际出库数量:</div>
        </a-col>
        <a-col :span="16">
          <a-input-number v-model:value="actualOutboundQuantity" :min="0.000001" :max="stockData.totalCountInTime" class="qty-input" :step="1" />
        </a-col>
      </a-row> -->
      <div class="qty-hint">
        当前库存: {{ stockData.totalCountInTime }} {{ stockData.unit }} | 条码数量: {{ parsed?.quantity ?? '-' }}
      </div>
    </div>

    <a-row v-if="stockData" class="button-row">
      <a-button type="primary" @click="createOutboundTask" :disabled="!canCreate" :loading="executing">创建出库任务</a-button>
      <a-button @click="reset">重置</a-button>
    </a-row>

    <div v-if="showSuccess" class="success-message">
      <a-alert type="success" message="成功" description="出库任务已创建" closable @close="showSuccess = false" />
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed, onMounted } from 'vue';
import { ScanOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
import Header from '../header/Header.vue';
import { StockServiceProxy } from '/@/services/ServiceProxies';
import { stocksQuery } from './Stock';
import { PagedStockQueryDto } from '/@/services/ServiceProxies';

const barcode = ref('');
const barcodeInputRef = ref<any>();
const outCellInputRef = ref<any>();
const loading = ref(false);
const executing = ref(false);
const showSuccess = ref(false);
const stockData = ref<any>(null);
const allStocks = ref<any[]>([]);
const showAllStocks = ref(false);
const selectedRowId = ref('');
const outCellCode = ref('');
const actualOutboundQuantity = ref<number | undefined>(undefined);
const parsedData = ref<ParsedBarcode | null>(null);

const stockList = computed<any[]>(() => {
  if (showAllStocks.value) return allStocks.value;
  return allStocks.value.filter((s) => !s.hasTask);
});

const hiddenCount = computed(() => allStocks.value.length - stockList.value.length);

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
    title: '库位',
    dataIndex: 'cellCode',
    key: 'cellCode',
    align: 'center',
    width: 80,
  },
  {
    title: '数量',
    dataIndex: 'totalCountInTime',
    key: 'totalCountInTime',
    align: 'center',
    width: 70,
  },
  {
    title: '批次号',
    dataIndex: 'batchCode',
    key: 'batchCode',
    align: 'center',
    width: 80,
  },
];

interface ParsedBarcode {
  raw: string;
  fields: string[];
  warehouseCode: string;
  customerCode: string;
  masterId: string;
  quantity: number;
  materialCode: string;
  packaging: string;
  grade: string;
  labelPrint: string;
  deliveryOrderNo: string;
  qtyPerBox: number;
}

const parsed = computed<ParsedBarcode | null>(() => {
  const v = barcode.value.trim();
  if (!v) return null;
  const parts = v.split('@');
  if (parts.length !== 10) return null;
  return {
    raw: v,
    fields: parts,
    warehouseCode: parts[0],
    customerCode: parts[1],
    masterId: parts[2],
    quantity: Number(parts[3]) || 0,
    materialCode: parts[4],
    packaging: parts[5],
    grade: parts[6],
    labelPrint: parts[7],
    deliveryOrderNo: parts[8],
    qtyPerBox: Number(parts[9]) || 0,
  };
});

const canCreate = computed(() => {
  if (!stockData.value) return false;
  if (!parsedData.value) return false;
  if (!outCellCode.value.trim()) return false;
  // if (!actualOutboundQuantity.value || actualOutboundQuantity.value <= 0) return false;
  // if (actualOutboundQuantity.value > stockData.value.totalCountInTime) return false;
  return true;
});

onMounted(() => {
  setTimeout(() => {
    if (barcodeInputRef.value) {
      barcodeInputRef.value.focus();
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
  selectedRowId.value = stock.id;
  const barcodeQty = parsedData.value?.quantity ?? 0;
  const avail = stock.totalCountInTime ?? 0;
  if (barcodeQty > 0) {
    actualOutboundQuantity.value = Math.min(barcodeQty, avail);
  } else {
    actualOutboundQuantity.value = undefined;
  }
  message.success('已选择物料');
  setTimeout(() => {
    if (outCellInputRef.value) {
      outCellInputRef.value.focus();
    }
  }, 100);
  barcode.value = '';
}

function focusQuantity() {
  const el = document.querySelector('.qty-input input') as any;
  if (el) el.focus();
}

async function scanBarcode() {
  const p = parsed.value;
  if (!p) {
    message.error('条码格式错误，需要10个@分隔的字段');
    return;
  }

  loading.value = true;
  parsedData.value = p;
  stockData.value = null;
  allStocks.value = [];
  selectedRowId.value = '';
  outCellCode.value = '';
  actualOutboundQuantity.value = p.quantity > 0 ? p.quantity : undefined;

  try {
    const params = new PagedStockQueryDto();
    params.materialCode = p.materialCode;
    const result = await stocksQuery(params);

    if (result && result.length > 0) {
      allStocks.value = result;
      const visible = stockList.value;
      const hidden = result.length - visible.length;
      if (visible.length > 0) {
        if (visible.length === 1) {
          selectStock(visible[0]);
        } else {
          message.success(`存货编码 ${p.materialCode} 共 ${visible.length} 条库存，请选择出库物料`);
        }
        if (hidden > 0) {
          message.warning(`已隐藏 ${hidden} 条已下出库任务的库存，点击右上角「所有库存信息」可查看`);
        }
      } else {
        message.warning(`存货编码 ${p.materialCode} 的库存均已下出库任务`);
      }
    } else {
      message.warning(`未找到存货编码 ${p.materialCode} 的库存`);
    }
  } catch (error: any) {
    message.error(error?.error?.message || error?.message || '查询失败');
  } finally {
    loading.value = false;
  }
}

function toggleShowAll() {
  showAllStocks.value = !showAllStocks.value;
  if (stockData.value) {
    stockData.value = null;
    selectedRowId.value = '';
    outCellCode.value = '';
    actualOutboundQuantity.value = undefined;
  }
}

async function createOutboundTask() {
  if (!canCreate.value) {
    message.error('请完善出库信息');
    return;
  }

  if (!stockData.value.boxCode) {
    message.error('该库存未绑定容器，无法创建搬运任务');
    return;
  }

  executing.value = true;

  try {
    const response = await stockService.createOutboundTaskFromBarcode(
      parsedData.value!.deliveryOrderNo,
      parsedData.value!.materialCode,
      actualOutboundQuantity.value!,
      stockData.value.boxCode,
      stockData.value.cellCode,
      outCellCode.value.trim()
    );

    if (response && response.success === true) {
      showSuccess.value = true;
      message.success('出库任务创建成功');
      setTimeout(() => { reset(); }, 2000);
      setTimeout(() => { showSuccess.value = false; }, 3000);
    } else {
      message.error(response?.message || '出库任务创建失败');
    }
  } catch (error: any) {
    message.error(error?.response?.data?.message || error?.message || '出库任务创建失败');
  } finally {
    executing.value = false;
  }
}

function reset() {
  barcode.value = '';
  parsedData.value = null;
  stockData.value = null;
  allStocks.value = [];
  showAllStocks.value = false;
  selectedRowId.value = '';
  outCellCode.value = '';
  actualOutboundQuantity.value = undefined;
}
</script>

<style scoped>
.page-container {
  padding: 15px;
  background: #f5f5f5;
  min-height: 100vh;
}

.input-row {
  margin-bottom: 10px;
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

.parsed-card {
  margin: 10px 0;
  background: #fff;
  border-radius: 8px;
  padding: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

.stock-section {
  margin: 15px 0;
  background: #fff;
  border-radius: 8px;
  padding: 15px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

.stock-section h3 {
  margin: 0 0 12px;
  font-size: 14px;
  font-weight: 500;
  color: #333;
}

.hidden-hint {
  font-size: 12px;
  color: #fa8c16;
  margin-bottom: 8px;
}

.info-card {
  margin: 15px 0;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
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
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

.qty-input {
  width: 100%;
  height: 36px;
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

.selected-row {
  background-color: #e6f7ff !important;
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
