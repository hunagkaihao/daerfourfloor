<template>
  <section class="asn-material-inspection">
    <h3>ASN订单检验状态</h3>
    <p>报检成功后进入检验中，检验完成后按采购订单确认合格或不合格。</p>
    <div class="search">
      <a-input v-model:value="asnCode" placeholder="扫描或输入ASN送货单号" :disabled="busy || disabled" allow-clear @pressEnter="load()" />
      <a-button :disabled="busy || disabled || !asnCode.trim()" :loading="busy" @click="load()">查询状态</a-button>
    </div>
    <a-alert v-if="notice" :message="notice" :type="noticeType" show-icon />
    <a-card v-for="group in groups" :key="group.orderCode" :title="`订单：${group.orderCode}`" size="small">
      <div v-for="line in group.lines" :key="line.autoid" class="material-row">
        <div>{{ line.cinvcode }} / {{ line.cinvname }}</div>
        <div>批号：{{ line.cbatch || '-' }}；数量：{{ line.iquantity }}</div>
        <div>检验状态：{{ line.materialInspectionStatusName || '待检' }}</div>
      </div>
      <div class="actions">
        <a-button type="primary" :disabled="busy || disabled || !canConfirm(group.lines, 2)" @click="confirm(group.orderCode, 2)">确认合格</a-button>
        <a-button danger :disabled="busy || disabled || !canConfirm(group.lines, 3)" @click="confirm(group.orderCode, 3)">确认不合格</a-button>
      </div>
    </a-card>
  </section>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { useMessage } from '/@/hooks/web/useMessage';
import { queryAsnMaterialInspection, setAsnOrderInspectionStatus } from '../views/U8InboundApi';
import type { AsnMaterialInspectionLine } from '../views/U8InboundApi';

const props = defineProps<{
  /** 组盘提交或已有待提交箱码时锁定查询和人工判定，避免切换来源。 */
  disabled?: boolean;
}>();
const emit = defineEmits<{
  /** 查询或人工确认后的本地ASN明细，空单号表示原查询已失效。 */
  (event: 'loaded', asnCode: string, lines: AsnMaterialInspectionLine[]): void;
  /** 将查询与人工判定状态同步给父页，与组盘及搬运互斥。 */
  (event: 'busy-change', busy: boolean): void;
}>();
const { createConfirm } = useMessage();
const asnCode = ref('');
const loadedCode = ref('');
const lines = ref<AsnMaterialInspectionLine[]>([]);
const busy = ref(false);
const notice = ref('');
const noticeType = ref<'success' | 'warning' | 'error'>('warning');

/** 查询条件变化后清除旧订单，避免把旧结果确认到另一个ASN。 */
watch(asnCode, () => {
  lines.value = []; loadedCode.value = ''; notice.value = '';
  emit('loaded', '', []);
}, { flush: 'sync' });
watch(busy, value => emit('busy-change', value), { flush: 'sync' });

/** 同一送货单内按真实采购订单显示物料，已取消的明细不参与确认。 */
const groups = computed(() => {
  const map = new Map<string, AsnMaterialInspectionLine[]>();
  for (const line of lines.value.filter(item => item.status !== 4)) {
    const orderCode = line.cordercode || '';
    if (!map.has(orderCode)) map.set(orderCode, []);
    map.get(orderCode)!.push(line);
  }
  return Array.from(map, ([orderCode, orderLines]) => ({ orderCode, lines: orderLines }));
});

/** 仅检验中状态可以确认；已提交的相同结果允许与其他待确认明细一并重试。 */
function canConfirm(orderLines: AsnMaterialInspectionLine[], target: 2 | 3) {
  return orderLines.some(line => line.materialInspectionStatus === 1) && orderLines.every(line =>
    line.isInspectionReportPushed && (line.materialInspectionStatus === 1 || line.materialInspectionStatus === target));
}

/** 刷新本地状态，人工确认无需等待U8检验单查询。 */
async function load(force = false) {
  if (busy.value || (props.disabled && !force) || !asnCode.value.trim()) return;
  const code = asnCode.value.trim();
  busy.value = true;
  lines.value = [];
  loadedCode.value = '';
  emit('loaded', '', []);
  try {
    const result = await queryAsnMaterialInspection(code);
    if (code !== asnCode.value.trim()) return;
    loadedCode.value = result.success ? code : '';
    lines.value = result.success ? result.data || [] : [];
    notice.value = result.message || '查询完成';
    noticeType.value = result.success ? 'success' : 'warning';
  } catch (error: any) {
    notice.value = error?.message || '查询检验状态失败';
    noticeType.value = 'error';
  } finally {
    emit('loaded', loadedCode.value, lines.value);
    busy.value = false;
  }
}

/** 父页组盘或取消成功后刷新剩余数量，允许在父页提交锁内调用。 */
function reload() { return load(true); }
defineExpose({ reload });

/** 按送货单和采购订单确认结果，服务端再次校验状态并在一个事务内保存。 */
function confirm(orderCode: string, target: 2 | 3) {
  if (busy.value || props.disabled || !loadedCode.value || loadedCode.value !== asnCode.value.trim()) return;
  const code = loadedCode.value;
  const label = target === 2 ? '合格' : '不合格';
  createConfirm({
    title: `确认${label}`,
    content: `确认将送货单${code}中采购订单${orderCode}的检验中物料判定为${label}？`,
    onOk: async () => {
      if (busy.value || props.disabled || code !== loadedCode.value) return;
      busy.value = true;
      let text = '';
      let type: 'success' | 'warning' | 'error' = 'warning';
      try {
        const result = await setAsnOrderInspectionStatus(code, orderCode, target);
        text = result.message || '状态已更新';
        type = result.success ? 'success' : 'warning';
      } catch (error: any) {
        text = error?.message || '确认检验结果失败';
        type = 'error';
      } finally { busy.value = false; }
      await load();
      notice.value = text;
      noticeType.value = type;
    },
  });
}
</script>

<style scoped>
.asn-material-inspection { margin: 12px; padding: 12px; border: 1px solid #eee; border-radius: 8px; }
.asn-material-inspection h3 { margin: 0 0 8px; }
.asn-material-inspection p { color: #666; font-size: 12px; }
.search, .actions { display: flex; gap: 8px; margin: 8px 0; }
.search .ant-input-affix-wrapper { flex: 1; min-width: 0; }
.ant-card { margin-top: 10px; }
.material-row { padding: 6px 0; overflow-wrap: anywhere; }
</style>
