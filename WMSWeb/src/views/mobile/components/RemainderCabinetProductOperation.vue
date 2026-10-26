<template>
  <div class="product-operation">
    <a-alert v-if="uncertain" type="warning" show-icon message="上次操作结果未确认，请重试原请求；箱码、柜位和数量暂时锁定。" />
    <label>扫描箱码</label>
    <a-input v-model:value="scanCode" :disabled="locked" placeholder="例如606810005,260924F1042,02,Z3,240,|F9OF,,0002"
      allow-clear @pressEnter="recognize" />
    <p>存货编码：{{ materialCode || '-' }}</p>
    <p>等级：{{ grade || '-' }}</p>
    <p v-if="locations.length">对应柜位：{{ locations.map(item => item.cellCode).join('、') }}</p>
    <label>零头柜位</label>
    <a-select :value="cellCode || undefined" :options="locationOptions" :disabled="locked || resolving" show-search
      option-filter-prop="label" placeholder="扫码后显示对应柜位，多个柜位时请选择" style="width: 100%" @change="selectCell" />
    <a-alert v-if="cabinet" show-icon :type="cabinet.canStore || operation === 'withdraw' ? 'info' : 'warning'"
      :message="`${cabinet.warehouseName} / ${cabinet.cellCode}`" />
    <p v-if="cabinet">当前库存：{{ currentQuantity }}；可领用：{{ availableQuantity }}</p>
    <label>{{ operationName }}数量</label>
    <a-input-number v-model:value="quantity" :disabled="locked || resolving || !cabinet" :min="0" :precision="6"
      :max="operation === 'withdraw' ? availableQuantity : undefined" placeholder="输入实际数量" style="width: 100%" />
    <a-alert v-if="notice" :type="noticeType" :message="notice" show-icon />
    <a-button type="primary" block :loading="busy" :disabled="busy || resolving" @click="submit">
      {{ uncertain ? '重试确认原请求' : `确认${operationName}` }}
    </a-button>
  </div>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { depositRemainder, getRemainderCabinet, getRemainderLocations, withdrawRemainder } from '../views/RemainderCabinetApi';
import type { RemainderCabinet, RemainderProductOperation } from '../views/RemainderCabinetApi';
import { newRemainderRequestId, parseRemainderScan } from '../views/RemainderCabinetHelpers';

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
/** 绑定位置不依赖现存库存，空柜仍能用于寄存。 */
const locations = ref<RemainderCabinet[]>([]);
const locationOptions = computed(() => locations.value.map(item => ({
  label: `${item.cellCode}${item.grade ? ' / ' + item.grade : ''}`, value: item.cellCode,
  disabled: !!item.grade && item.grade !== grade.value,
})));
const resolving = ref(false);
/** 扫码或切换柜位后递增，防止迟到响应覆盖新箱码的柜位。 */
let lookupVersion = 0;
let scanTimer: number | undefined;
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
/** 批次不参与余额匹配，同物料、同等级合并可领用数量。 */
const matchedStocks = computed(() => (cabinet.value?.stocks || []).filter(stock =>
  stock.materialCode === materialCode.value && stock.grade === grade.value));
const currentQuantity = computed(() => Number(matchedStocks.value.reduce((sum, stock) => sum + Number(stock.quantity), 0).toFixed(6)));
const availableQuantity = computed(() => Number(matchedStocks.value.reduce((sum, stock) => sum + (stock.canWithdraw ? Number(stock.quantity) : 0), 0).toFixed(6)));

watch(locked, value => emit('lock-change', value), { immediate: true });
watch(scanCode, () => {
  if (locked.value) return;
  lookupVersion++;
  if (scanTimer !== undefined) window.clearTimeout(scanTimer);
  scanTimer = undefined;
  resolving.value = false;
  cabinet.value = undefined; cellCode.value = ''; locations.value = [];
  grade.value = parsed.value?.grade || '';
  quantity.value = null;
  notice.value = '';
  // 兼容不附带回车的扫码器，连续输入结束后自动查询；回车会取消本次延时。
  if (parsed.value?.materialCode && parsed.value?.grade)
    scanTimer = window.setTimeout(recognize, 350);
}, { flush: 'sync' });

/** 读取实际柜位；过期响应不能覆盖用户刚选择的新柜位。 */
async function loadCell() {
  const code = cellCode.value;
  const version = lookupVersion;
  if (!code) return;
  try {
    const result = await getRemainderCabinet(code);
    if (code !== cellCode.value || version !== lookupVersion) return;
    cabinet.value = result;
    if (!locked.value && !grade.value) grade.value = parsed.value?.grade || result.grade || '';
  } catch (error: any) {
    if (code !== cellCode.value || version !== lookupVersion) return;
    cabinet.value = undefined;
    notice.value = error?.message || '查询柜位失败'; noticeType.value = 'error';
  }
}

/** 下拉选择后自动读取固定存货、等级及当前数量。 */
async function selectCell(value?: string) {
  if (locked.value) return;
  lookupVersion++;
  cellCode.value = (value || '').toUpperCase();
  cabinet.value = undefined;
  quantity.value = null;
  resolving.value = true;
  const version = lookupVersion;
  try { if (cellCode.value) await loadCell(); }
  finally { if (version === lookupVersion) resolving.value = false; }
}

/** 按箱码物料、等级定位柜位；仅寄存提取批次用于流水，领用忽略批次。 */
async function recognize() {
  if (locked.value) return;
  if (scanTimer !== undefined) window.clearTimeout(scanTimer);
  scanTimer = undefined;
  const version = ++lookupVersion;
  const raw = scanCode.value.trim();
  resolving.value = true;
  cabinet.value = undefined; cellCode.value = ''; locations.value = [];
  try {
    const scan = parseRemainderScan(raw);
    if (!scan.materialCode || !scan.grade) throw new Error('请扫描完整箱码，物料编码在第1段、等级在第4段');
    if (props.operation === 'deposit' && scan.batchCode.length > 30) throw new Error('箱码批次号超过30字符，请核对箱码');
    grade.value = scan.grade;
    const result = await getRemainderLocations(scan.materialCode);
    if (version !== lookupVersion || locked.value) return;
    locations.value = (result || []).map(item => ({ ...item, cellCode: item.cellCode.toUpperCase() }));
    const compatible = locations.value.filter(item => !item.grade || item.grade === grade.value);
    if (compatible.length === 1) {
      cellCode.value = compatible[0].cellCode;
      await loadCell();
    }
    if (version !== lookupVersion || locked.value) return;
    notice.value = locations.value.length === 0 ? '该物料尚未绑定零头柜位，请先在“库位绑定”页面维护'
      : compatible.length === 0 ? '对应柜位均已固定为其他等级，请先核对物料等级'
      : compatible.length > 1 ? `匹配${compatible.length}个同等级柜位，请选择实际柜位`
      : cabinet.value ? '已显示对应柜位，请输入实际数量' : notice.value;
    noticeType.value = cabinet.value || compatible.length > 1 ? 'info' : 'warning';
  } catch (error: any) {
    if (version === lookupVersion) { notice.value = error?.message || '识别箱码失败'; noticeType.value = 'error'; }
  } finally { if (version === lookupVersion) resolving.value = false; }
}

/** 先保存原请求，再加减库存；结果未知时锁定原内容，仅重试相同操作。 */
async function submit() {
  if (busy.value || resolving.value) return;
  let input = pending.value;
  if (!uncertain.value) {
    const count = Number(quantity.value);
    if (!materialCode.value || !grade.value.trim() || !cabinet.value || !Number.isFinite(count) || count <= 0 ||
      (parsed.value?.cellCode && parsed.value.cellCode !== cellCode.value)) {
      notice.value = '请扫描完整箱码、选择对应柜位并输入正数量'; noticeType.value = 'warning'; return;
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
    input = { requestId: requestId.value, materialCode: materialCode.value, grade: grade.value.trim(),
      cellCode: cabinet.value.cellCode.toUpperCase(), quantity: count,
      ...(props.operation === 'deposit' ? { batchCode: parsed.value?.batchCode || '' } : {}) };
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
    scanCode.value = [input.materialCode, input.batchCode || '', '', input.grade].join(',');
    grade.value = input.grade; cellCode.value = input.cellCode.toUpperCase(); quantity.value = input.quantity;
    await loadCell();
    if (cabinet.value) locations.value = [cabinet.value];
  } catch (error: any) { notice.value = error?.message || '读取待确认请求失败'; noticeType.value = 'error'; }
});

onBeforeUnmount(() => { lookupVersion++; if (scanTimer !== undefined) window.clearTimeout(scanTimer); });
</script>

<style scoped>
.product-operation { padding: 4px 16px 20px; }
label { display: block; font-weight: 600; margin: 14px 0 8px; }
.ant-btn { margin-top: 10px; }
.ant-alert { margin-top: 10px; }
p { margin: 10px 0; }
</style>
