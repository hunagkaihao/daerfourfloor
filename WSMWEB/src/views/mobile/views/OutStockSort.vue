﻿﻿﻿<template>
  <div class="page-container">
    <Header numb="出库分拣"></Header>
    
    <a-row class="input-row">
      <a-col :span="6">
        <div class="label">分拣单号:</div>
      </a-col>
      <a-col :span="17">
        <a-input v-model:value="sortCode" placeholder="扫描分拣单号" @keyup.enter="scanSort" :allowClear="true" class="search-input" autofocus>
          <template #suffix>
            <search-outlined class="icon" />
          </template>
        </a-input>
      </a-col>
    </a-row>

    <a-row class="button-row">
      <a-button type="primary" @click="scanSort" :loading="loading">查询</a-button>
    </a-row>

    <div v-if="sortData" class="info-card">
      <a-descriptions title="分拣单信息" :column="2" bordered size="small">
        <a-descriptions-item label="单号">{{ sortData.ccode }}</a-descriptions-item>
        <a-descriptions-item label="关联出库单">{{ sortData.orderCode }}</a-descriptions-item>
        <a-descriptions-item label="仓库">{{ sortData.warehouse }}</a-descriptions-item>
        <a-descriptions-item label="状态">{{ sortData.status }}</a-descriptions-item>
      </a-descriptions>
    </div>

    <div v-if="items.length" class="items-table">
      <h3>分拣明细</h3>
      <a-table :columns="columns" :data-source="items" :pagination="false" size="small">
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'action'">
            <a-button v-if="record.status === '未分拣'" type="text" @click="doSort(record)">确认</a-button>
            <span v-else>已分拣</span>
          </template>
        </template>
      </a-table>
    </div>

    <a-row v-if="sortData" class="button-row">
      <a-button type="primary" @click="completeSort">完成分拣</a-button>
      <a-button @click="printSort">打印</a-button>
    </a-row>
  </div>
</template>

<script lang="ts" setup>
import { ref } from 'vue';
import { SearchOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
import Header from '../header/Header.vue';

const sortCode = ref('');
const loading = ref(false);
const sortData = ref<any>(null);

const columns = [
  { title: '物料编码', dataIndex: 'code' },
  { title: '物料名称', dataIndex: 'name' },
  { title: '数量', dataIndex: 'qty' },
  { title: '状态', dataIndex: 'status' },
  { title: '操作', key: 'action' },
];

const items = ref([
  { code: 'INV001', name: '产品A', qty: 10, status: '未分拣' },
  { code: 'INV002', name: '产品B', qty: 20, status: '已分拣' },
]);

async function scanSort() {
  if (!sortCode.value.trim()) {
    message.error('请输入分拣单号');
    return;
  }
  loading.value = true;
  await new Promise(r => setTimeout(r, 500));
  sortData.value = {
    ccode: sortCode.value,
    orderCode: 'OUT001',
    warehouse: '成品仓',
    status: '分拣中'
  };
  loading.value = false;
}

function doSort(record: any) {
  record.status = '已分拣';
  message.success('分拣成功');
}

function completeSort() {
  message.success('分拣完成');
}

function printSort() {
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