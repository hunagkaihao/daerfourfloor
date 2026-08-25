<template>
  <div class="components-input-demo-presuffix">
    <Header numb="创建出库任务(汇总)"></Header>
    
    <a-row class="input-row">
      <a-col :span="9" :offset="1">
        <a-select v-model:value="findtype" class="modern-select" style="width: 90%;">
          <a-select-option value="materialCode">物料编号</a-select-option>
          <a-select-option value="cellCode">库位</a-select-option>
          <a-select-option value="barcode">收料码</a-select-option>
          <a-select-option value="batchCode">批次号</a-select-option>
        </a-select>
      </a-col>
      <a-col :span="13">
        <a-input v-model:value="fliter" placeholder="扫描或输入查询条件" @keyup.enter="scancellCode" ref="focus1"
          :allowClear="true" @focus="focusFn" class="modern-input">
          <template #suffix>
            <scan-outlined class="scan-icon" />
          </template>
        </a-input>
      </a-col>
    </a-row>

    <a-row style="margin-top: 10px;">
      <a-col :span="6">
        <div class="htext">
          <h1>出库库位:</h1>
        </div>
      </a-col>
      <a-col :span="17">
        <a-input v-model:value="outCellCode" placeholder="出库库位" :allowClear="true"
          @focus="focusFn" class="modern-input">
        </a-input>
      </a-col>
    </a-row>

    <div v-if="summaryData.length > 0" class="summary-section">
      <a-table 
        :dataSource="summaryData" 
        :columns="summaryColumns" 
        :pagination="false"
        :scroll="{ x: screenWidth, y: '400px' }"
        rowKey="key"
        :row-selection="rowSelection"
      >
      </a-table>
    </div>

    <div v-show="showtable">
      <div class="tab-bar">
        <a-button @click="resetSelection" type="default" class="modern-btn">
          重置选择
        </a-button>
        <a-button @click="selectAll" type="default" class="modern-btn">
          全选
        </a-button>
        <a-button @click="createOutStockTask" type="primary" class="modern-btn">
          创建出库任务
        </a-button>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, computed } from 'vue';
import { ScanOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
import { stocksQuery, createStockTask } from './Stock';
import { PagedStockQueryDto } from '/@/services/ServiceProxies';
import Header from '../header/Header.vue';

const summaryColumns = [
  {
    title: '物料编号',
    dataIndex: 'materialCode',
    key: 'materialCode',
    align: 'center',
  },
  {
    title: '物料名称',
    dataIndex: 'materialName',
    key: 'materialName',
    align: 'center',
  },
  {
    title: '批次号',
    dataIndex: 'batchCode',
    key: 'batchCode',
    align: 'center',
  },
  {
    title: '库位',
    dataIndex: 'cellCode',
    key: 'cellCode',
    align: 'center',
  },
  {
    title: '数量',
    dataIndex: 'totalCountInTime',
    key: 'totalCountInTime',
    align: 'center',
  },
];

const focus1 = ref<any>();
const fliter = ref<string>('');
const outCellCode = ref<string>('');
const findtype = ref<string>('barcode');
const screenWidth = ref((window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth) - 8);
const showtable = ref(true);

const summaryData = ref<any[]>([]);
const selectedRowKeys = ref<string[]>([]);

const rowSelection = computed(() => ({
  type: 'checkbox',
  selectedRowKeys: selectedRowKeys.value,
  onChange: (selectedKeys: string[], selectedRows: any[]) => {
    selectedRowKeys.value = selectedKeys;
    if (selectedRows.length > 0) {
      message.info(`已选择 ${selectedRows.length} 个物料`);
    }
  }
}));

onMounted(() => {
  setTimeout(() => {
    if (focus1.value) {
      focus1.value.focus();
    }
  }, 100);
});

const scancellCode = async () => {
  if (!fliter.value.trim()) {
    message.error('请输入查询条件');
    return;
  }

  let queryValue = fliter.value;
  
  if (queryValue.includes('@')) {
    const parts = queryValue.split('@');
    if (parts.length >= 5) {
      queryValue = parts[4];
    }
  }
  
  if (queryValue.includes(',')) {
    const parts = queryValue.split(',');
    if (parts.length > 0) {
      queryValue = parts[0];
    }
  }
  
  var params = new PagedStockQueryDto();
  if (findtype.value == "barcode") {
    params.barcode = queryValue;
    params.receivingMaterialBarcode = queryValue;
  }
  if (findtype.value == "cellCode") {
    params.cellCode = fliter.value;
  }
  if (findtype.value == "materialCode") {
    params.materialCode = queryValue;
  }
  if (findtype.value == "batchCode") {
    params.batchCode = fliter.value;
  }
  
  await stocksQuery(params).then((res) => {
    summaryData.value = generateSummary(res);
    showtable.value = res.length > 0;
  }).catch((err) => {
    message.error(err.error?.message || '查询失败');
  });
};

const generateSummary = (stockList) => {
  const summaryMap = new Map();
  
  stockList.forEach(item => {
    const key = `${item.materialCode}-${item.batchCode}-${item.cellCode}`;
    if (!summaryMap.has(key)) {
      summaryMap.set(key, {
        key: key,
        materialCode: item.materialCode,
        materialName: item.materialName,
        specs: item.specs,
        batchCode: item.batchCode || '-',
        cellCode: item.cellCode || '-',
        boxNumber: item.boxNumber || '-',
        totalCountInTime: 0,
        stocks: [],
      });
    }
    const summary = summaryMap.get(key);
    summary.totalCountInTime += item.totalCountInTime;
    summary.stocks.push(item);
  });
  
  return Array.from(summaryMap.values());
};

const selectAll = () => {
  selectedRowKeys.value = summaryData.value.map(item => item.key);
};

const resetSelection = () => {
  selectedRowKeys.value = [];
};

const createOutStockTask = async () => {
  const selectedItems = summaryData.value.filter(item => selectedRowKeys.value.includes(item.key));
  
  if (selectedItems.length === 0) {
    message.error("请选择至少一个物料");
    return;
  }
  if (!outCellCode.value) {
    message.error("请输入出库库位");
    return;
  }
  
  let successCount = 0;
  let failCount = 0;
  
  for (const item of selectedItems) {
    for (const stock of item.stocks) {
      try {
        await createStockTask(stock.boxCode, stock.cellCode, outCellCode.value).then((res) => {
          if (res.success) {
            successCount++;
          } else {
            failCount++;
          }
        }).catch(() => {
          failCount++;
        });
      } catch {
        failCount++;
      }
    }
  }
  
  if (successCount > 0) {
    message.success(`成功创建 ${successCount} 个出库任务`);
  }
  if (failCount > 0) {
    message.warning(`有 ${failCount} 个出库任务创建失败`);
  }
  
  fliter.value = '';
  outCellCode.value = '';
  summaryData.value = [];
  selectedRowKeys.value = [];
  showtable.value = false;
};

function focusFn(e) {
  e.target.setAttribute('readonly', 'readonly');
  setTimeout(() => {
    e.target.removeAttribute('readonly');
  }, 200);
}
</script>

<style scoped lang="less">
.input-row {
  margin: 10px 0;
  padding: 0 16px;
}

.htext {
  text-align: center;
  line-height: 32px;
  
  h1 {
    color: #333333;
    font-size: 14px;
    font-weight: 500;
    margin: 0;
    letter-spacing: 0.3px;
  }
}

.modern-input {
  height: 32px;
  border-radius: 6px !important;
  border: 1px solid #d9d9d9 !important;
  background: #ffffff !important;
  transition: all 0.2s ease !important;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1) !important;
  
  &:focus,
  &:hover {
    border-color: #1890ff !important;
    box-shadow: 0 1px 6px rgba(24, 144, 255, 0.2) !important;
  }
  
  &::placeholder {
    color: rgba(0, 0, 0, 0.45) !important;
    font-weight: 400;
  }
}

.modern-select {
  height: 32px;
  border-radius: 6px !important;
  border: 1px solid #d9d9d9 !important;
  background: #ffffff !important;
  transition: all 0.2s ease !important;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1) !important;
  
  &:focus,
  &:hover {
    border-color: #1890ff !important;
    box-shadow: 0 1px 6px rgba(24, 144, 255, 0.2) !important;
  }
}

.modern-btn {
  flex: 1;
  margin: 0 4px;
  height: 32px !important;
  border-radius: 6px !important;
  font-size: 13px !important;
  font-weight: 500 !important;
  transition: all 0.2s ease !important;
}

.scan-icon {
  color: #1890ff !important;
  font-size: 18px;
  transition: all 0.2s ease;
  
  &:hover {
    color: #40a9ff !important;
  }
}

.tab-bar {
  display: flex;
  align-items: center;
  position: fixed;
  left: 0;
  right: 0;
  bottom: 0;
  height: 60px;
  background: #ffffff;
  border-top: 1px solid #f0f0f0;
  padding: 0 16px;
}

.summary-section {
  margin-top: 10px;
}

::v-deep(.ant-table-thead > tr > th) {
  padding: 5px 0px;
}

::v-deep(.ant-table-tbody > tr > td) {
  padding: 5px 0px;
}

p {
  margin-bottom: 0em
}
</style>
