<template>
  <div class="cabinet-query">
    <label>扫描存货编码或柜门</label>
    <a-input v-model:value="scanCode" placeholder="存货码、物料等级卡或柜门A15-08" allow-clear @pressEnter="search" />
    <label>货位（可选）</label>
    <a-input v-model:value="cellCode" placeholder="支持输入部分货位，例如A15" allow-clear @pressEnter="search" />
    <label>等级（可选）</label>
    <a-input v-model:value="grade" placeholder="支持等级片段" allow-clear @pressEnter="search" />
    <a-button type="primary" block :loading="loading" @click="search">查询库存</a-button>
    <a-alert v-if="error" type="error" :message="error" show-icon />
    <p v-if="queried">共 {{ total }} 条库存，按存货、等级、货位汇总。</p>
    <a-card v-for="row in rows" :key="row.id" size="small" class="stock-card" :title="`${row.materialCode} / ${row.grade || '-'}`">
      <p>{{ row.materialName || '-' }}</p>
      <p>货位：{{ row.cellCode }}</p>
      <p>当前库存：{{ row.quantity }} {{ row.unit || '' }}</p>
      <p>可领用数量：{{ row.availableQuantity }} {{ row.unit || '' }}</p>
    </a-card>
    <a-empty v-if="queried && !rows.length && !loading && !error" description="未查询到零头柜库存" />
    <div v-if="total > pageSize" class="pager">
      <a-button :disabled="loading || page === 1" @click="load(page - 1)">上一页</a-button>
      <span>第 {{ page }} / {{ Math.ceil(total / pageSize) }} 页</span>
      <a-button :disabled="loading || page * pageSize >= total" @click="load(page + 1)">下一页</a-button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { queryRemainderStocks } from '../views/RemainderCabinetApi';
import type { RemainderBalance, RemainderQuery } from '../views/RemainderCabinetApi';

const scanCode = ref('');
const cellCode = ref('');
const grade = ref('');
const rows = ref<RemainderBalance[]>([]);
const total = ref(0);
const page = ref(1);
const pageSize = 20;
const loading = ref(false);
const queried = ref(false);
const error = ref('');
/** 分页沿用上一次已提交条件，避免未点击查询时将新输入混入旧分页。 */
let filters: RemainderQuery = {};

/** 扫码后点击查询或按回车，空条件可查询所有零头柜库存。 */
function search() {
  if (loading.value) return;
  filters = { scanCode: scanCode.value.trim(), cellCode: cellCode.value.trim(), grade: grade.value.trim() };
  return load(1);
}

/** 只读分页查询，失败时清除旧结果，避免把过期库存当作当前结果。 */
async function load(target: number) {
  if (loading.value) return;
  loading.value = true; error.value = '';
  try {
    const result = await queryRemainderStocks({ ...filters, skipCount: (target - 1) * pageSize, maxResultCount: pageSize });
    rows.value = result.items || []; total.value = result.totalCount || 0; page.value = target; queried.value = true;
  } catch (cause: any) { rows.value = []; total.value = 0; error.value = cause?.message || '库存查询失败'; }
  finally { loading.value = false; }
}
</script>

<style scoped>
.cabinet-query { padding: 4px 16px 20px; }
label { display: block; font-weight: 600; margin: 14px 0 8px; }
.ant-btn, .ant-alert, .stock-card { margin-top: 12px; }
p { margin: 8px 0; }
.pager { display: flex; align-items: center; justify-content: space-between; gap: 8px; }
</style>
