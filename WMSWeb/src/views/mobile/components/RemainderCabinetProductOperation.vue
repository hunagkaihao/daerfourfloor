<template>
  <div class="product-operation">
    <a-alert v-if="uncertain" type="warning" show-icon message="上次操作结果未确认，请重试原请求；存货、等级、柜位和数量暂时锁定。" />
    <label>扫描柜门上的存货编码</label>
    <a-input v-model:value="scanCode" :disabled="locked" placeholder="存货码、622550001,,,Z1,,,或柜门A15-08"
      allow-clear @pressEnter="recognize" />
    <p>存货编码：{{ materialCode || '-' }}</p>
    <label>等级</label>
    <a-input v-model:value="grade" :disabled="locked" placeholder="卡码自动识别等级；纯存货码可手动填写" />
    <label>零头柜位</label>
    <a-select :value="cellCode || undefined" :options="remainderCellOptions" :disabled="locked" show-search
      option-filter-prop="label" placeholder="请选择实际柜位，可搜索A15-08" style="width: 100%" @change="selectCell" />
    <a-alert v-if="cabinet" show-icon :type="cabinet.canStore || operation === 'withdraw' ? 'info' : 'warning'"
      :message="`${cabinet.warehouseName} / ${cabinet.cellCode}；固定存货：${cabinet.materialCode || '首次寄存绑定'}；等级：${cabinet.grade || '-'}`" />
    <p v-if="cabinet">当前库存：{{ currentQuantity }}；可领用：{{ availableQuantity }}</p>
    <label>{{ operationName }}数量</label>
    <a-input-number v-model:value="quantity" :disabled="locked" :min="0" :precision="6"
      :max="operation === 'withdraw' ? availableQuantity : undefined" placeholder="输入实际数量" style="width: 100%" />
    <a-alert v-if="notice" :type="noticeType" :message="notice" show-icon />
    <a-button type="primary" block :loading="busy" :disabled="busy" @click="submit">
      {{ uncertain ? '重试确认原请求' : `确认${operationName}` }}
    </a-button>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import { depositRemainder, getRemainderCabinet, queryRemainderStocks, withdrawRemainder } from '../views/RemainderCabinetApi';
import type { RemainderCabinet, RemainderProductOperation } from '../views/RemainderCabinetApi';
import { newRemainderRequestId, parseRemainderScan, remainderCellOptions } from '../views/RemainderCabinetHelpers';

const props = defineProps<{ operation: 'deposit' | 'withdraw' }>();
const emit = defineEmits<{ (event: 'lock-change', locked: boolean): void }>();
const operationName = computed(() => props.operation === 'deposit' ? '寄存' : '领用');
/** 寄存与领用分别保存原请求，页面刷新后仍能核实结果。 */
const pendingKey = `wms:remainder-cabinet:${props.operation}:pending:v1`;
const scanCode = ref('');
const grade = ref('');
const cellCode = ref('');
const quantity = ref<number | null>(null);
const cabinet = ref<RemainderCabinet>();
const busy = ref(false);
const uncertain = ref(false);
const locked = computed(() => busy.value || uncertain.value);
const pending = ref<RemainderProductOperation>();
const requestId = ref('');
const notice = ref('');
const noticeType = ref<'info' | 'success' | 'warning' | 'error'>('info');
const parsed = computed(() => {
  try { return parseRemainderScan(scanCode.value); } catch { return null; }
});
const materialCode = computed(() => parsed.value?.cellCode ? cabinet.value?.materialCode || '' : parsed.value?.materialCode || '');
const currentQuantity = computed(() => Number((cabinet.value?.stocks || []).reduce((sum, stock) => sum + Number(stock.quantity), 0).toFixed(6)));
const availableQuantity = computed(() => Number((cabinet.value?.stocks || []).reduce((sum, stock) => sum + (stock.canWithdraw ? Number(stock.quantity) : 0), 0).toFixed(6)));

watch(locked, value => emit('lock-change', value), { immediate: true });
watch(scanCode, () => {
  if (locked.value) return;
  grade.value = parsed.value?.grade || (cabinet.value?.materialCode === materialCode.value ? cabinet.value?.grade || '' : '');
  quantity.value = null;
  notice.value = '';
});

/** 读取实际柜位；过期响应不能覆盖用户刚选择的新柜位。 */
async function loadCell() {
  const code = cellCode.value;
  if (!code) return;
  try {
    const result = await getRemainderCabinet(code);
    if (code !== cellCode.value) return;
    cabinet.value = result;
    if (!locked.value && !grade.value) grade.value = parsed.value?.grade || result.grade || '';
  } catch (error: any) {
    if (code !== cellCode.value) return;
    cabinet.value = undefined;
    notice.value = error?.message || '查询柜位失败'; noticeType.value = 'error';
  }
}

/** 下拉选择后自动读取固定存货、等级及当前数量。 */
async function selectCell(value?: string) {
  cellCode.value = value || '';
  cabinet.value = undefined;
  if (cellCode.value) await loadCell();
}

/** 唯一库存柜位时自动带入；存在多个位置时让操作员选择实际格位。 */
async function recognize() {
  if (locked.value) return;
  const raw = scanCode.value.trim();
  try {
    const scan = parseRemainderScan(raw);
    if (scan.cellCode) {
      await selectCell(scan.cellCode);
      if (!grade.value) grade.value = cabinet.value?.grade || '';
      return;
    }
    const result = await queryRemainderStocks({ scanCode: raw, maxResultCount: 200 });
    if (raw !== scanCode.value.trim() || locked.value) return;
    if (result.totalCount === 1) {
      await selectCell(result.items[0].cellCode);
      if (raw !== scanCode.value.trim() || locked.value) return;
      if (!grade.value) grade.value = result.items[0].grade;
    }
    notice.value = result.totalCount > 1 ? `匹配${result.totalCount}个柜位，请选择本次实际操作的柜位`
      : result.totalCount === 0 ? (props.operation === 'deposit' ? '尚无该存货库存，请选择柜位并填写等级后寄存' : '未查询到该存货库存')
      : '已识别存货及柜位，请填写实际数量';
    noticeType.value = 'info';
  } catch (error: any) { notice.value = error?.message || '识别存货失败'; noticeType.value = 'error'; }
}

/** 先保存原请求，再加减库存；结果未知时锁定原内容，仅重试相同操作。 */
async function submit() {
  if (busy.value) return;
  let input = pending.value;
  if (!uncertain.value) {
    const count = Number(quantity.value);
    if (!materialCode.value || !grade.value.trim() || !cabinet.value || !Number.isFinite(count) || count <= 0 ||
      (parsed.value?.cellCode && parsed.value.cellCode !== cellCode.value)) {
      notice.value = '请扫描有效存货编码，填写等级、选择正确柜位并输入正数量'; noticeType.value = 'warning'; return;
    }
    if ((cabinet.value.materialCode && cabinet.value.materialCode !== materialCode.value) ||
      (cabinet.value.grade && cabinet.value.grade !== grade.value.trim())) {
      notice.value = '存货或等级与柜位固定信息不一致，不能混放或领用'; noticeType.value = 'error'; return;
    }
    if ((props.operation === 'deposit' && !cabinet.value.canStore) ||
      (props.operation === 'withdraw' && count > availableQuantity.value)) {
      notice.value = props.operation === 'deposit' ? '该柜位当前不能寄存' : '领用数量不能超过当前可领用数量'; noticeType.value = 'warning'; return;
    }
    requestId.value ||= newRemainderRequestId();
    input = { requestId: requestId.value, materialCode: materialCode.value, grade: grade.value.trim(), cellCode: cabinet.value.cellCode, quantity: count };
  }
  if (!input) return;
  try { window.localStorage.setItem(pendingKey, JSON.stringify(input)); }
  catch { notice.value = '无法保存操作标识，本次未发送，请检查浏览器存储'; noticeType.value = 'error'; return; }
  pending.value = input;
  busy.value = true;
  try {
    const result = await (props.operation === 'deposit' ? depositRemainder(input) : withdrawRemainder(input));
    window.localStorage.removeItem(pendingKey);
    pending.value = undefined; uncertain.value = false;
    notice.value = result.message || (result.success ? '操作完成' : '操作未完成');
    noticeType.value = result.success ? 'success' : 'warning';
    if (result.success) { requestId.value = ''; quantity.value = null; await loadCell(); }
  } catch (error: any) {
    uncertain.value = true;
    notice.value = (error?.message || '操作结果未确认') + '；请重试原请求，避免重复加减库存'; noticeType.value = 'error';
  } finally { busy.value = false; }
}

onMounted(async () => {
  try {
    const saved = window.localStorage.getItem(pendingKey);
    if (!saved) return;
    uncertain.value = true;
    const input = JSON.parse(saved) as RemainderProductOperation;
    if (!input.requestId || !input.materialCode || !input.grade || !input.cellCode || !(input.quantity > 0))
      throw new Error('本机待确认请求不完整，请先核对原操作记录');
    pending.value = input; requestId.value = input.requestId;
    scanCode.value = input.materialCode; grade.value = input.grade; cellCode.value = input.cellCode; quantity.value = input.quantity;
    await loadCell();
  } catch (error: any) { notice.value = error?.message || '读取待确认请求失败'; noticeType.value = 'error'; }
});
</script>

<style scoped>
.product-operation { padding: 4px 16px 20px; }
label { display: block; font-weight: 600; margin: 14px 0 8px; }
.ant-btn { margin-top: 10px; }
.ant-alert { margin-top: 10px; }
p { margin: 10px 0; }
</style>
