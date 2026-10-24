<template>
  <component :is="inline ? 'div' : Drawer" v-bind="containerProps">
    <a-alert v-if="uncertain" type="warning" show-icon
      message="上次入柜结果未确认，请重试原请求；柜位、物料及数量暂时锁定，避免重复增加库存。" />
    <template v-if="inline">
      <p>所属仓库：4楼叉车库</p>
      <div class="entry-field">
        <label>来源ASN单号</label>
        <a-input v-model:value="entryAsn" :disabled="locked" placeholder="扫描或输入已保存的ASN单号"
          allow-clear @pressEnter="loadSources" />
        <a-button :disabled="locked || !entryAsn.trim()" @click="loadSources">查询ASN</a-button>
      </div>
    </template>
    <p v-else>当前ASN：{{ entryAsn || '-' }}</p>
    <p>每格固定同物料、同等级，批次分别记录；列从左到右01—15，层从下到上01—08。</p>
    <div class="entry-field">
      <label>零头柜位</label>
      <a-select :value="cellCode || undefined" :disabled="locked" :options="cellOptions" show-search allow-clear
        option-filter-prop="label" placeholder="请选择柜位，可搜索A15-08" style="width: 100%" @change="selectCabinet" />
      <a-button :disabled="locked || !cellCode.trim()" @click="loadCabinet">刷新柜位</a-button>
    </div>
    <a-alert v-if="cabinet" :type="cabinet.canStore ? 'info' : 'warning'" show-icon
      :message="`${cabinet.warehouseName} / ${cabinet.cellCode}；固定物料：${cabinet.materialCode || '首次入柜时绑定'}；等级：${cabinet.grade || '-'}`" />
    <div class="entry-field">
      <label>柜位物料标识卡</label>
      <a-input v-model:value="cardBarcode" :disabled="locked" placeholder="例如622550001,,,Z1,,," @pressEnter="scanCard" />
      <a-button :disabled="locked" @click="scanCard">识别标识卡</a-button>
    </div>
    <p v-if="card">物料编码：{{ card.materialCode }}；等级：{{ card.grade }}</p>
    <div class="entry-field">
      <label>对应ASN订单及批次</label>
      <a-select v-model:value="selectedLineId" :disabled="locked" placeholder="选择真实来源订单、物料及批次" style="width: 100%">
        <a-select-option v-for="line in candidates" :key="line.autoid" :value="line.autoid" :disabled="!eligible(line)">
          {{ line.cordercode }} / {{ line.cinvcode }} / {{ line.cbatch || '无批号' }} / 剩余{{ remaining(line) }} / {{ line.materialInspectionStatusName }}
        </a-select-option>
      </a-select>
      <a-button v-if="!inline" :disabled="locked" @click="loadSources">刷新ASN</a-button>
    </div>
    <p v-if="selected">物料：{{ selected.cinvname }}；批次：{{ selected.cbatch || '-' }}；剩余待入库：{{ remaining(selected) }}</p>
    <div class="entry-field">
      <label>实际零头数量</label>
      <a-input-number v-model:value="quantity" :disabled="locked" :min="0" :max="selected ? remaining(selected) : undefined"
        :precision="6" placeholder="按实物录入，标识卡不含数量" style="width: 100%" />
      <span>{{ selected?.cinfvm_unit || '单位同ASN' }}</span>
    </div>
    <a-alert v-if="notice" :message="notice" :type="noticeType" show-icon />
    <div class="entry-actions">
      <a-button v-if="!inline" :disabled="locked" @click="close">关闭</a-button>
      <a-button type="primary" :loading="busy" :disabled="busy" @click="submit">
        {{ uncertain ? '重试确认原入柜请求' : '确认已放入柜位并登记库存' }}
      </a-button>
    </div>
    <h4>柜位当前库存</h4>
    <div v-for="stock in cabinet?.stocks || []" :key="stock.stockId" class="stock-row">
      <div>{{ stock.materialCode }} / {{ stock.grade }} / 批次{{ stock.batchCode || '-' }} / 数量{{ stock.quantity }}</div>
      <div>ASN：{{ stock.asnCode || '-' }}；订单：{{ stock.orderCode || '-' }}；{{ stock.status === 'Waiting' ? '待U8采购入库' : stock.status === 'Available' ? '可用库存' : stock.status }}</div>
    </div>
  </component>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { Drawer } from 'ant-design-vue';
import { queryAsnMaterialInspection } from '../views/U8InboundApi';
import type { AsnMaterialInspectionLine } from '../views/U8InboundApi';
import { getRemainderCabinet, storeAsnRemainder } from '../views/RemainderCabinetApi';
import type { RemainderCabinet, RemainderCabinetEntry } from '../views/RemainderCabinetApi';

const props = defineProps<{
  visible: boolean;
  asnCode: string;
  /** 独立录入页面直接显示表单；ASN组盘仍以抽屉显示同一表单。 */
  inline?: boolean;
}>();
const emit = defineEmits<{
  (event: 'update:visible', value: boolean): void;
  (event: 'stored', asnCode: string): void;
}>();
const drawerWidth = Math.min(window.innerWidth, 760);
/** 原请求先保存到本机，页面刷新后仍可沿用标识核实结果，不用新请求重复入库。 */
const pendingKey = 'wms:asn-remainder-entry:pending:v1';
const entryAsn = ref('');
const cellCode = ref('');
/** 固定柜位清单：A—L柜，列从左到右01—15、层从下到上01—08，共1440格。 */
const cellOptions: { label: string; value: string }[] = [];
for (let cabinet = 0; cabinet < 12; cabinet++) {
  for (let column = 1; column <= 15; column++) {
    for (let layer = 1; layer <= 8; layer++) {
      const code = `${String.fromCharCode(65 + cabinet)}${String(column).padStart(2, '0')}-${String(layer).padStart(2, '0')}`;
      cellOptions.push({ label: code, value: code });
    }
  }
}
const cardBarcode = ref('');
const selectedLineId = ref<string>();
const quantity = ref<number | null>(null);
const sources = ref<AsnMaterialInspectionLine[]>([]);
const cabinet = ref<RemainderCabinet>();
const busy = ref(false);
const uncertain = ref(false);
const locked = computed(() => busy.value || uncertain.value);
const requestId = ref('');
const pending = ref<RemainderCabinetEntry>();
const notice = ref('');
const noticeType = ref<'info' | 'success' | 'warning' | 'error'>('info');
/** 内嵌模式只设置页面样式，抽屉模式继续保留关闭及未确认请求的锁定约束。 */
const containerProps = computed(() => props.inline ? { class: 'remainder-entry-inline' } : {
  visible: props.visible, title: '零头柜入柜（4楼叉车库）', width: drawerWidth,
  maskClosable: false, closable: !locked.value, keyboard: !locked.value, onClose: close,
});

/** 卡码只负责识别物料及等级；批次为空时必须从ASN明确选择，不推断数量。 */
const card = computed(() => {
  const fields = cardBarcode.value.trim().split(',').map(value => value.trim());
  return fields.length >= 4 && fields[0] && fields[3]
    ? { materialCode: fields[0], grade: fields[3], batchCode: fields[1] || '' } : null;
});
const candidates = computed(() => sources.value.filter(line => line.status !== 4 && card.value &&
  line.cinvcode === card.value.materialCode && (line.cfree5 || '').trim() === card.value.grade &&
  (!card.value.batchCode || line.cbatch === card.value.batchCode)));
const selected = computed(() => sources.value.find(line => line.autoid === selectedLineId.value));

/** 同时扣除已经组盘的整箱和已入柜的零头，数量均以服务端最新ASN为准。 */
function remaining(line: AsnMaterialInspectionLine) {
  return Math.max(0, Number(line.iquantity || 0) - Number(line.alreadyStockInQuantity || 0));
}
function eligible(line: AsnMaterialInspectionLine) {
  return line.status !== 4 && line.isInspectionReportPushed && line.materialInspectionStatus === 2 && remaining(line) > 0;
}

/** GUID在首次提交时生成；失败重试不变，成功后下一次入柜重新生成。 */
function newRequestId() {
  const browserCrypto = window.crypto as Crypto & { randomUUID?: () => string };
  return browserCrypto?.randomUUID?.() || 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, value => {
    const number = Math.floor(Math.random() * 16);
    return (value === 'x' ? number : (number & 3) | 8).toString(16);
  });
}

watch(cardBarcode, () => {
  if (locked.value) return;
  selectedLineId.value = undefined;
  chooseUniqueSource();
});
watch(cellCode, () => { if (!locked.value) cabinet.value = undefined; });
/** 内嵌表单切换ASN时清除旧来源和数量；未确认的原入柜请求仍锁定，不能换单。 */
watch(entryAsn, () => {
  if (locked.value) return;
  sources.value = [];
  selectedLineId.value = undefined;
  quantity.value = null;
  requestId.value = '';
  notice.value = '';
});
watch(() => props.visible, async visible => {
  if (!visible) return;
  try {
    const saved = window.localStorage.getItem(pendingKey);
    if (saved) {
      uncertain.value = true;
      const original = JSON.parse(saved) as RemainderCabinetEntry;
      if (!original.requestId || !original.asnLineId || !original.asnCode || !original.cellCode || !(original.quantity > 0))
        throw new Error('本机待确认入柜数据不完整，请先核对原库存记录');
      pending.value = original;
      requestId.value = original.requestId;
      entryAsn.value = original.asnCode;
      cellCode.value = original.cellCode;
      cardBarcode.value = original.cardBarcode;
      selectedLineId.value = original.asnLineId;
      quantity.value = original.quantity;
    } else {
      entryAsn.value = props.asnCode.trim();
      uncertain.value = false;
      pending.value = undefined;
      requestId.value = '';
      sources.value = [];
      selectedLineId.value = undefined;
      quantity.value = null;
      notice.value = '';
    }
    await loadSources();
    if (cellCode.value.trim()) await loadCabinet();
  } catch (error: any) {
    notice.value = error?.message || '读取待确认入柜请求失败';
    noticeType.value = 'error';
  }
}, { immediate: true });

/** 查询本地已保存ASN；入柜只允许已报检并人工确认合格的剩余明细。 */
async function loadSources() {
  if (!entryAsn.value) return;
  const code = entryAsn.value;
  try {
    const result = await queryAsnMaterialInspection(code);
    if (code !== entryAsn.value) return;
    if (!result.success) throw new Error(result.message || '查询ASN失败');
    sources.value = result.data || [];
    if (!uncertain.value) chooseUniqueSource();
  } catch (error: any) {
    if (code !== entryAsn.value) return;
    notice.value = error?.message || '加载ASN来源失败';
    noticeType.value = 'error';
  }
}

/** 同料同级有多个订单或批次时保留人工选择，不按返回顺序猜测来源。 */
function chooseUniqueSource() {
  const choices = candidates.value.filter(eligible);
  if (choices.length === 1) selectedLineId.value = choices[0].autoid;
}
function scanCard() {
  if (!card.value) {
    notice.value = '标识卡需包含第1段物料编码、第4段等级，例如622550001,,,Z1,,,';
    noticeType.value = 'warning';
    return;
  }
  chooseUniqueSource();
  notice.value = candidates.value.some(eligible) ? '已识别标识卡，请选择来源批次并录入实际零头数量' : '该ASN没有与标识卡匹配且已检验合格的剩余物料';
  noticeType.value = 'warning';
}

/** 下拉选择后自动查询；清空或切换柜位时立即清除旧库存，避免沿用上一格的信息。 */
async function selectCabinet(value?: string) {
  cellCode.value = value || '';
  cabinet.value = undefined;
  if (cellCode.value) await loadCabinet();
}

/** 柜位与物料卡分开选择，空库存格位仍显示固定物料及等级。 */
async function loadCabinet() {
  if (!cellCode.value.trim()) return;
  const code = cellCode.value.trim();
  try {
    const location = await getRemainderCabinet(code);
    if (code !== cellCode.value.trim()) return;
    cabinet.value = location;
  } catch (error: any) {
    if (code !== cellCode.value.trim()) return;
    cabinet.value = undefined;
    notice.value = error?.message || '查询零头柜位失败';
    noticeType.value = 'error';
  }
}

/** 先持久化原请求，网络异常时锁定原数据重试；只有成功或明确业务拒绝后清除待确认请求。 */
async function submit() {
  if (busy.value) return;
  let input = pending.value;
  if (!uncertain.value) {
    if (!cabinet.value?.canStore || !card.value || !selected.value || !eligible(selected.value) ||
      !candidates.value.some(line => line.autoid === selectedLineId.value) ||
      !Number.isFinite(Number(quantity.value)) || !(Number(quantity.value) > 0) || Number(quantity.value) > remaining(selected.value)) {
      notice.value = '请查询有效柜位、识别标识卡、选择合格ASN明细，并填写不超过剩余数量的实际零头数量';
      noticeType.value = 'warning'; return;
    }
    if ((cabinet.value.materialCode && cabinet.value.materialCode !== card.value.materialCode) ||
      (cabinet.value.grade && cabinet.value.grade !== card.value.grade)) {
      notice.value = '柜位固定物料或等级与标识卡不一致，不能混放'; noticeType.value = 'error'; return;
    }
    requestId.value ||= newRequestId();
    input = { requestId: requestId.value, asnCode: entryAsn.value, asnLineId: selectedLineId.value!,
      cellCode: cabinet.value.cellCode, cardBarcode: cardBarcode.value.trim(), quantity: Number(quantity.value) };
  }
  if (!input) return;
  try { window.localStorage.setItem(pendingKey, JSON.stringify(input)); }
  catch { notice.value = '无法保存入柜提交标识，本次未发送，请检查浏览器存储后重试'; noticeType.value = 'error'; return; }
  pending.value = input;
  busy.value = true;
  try {
    const result = await storeAsnRemainder(input);
    window.localStorage.removeItem(pendingKey);
    uncertain.value = false;
    pending.value = undefined;
    notice.value = result.message || (result.success ? '零头库存已登记' : '入柜未完成');
    noticeType.value = result.success ? 'success' : 'warning';
    if (result.success) {
      requestId.value = '';
      quantity.value = null;
      emit('stored', input.asnCode);
      await loadSources();
      await loadCabinet();
    }
  } catch (error: any) {
    uncertain.value = true;
    notice.value = (error?.message || '入柜请求异常') + '；请重试确认原请求，不要新增重复入库';
    noticeType.value = 'error';
  } finally { busy.value = false; }
}

/** 原入柜结果尚未确认时保持页面及原标识，先重试确认，再开始下一笔入库。 */
function close() {
  if (!locked.value) emit('update:visible', false);
}
</script>

<style scoped>
.remainder-entry-inline { padding: 12px 16px; }
.entry-field { margin: 14px 0; display: flex; flex-wrap: wrap; gap: 8px; align-items: center; }
.entry-field label { width: 100%; font-weight: 600; }
.entry-field .ant-input { flex: 1; min-width: 150px; }
.entry-actions { display: flex; gap: 8px; flex-wrap: wrap; margin: 16px 0; }
.stock-row { border-bottom: 1px solid #eee; padding: 8px 0; overflow-wrap: anywhere; }
p { margin: 8px 0; }
</style>
