<template>
  <div class="cabinet-query">
    <label>存货编码</label>
    <a-input v-model:value="materialCode" placeholder="输入全部或部分存货编码，也可扫描物料卡" allow-clear @pressEnter="search" />
    <label>批次号（寄存）</label>
    <a-input v-model:value="batchCode" placeholder="输入全部或部分批次号，也可扫描箱码自动带入" allow-clear @pressEnter="search" />
    <label>货位</label>
    <a-select :value="cellCode || undefined" :options="remainderCellOptions" show-search allow-clear
      option-filter-prop="label" placeholder="全部货位，可搜索A15-08" style="width: 100%" @change="cellCode = $event || ''" />
    <label>日期</label>
    <a-date-picker :value="dateKeyword || undefined" format="YYYY-MM-DD" value-format="YYYY-MM-DD"
      :input-read-only="true" allow-clear placeholder="请选择日期，留空查询全部" style="width: 100%"
      @update:value="dateKeyword = $event || ''" />
    <a-button type="primary" block :loading="loading" @click="search">查询流水</a-button>
    <a-alert v-if="error" type="error" :message="error" show-icon />
    <p v-if="queried">共 {{ total }} 条寄存、领用流水。</p>
    <a-card v-for="row in rows" :key="row.id" size="small" class="history-card"
      :title="`${row.operation} / ${row.materialCode}`" :head-style="historyHeaderStyles[row.operation]">
      <p>{{ row.materialName || '-' }}</p>
      <p>货位：{{ row.cellCode }}；等级：{{ row.grade || '-' }}</p>
      <p v-if="row.operation === '寄存'">批次号：{{ row.batchCode || '-' }}</p>
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
import type { CSSProperties } from 'vue';
import { queryRemainderHistories } from '../views/RemainderCabinetApi';
import type { RemainderHistory, RemainderQuery } from '../views/RemainderCabinetApi';
import { displayRemainderTime, normalizeRemainderHistoryMaterial, remainderCellOptions } from '../views/RemainderCabinetHelpers';

const materialCode = ref('');
/** 仅筛选寄存流水中的箱码批次，不影响领用与库存余额。 */
const batchCode = ref('');
const cellCode = ref('');
/** 日历选择结果以本地日期字符串传入后端，清空时取消日期过滤。 */
const dateKeyword = ref('');
/** 寄存表头为绿色、领用表头为蓝色，保留操作文字以辅助识别。 */
const historyHeaderStyles: Record<string, CSSProperties> = {
  寄存: { backgroundColor: '#f6ffed', color: '#389e0d', borderBottomColor: '#b7eb8f' },
  领用: { backgroundColor: '#e6f7ff', color: '#096dd9', borderBottomColor: '#91d5ff' },
};
const rows = ref<RemainderHistory[]>([]);
const total = ref(0);
const page = ref(1);
const pageSize = 20;
const loading = ref(false);
const queried = ref(false);
const error = ref('');
/** 已确认的查询条件，翻页时沿用，不受尚未提交的输入影响。 */
let filters: RemainderQuery = {};

/** 存货、货位和日期均可选，组合后查询；所选日期覆盖当天全部流水。 */
function search() {
  if (loading.value) return;
  try {
    // 查询按钮与扫码回车共用编码提取，避免把等级等卡码内容一起传给存货条件。
    const code = normalizeRemainderHistoryMaterial(materialCode.value);
    if (materialCode.value.includes(',')) batchCode.value = materialCode.value.split(',')[1]?.trim() || '';
    materialCode.value = code;
    filters = { materialCode: code, batchCode: batchCode.value.trim(), cellCode: cellCode.value.trim().toUpperCase(), dateKeyword: dateKeyword.value };
  } catch (cause: any) {
    rows.value = []; total.value = 0;
    error.value = cause?.message || '请提供有效的存货编码';
    return;
  }
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
