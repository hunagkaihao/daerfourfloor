<template>
  <div class="page-container">
    <Header numb="库存查询"></Header>
    
    <a-row class="input-row">
      <a-col :span="6">
        <div class="label">物料编号:</div>
      </a-col>
      <a-col :span="17">
        <a-input
          v-model:value="queryForm.materialCode"
          placeholder="输入物料编号"
          :allowClear="true"
          class="search-input"
          @keyup.enter="handleSearch"
        >
          <template #suffix>
            <search-outlined class="icon" />
          </template>
        </a-input>
      </a-col>
    </a-row>

    <a-row class="input-row">
      <a-col :span="6">
        <div class="label">批次号:</div>
      </a-col>
      <a-col :span="17">
        <a-input
          v-model:value="queryForm.batchCode"
          placeholder="输入生产批次"
          :allowClear="true"
          class="search-input"
          @keyup.enter="handleSearch"
        >
          <template #suffix>
            <search-outlined class="icon" />
          </template>
        </a-input>
      </a-col>
    </a-row>

    <a-row class="input-row">
      <a-col :span="6">
        <div class="label">库位编码:</div>
      </a-col>
      <a-col :span="17">
        <a-input
          v-model:value="queryForm.cellCode"
          placeholder="输入库位编码"
          :allowClear="true"
          class="search-input"
          @keyup.enter="handleSearch"
        >
          <template #suffix>
            <search-outlined class="icon" />
          </template>
        </a-input>
      </a-col>
    </a-row>

    <a-row class="button-row">
      <a-button type="primary" @click="handleSearch" :loading="loading">查询</a-button>
      <a-button @click="resetForm" :disabled="loading">重置</a-button>
    </a-row>

    <div v-if="hasQueried && stockList.length > 0" class="result-summary">
      共 {{ stockList.length }} 条库存
    </div>

    <div v-if="stockList.length > 0" class="table-container">
      <a-table
        :data-source="stockList"
        :columns="columns"
        :pagination="false"
        :scroll="{ y: '420px' }"
        row-key="id"
        size="small"
      />
    </div>

    <div v-if="hasQueried && !loading && stockList.length === 0" class="empty-state">
      <a-empty description="暂无库存数据" />
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, reactive } from 'vue';
import { SearchOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
import Header from '../header/Header.vue';
import { PagedStockQueryDto } from '/@/services/ServiceProxies';
import { stocksQuery, stocksGetInCell } from './Stock';

const queryForm = reactive({
  materialCode: '',
  batchCode: '',
  cellCode: '',
});

const stockList = ref<any[]>([]);
const loading = ref(false);
const hasQueried = ref(false);

const columns = [
  {
    title: '物料编号',
    dataIndex: 'materialCode',
    key: 'materialCode',
    width: 90,
    align: 'center',
  },
  {
    title: '物料名称',
    dataIndex: 'materialName',
    key: 'materialName',
    width: 150,
    align: 'left',
  },
  {
    title: '数量',
    dataIndex: 'totalCountInTime',
    key: 'totalCountInTime',
    width: 70,
    align: 'center',
  },
  {
    title: '批次号',
    dataIndex: 'batchCode',
    key: 'batchCode',
    width: 80,
    align: 'center',
  },
  {
    title: '库位',
    dataIndex: 'cellCode',
    key: 'cellCode',
    width: 90,
    align: 'center',
  },
];

function toList(result: any): any[] {
  if (!result) return [];
  if (Array.isArray(result)) return result;
  if (Array.isArray(result.items)) return result.items;
  if (Array.isArray(result.Items)) return result.Items;
  return [];
}

function mapStock(item: any, index: number) {
  return {
    id: item.id || item.Id || `stock-${index}`,
    materialCode: item.materialCode || item.MaterialCode || '-',
    materialName: item.materialName || item.MaterialName || '-',
    totalCountInTime: item.totalCountInTime ?? item.TotalCountInTime ?? 0,
    batchCode: item.batchCode || item.BatchCode || '-',
    cellCode: item.cellCode || item.CellCode || '-',
  };
}

async function queryStocks() {
  const materialCode = queryForm.materialCode.trim();
  const batchCode = queryForm.batchCode.trim();
  const cellCode = queryForm.cellCode.trim();

  if (!materialCode && !batchCode && !cellCode) {
    message.warning('请至少输入一个查询条件');
    return;
  }

  loading.value = true;
  hasQueried.value = true;
  stockList.value = [];

  try {
    let items: any[] = [];

    // 仅库位查询时走专用接口，更快更稳定
    if (cellCode && !materialCode && !batchCode) {
      items = toList(await stocksGetInCell(cellCode));
    } else {
      const queryDto = new PagedStockQueryDto();
      queryDto.materialCode = materialCode || undefined;
      queryDto.cellCode = cellCode || undefined;
      queryDto.pageIndex = 1;
      queryDto.pageSize = 50;
      items = toList(await stocksQuery(queryDto));
    }

    items = items.map(mapStock);

    if (batchCode) {
      items = items.filter((item) => String(item.batchCode).includes(batchCode));
    }

    stockList.value = items;

    if (items.length === 0) {
      message.info('未查询到库存数据');
    }
  } catch (error: any) {
    stockList.value = [];
    message.error(error?.error?.message || error?.message || '查询失败');
  } finally {
    loading.value = false;
  }
}

function handleSearch() {
  queryStocks();
}

function resetForm() {
  queryForm.materialCode = '';
  queryForm.batchCode = '';
  queryForm.cellCode = '';
  stockList.value = [];
  hasQueried.value = false;
}
</script>

<style scoped>
.page-container {
  padding: 15px;
  background: #f5f5f5;
  min-height: 100vh;
}

.input-row {
  margin-bottom: 12px;
}

.label {
  text-align: right;
  line-height: 36px;
  font-weight: 500;
  color: #666;
  font-size: 14px;
  padding-right: 8px;
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
}

.result-summary {
  margin: 8px 0 12px;
  padding: 8px 12px;
  background: #e6f7ff;
  border-radius: 6px;
  color: #1890ff;
  font-size: 13px;
  font-weight: 500;
}

.table-container {
  background: #fff;
  border-radius: 8px;
  padding: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

.empty-state {
  margin-top: 50px;
  text-align: center;
}

:deep(.ant-table-thead > tr > th) {
  padding: 8px 6px;
  font-size: 12px;
  font-weight: 500;
  background: #f8f9fa;
}

:deep(.ant-table-tbody > tr > td) {
  padding: 8px 6px;
  font-size: 12px;
  white-space: normal;
  word-break: break-word;
  line-height: 1.4;
}
</style>
