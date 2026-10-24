<template>
  <div class="remainder-cabinet-page">
    <Header numb="零头柜录入" />
<!--    <p class="warehouse-label">4楼叉车库</p>-->
    <a-tabs v-model:activeKey="activeTab" centered size="small">
      <a-tab-pane key="deposit" tab="寄存产品" :disabled="operationLocked && activeTab !== 'deposit'">
        <RemainderCabinetProductOperation operation="deposit" @lock-change="operationLocked = $event" />
      </a-tab-pane>
      <a-tab-pane key="withdraw" tab="领用产品" :disabled="operationLocked && activeTab !== 'withdraw'">
        <RemainderCabinetProductOperation operation="withdraw" @lock-change="operationLocked = $event" />
      </a-tab-pane>
      <a-tab-pane key="history" tab="流水查询" :disabled="operationLocked && activeTab !== 'history'">
        <RemainderCabinetHistoryQuery />
      </a-tab-pane>
      <a-tab-pane key="stock" tab="库存查询" :disabled="operationLocked && activeTab !== 'stock'">
        <RemainderCabinetStockQuery />
      </a-tab-pane>
    </a-tabs>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import Header from '../../header/Header.vue';
import RemainderCabinetProductOperation from '../../components/RemainderCabinetProductOperation.vue';
import RemainderCabinetHistoryQuery from '../../components/RemainderCabinetHistoryQuery.vue';
import RemainderCabinetStockQuery from '../../components/RemainderCabinetStockQuery.vue';

/** 四个子页面直接显示；寄存、领用结果未确认时保持当前操作页，防止重复记账。 */
const activeTab = ref('deposit');
const operationLocked = ref(false);
</script>

<style scoped>
.remainder-cabinet-page { min-height: 100vh; background: #fff; }
.warehouse-label { margin: 10px 16px; color: #666; }
</style>
