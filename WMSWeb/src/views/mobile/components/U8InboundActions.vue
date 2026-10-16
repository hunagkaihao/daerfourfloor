<template>
  <div class="u8-inbound-actions">
    <a-input v-model:value="code" placeholder="扫描或输入ASN单号" :allowClear="true" :disabled="busy" />
    <div class="actions">
      <a-button :loading="busy" :disabled="!code.trim() || busy" @click="report">推送来料报检单</a-button>
      <a-button :loading="busy" :disabled="!code.trim() || busy" @click="query">查询U8检验结果</a-button>
    </div>
    <p>到货单须先由仓管员在U8审核，再推送报检。确认合格前请将抽检样品放回托盘。</p>
    <a-alert v-if="notice" :message="notice" :type="noticeType" show-icon />
    <div v-for="row in rows" :key="row.asnLineId" class="inspection-row">
      <div>{{ row.materialCode }} / {{ row.batchCode || '-' }}</div>
      <div>检验单：{{ row.checkCode || '待检验' }}；合格数量：{{ row.qualifiedQuantity }}</div>
      <div>{{ row.message }}</div>
      <a-button :disabled="busy || !row.canPurchase || row.isPurchasePushed" @click="purchase(row)">
        {{ row.isPurchasePushed ? '采购入库已推送' : '推送或重试采购入库' }}
      </a-button>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, watch } from 'vue';
import { useMessage } from '/@/hooks/web/useMessage';
import { queryU8Inspection, reportU8Inspection, retryU8Purchase, U8InspectionLine } from '../views/U8InboundApi';

const props = defineProps<{ asnCode?: string }>();
const { createConfirm } = useMessage();
const code = ref(props.asnCode || '');
const busy = ref(false);
const notice = ref('');
const noticeType = ref<'success' | 'warning' | 'error'>('warning');
const rows = ref<U8InspectionLine[]>([]);

/** 当前ASN变化时清除旧检验结果，避免使用上一次查询的来源行操作。 */
watch(() => props.asnCode, value => { if (value) code.value = value; });
watch(code, () => { rows.value = []; notice.value = ''; });

/** 执行单次业务操作，失败说明持续显示，避免仅用短暂消息掩盖待审核状态。 */
async function run(action: () => Promise<any>) {
  if (busy.value || !code.value.trim()) return;
  busy.value = true;
  try {
    const result = await action();
    notice.value = result.message || (result.success ? '操作成功' : '操作未完成');
    noticeType.value = result.success ? 'success' : 'warning';
    if (Array.isArray(result.items)) rows.value = result.success ? result.items : [];
  } catch (error: any) {
    rows.value = [];
    notice.value = error?.message || 'U8请求失败，请稍后重试';
    noticeType.value = 'error';
  } finally { busy.value = false; }
}

/** 人工审核在U8完成，不在PDA伪造审核状态。 */
function report() {
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
  if (busy.value || !row.canPurchase || row.isPurchasePushed) return;
  createConfirm({
    title: '采购入库',
    content: '确认全部抽检样品已放回托盘，按检验单生成采购入库单？',
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

