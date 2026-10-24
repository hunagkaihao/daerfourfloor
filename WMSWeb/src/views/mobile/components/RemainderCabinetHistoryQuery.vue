<template>
  <div class="cabinet-query">
    <label>存货编码</label>
    <a-input v-model:value="materialCode" placeholder="输入全部或部分存货编码" allow-clear @pressEnter="search" />
    <label>货位</label>
    <a-input v-model:value="cellCode" placeholder="例如A、A15或A15-08" allow-clear @pressEnter="search" />
    <label>等级</label>
    <a-input v-model:value="grade" placeholder="输入全部或部分等级" allow-clear @pressEnter="search" />
    <label>日期</label>
    <a-input v-model:value="dateKeyword" placeholder="例如2026、2026-10或2026-10-24" allow-clear @pressEnter="search" />
    <a-button type="primary" block :loading="loading" @click="search">查询流水</a-button>
    <a-alert v-if="error" type="error" :message="error" show-icon />
    <p v-if="queried">共 {{ total }} 条寄存、领用流水。</p>
    <a-card v-for="row in rows" :key="row.id" size="small" class="history-card" :title="`${row.operation} / ${row.materialCode}`">
      <p>{{ row.materialName || '-' }}</p>
      <p>货位：{{ row.cellCode }}；等级：{{ row.grade || '-' }}</p>
      <p>{{ row.operation }}数量：{{ row.quantity }} {{ row.unit || '' }}</p>
      <p>时间：{{ displayRemainderTime(row.operationTime) }}</p>
      <p>操作人员：{{ row.operatorName || '-' }}</p>
    </a-card>
    <a-empty v-if="queried && !rows.length && !loading && !error" description="未查询到匹配流水" />
    <div v-if="total > pageSize" class="pager">
      <a-button :disabled="loading || page === 1" @click="load(page - 1)">上一页</a-button>
      <span>第 {{ page }} / {{ Math.ceil(total / pageSize) }} 页</span>
      <a-button :disabled="loading || page * pageSize >= total" @click="load(page + 1)">下一页</a-button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { queryRemainderHistories } from '../views/RemainderCabinetApi';
import type { RemainderHistory, RemainderQuery } from '../views/RemainderCabinetApi';
import { displayRemainderTime } from '../views/RemainderCabinetHelpers';

const materialCode = ref('');
const cellCode = ref('');
const grade = ref('');
const dateKeyword = ref('');
const rows = ref<RemainderHistory[]>([]);
const total = ref(0);
const page = ref(1);
const pageSize = 20;
const loading = ref(false);
const queried = ref(false);
const error = ref('');
/** 已确认的查询条件，翻页时沿用，不受尚未提交的输入影响。 */
let filters: RemainderQuery = {};

/** 四项条件均可选，组合后查询；文本按包含匹配，日期按年、月或日匹配。 */
function search() {
  if (loading.value) return;
  filters = { materialCode: materialCode.value.trim(), cellCode: cellCode.value.trim(), grade: grade.value.trim(), dateKeyword: dateKeyword.value.trim() };
  return load(1);
}

/** 分页读取统一入库和出库流水，不修改库存。 */
async function load(target: number) {
  if (loading.value) return;
  loading.value = true; error.value = '';
  try {
    const result = await queryRemainderHistories({ ...filters, skipCount: (target - 1) * pageSize, maxResultCount: pageSize });
    rows.value = result.items || []; total.value = result.totalCount || 0; page.value = target; queried.value = true;
  } catch (cause: any) { rows.value = []; total.value = 0; error.value = cause?.message || '流水查询失败'; }
  finally { loading.value = false; }
}
</script>

<style scoped>
.cabinet-query { padding: 4px 16px 20px; }
label { display: block; font-weight: 600; margin: 14px 0 8px; }
.ant-btn, .ant-alert, .history-card { margin-top: 12px; }
p { margin: 8px 0; }
.pager { display: flex; align-items: center; justify-content: space-between; gap: 8px; }
</style>
