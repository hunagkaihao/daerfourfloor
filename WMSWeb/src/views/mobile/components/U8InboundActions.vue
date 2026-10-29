<template>
  <div class="u8-inbound-actions">
    <a-input v-if="showAsnInput !== false" v-model:value="code" placeholder="扫描或输入ASN单号" :allowClear="true" :disabled="busy || disabled" />
    <p v-else>当前ASN：{{ code || '请先输入ASN单号' }}</p>
    <div class="actions">
      <a-button :loading="busy" :disabled="!code.trim() || busy || disabled" @click="arrival">生成或重试到货单</a-button>
      <a-button :loading="busy" :disabled="!code.trim() || busy || disabled" @click="report">推送来料报检单</a-button>
      <a-button v-if="showInspectionQuery !== false" :loading="busy" :disabled="!code.trim() || busy || disabled" @click="query">查询U8检验结果</a-button>
    </div>
    <p>先保存ASN并生成到货单，U8审核后报检进入检验中；检验合格后在物料组盘页逐箱组盘，采购入库另需U8检验单来源。</p>
    <a-alert v-if="notice" :message="notice" :type="noticeType" show-icon />
    <div v-for="row in rows" :key="row.asnLineId" class="inspection-row">
      <div>{{ row.materialCode }} / {{ row.batchCode || '-' }}</div>
      <div>检验单：{{ row.checkCode || '待检验' }}；合格数量：{{ row.qualifiedQuantity }}</div>
      <div>已组盘：{{ row.groupedQuantity || 0 }}；待组盘：{{ row.pendingGroupingQuantity || 0 }}</div>
      <div>{{ row.message }}</div>
      <a-button :disabled="busy || disabled || !row.canPurchase || row.isPurchasePushed" @click="purchase(row)">
        {{ row.isPurchasePushed ? '采购入库已推送' : '推送或重试采购入库' }}
      </a-button>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, watch } from 'vue';
import { useMessage } from '/@/hooks/web/useMessage';
import { createU8Arrival, queryU8Inspection, reportU8Inspection, retryU8Purchase, U8InspectionLine } from '../views/U8InboundApi';

const emit = defineEmits<{
  (event: 'inspection-results', rows: U8InspectionLine[]): void;
  (event: 'workflow-completed', asnCode: string): void;
  (event: 'busy-change', busy: boolean): void;
}>();
const props = withDefaults(defineProps<{
  asnCode?: string;
  showAsnInput?: boolean;
  /** 是否显示U8检验结果查询按钮；未传时显示，扫描ASN页传false隐藏。 */
  showInspectionQuery?: boolean;
  disabled?: boolean;
}>(), { showInspectionQuery: true });
const { createConfirm } = useMessage();
const code = ref(props.asnCode || '');
const busy = ref(false);
const notice = ref('');
const noticeType = ref<'success' | 'warning' | 'error'>('warning');
const rows = ref<U8InspectionLine[]>([]);

/** 同步整单请求状态，父页可与明细推送、保存和扫码互斥。 */
watch(busy, value => emit('busy-change', value), { immediate: true, flush: 'sync' });

/** 当前ASN变化时清除旧检验结果，避免使用上一次查询的来源行操作。 */
watch(() => props.asnCode, value => { code.value = value || ''; });
watch(code, () => { rows.value = []; notice.value = ''; });

/** 执行单次业务操作，失败说明持续显示，避免仅用短暂消息掩盖待审核状态。 */
async function run(action: () => Promise<any>) {
  if (busy.value || props.disabled || !code.value.trim()) return;
  const operationCode = code.value.trim();
  busy.value = true;
  try {
    const result = await action();
    notice.value = result.message || (result.success ? '操作成功' : '操作未完成');
    noticeType.value = result.success ? 'success' : 'warning';
    if (Array.isArray(result.items)) {
      rows.value = result.success ? result.items : [];
      emit("inspection-results", rows.value);
    }
    // 部分单据成功时也刷新父页，让检验中的订单及时显示最新状态。
    emit('workflow-completed', operationCode);
  } catch (error: any) {
    rows.value = [];
    emit("inspection-results", []);
    notice.value = error?.message || 'U8请求失败，请稍后重试';
    noticeType.value = 'error';
  } finally { busy.value = false; }
}

/** 到货数量来自ASN，不以组盘作为前提。 */
function arrival() { return run(() => createU8Arrival(code.value)); }

/** 人工审核在U8完成，不在PDA伪造审核状态。 */
function report() {
  if (busy.value || props.disabled) return;
  createConfirm({
    title: '推送来料报检单',
    content: '请先确认仓管员已在U8审核到货单。未审核时需等待审核后重试。',
    onOk: () => run(() => reportU8Inspection(code.value)),
  });
}

/** 主动查询检验结果；未找到时由操作员检验完成后再次查询。 */
function query() { return run(() => queryU8Inspection(code.value)); }

/** 采购重试只调用生单接口，不再次修改本地抽检数量。 */
function purchase(row: U8InspectionLine) {
  if (busy.value || props.disabled || !row.canPurchase || row.isPurchasePushed) return;
  createConfirm({
    title: '采购入库',
    content: '确认当前单据对应的组盘数量与实物一致，按该检验单推送采购入库？',
    onOk: () => run(async () => {
      const result = await retryU8Purchase(row.asnLineId);
      if (result.success) row.isPurchasePushed = true;
      return result;
    }),
  });
}
</script>

<style scoped>
.u8-inbound-actions { margin: 10px 12px; }
.actions { display: flex; gap: 8px; flex-wrap: wrap; margin-top: 8px; }
.u8-inbound-actions p { margin: 8px 0; color: #666; font-size: 12px; }
.inspection-row { padding: 10px 0; border-bottom: 1px solid #eee; overflow-wrap: anywhere; }
.inspection-row .ant-btn { margin-top: 6px; }
</style>

