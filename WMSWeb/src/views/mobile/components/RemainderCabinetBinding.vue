<template>
  <div class="cabinet-binding">
    <label>物料编码</label>
    <a-input v-model:value="scanCode" :disabled="busy" allow-clear placeholder="输入物料编码或扫描箱码"
      @pressEnter="searchLocations" />
    <a-button block :loading="searching" :disabled="busy" @click="searchLocations">查询对应库位</a-button>
    <p v-if="queried">对应库位：{{ locations.map(item => item.cellCode).join('、') || '暂无绑定' }}</p>
    <label>零头柜位</label>
    <a-select :value="cellCode || undefined" :options="remainderCellOptions" :disabled="busy" show-search
      option-filter-prop="label" placeholder="选择需要绑定或解绑的库位" style="width: 100%" @change="selectCell" />
    <p v-if="cabinet">当前绑定物料：{{ cabinet.materialCode || '未绑定' }}；等级：{{ cabinet.grade || '未绑定' }}</p>
    <p v-if="cabinet">当前库存：{{ currentQuantity }}</p>
    <a-alert v-if="notice" :type="noticeType" :message="notice" show-icon />
    <div class="actions">
      <a-button type="primary" :loading="busy" :disabled="reading || busy || searching" @click="changeBinding(false)">绑定物料</a-button>
      <a-button danger :loading="busy" :disabled="reading || busy || searching || !cabinet?.materialCode"
        @click="changeBinding(true)">解绑物料</a-button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, ref } from 'vue';
import { bindRemainderLocation, getRemainderCabinet, getRemainderLocations, unbindRemainderLocation } from '../views/RemainderCabinetApi';
import type { RemainderCabinet } from '../views/RemainderCabinetApi';
import { normalizeRemainderHistoryMaterial, remainderCellOptions } from '../views/RemainderCabinetHelpers';

const emit = defineEmits<{ (event: 'lock-change', locked: boolean): void }>();
const scanCode = ref('');
const cellCode = ref('');
const cabinet = ref<RemainderCabinet>();
const locations = ref<RemainderCabinet[]>([]);
const busy = ref(false);
const reading = ref(false);
const searching = ref(false);
const queried = ref(false);
const notice = ref('');
const noticeType = ref<'info' | 'success' | 'warning' | 'error'>('info');
const currentQuantity = computed(() => (cabinet.value?.stocks || []).reduce((sum, row) => sum + Number(row.quantity), 0));
/** 两类只读查询各自编号，旧请求不能覆盖新选中的库位或物料。 */
let cellVersion = 0;
let materialVersion = 0;

/** 按编码读取所有对应位置，空库存格位也参与匹配。 */
async function searchLocations() {
  if (busy.value) return;
  const version = ++materialVersion;
  const raw = scanCode.value;
  searching.value = true;
  try {
    const material = normalizeRemainderHistoryMaterial(raw);
    if (!material) throw new Error('请先输入或扫描物料编码');
    const rows = await getRemainderLocations(material);
    if (version !== materialVersion || raw !== scanCode.value) return;
    scanCode.value = material;
    locations.value = rows || [];
    queried.value = true;
    if (locations.value.length === 1) await selectCell(locations.value[0].cellCode);
  } catch (error: any) {
    if (version !== materialVersion) return;
    locations.value = []; queried.value = false;
    notice.value = error?.message || '查询库位失败'; noticeType.value = 'error';
  } finally { if (version === materialVersion) searching.value = false; }
}

/** 读取所选格位现有绑定；未创建的标准格位可通过绑定操作补建。 */
async function selectCell(value?: string) {
  if (busy.value) return;
  cellCode.value = (value || '').toUpperCase();
  cabinet.value = undefined;
  notice.value = '';
  const version = ++cellVersion;
  reading.value = true;
  try {
    if (!cellCode.value) return;
    const result = await getRemainderCabinet(cellCode.value);
    if (version === cellVersion) cabinet.value = result;
  } catch (error: any) {
    if (version === cellVersion) {
      notice.value = (error?.message || '未读取到柜位') + '；绑定时服务端会核对并在零头柜区补建缺失格位';
      noticeType.value = 'warning';
    }
  } finally { if (version === cellVersion) reading.value = false; }
}

/** 绑定和解绑均由服务端在柜位锁内确认空柜，已有库存时不能更换物料。 */
async function changeBinding(unbind: boolean) {
  if (busy.value || reading.value || searching.value) return;
  try {
    const materialCode = unbind ? cabinet.value?.materialCode : normalizeRemainderHistoryMaterial(scanCode.value);
    if (!cellCode.value || !materialCode) throw new Error('请提供物料编码并选择柜位');
    busy.value = true; emit('lock-change', true);
    const result = await (unbind ? unbindRemainderLocation : bindRemainderLocation)({ cellCode: cellCode.value, materialCode });
    notice.value = result.message || (result.success ? '操作完成' : '操作未完成');
    noticeType.value = result.success ? 'success' : 'warning';
    if (result.success) {
      // 提交已明确成功，刷新仍保持输入锁定；刷新失败不能误报为绑定结果未知。
      try {
        cabinet.value = await getRemainderCabinet(cellCode.value);
        const material = normalizeRemainderHistoryMaterial(scanCode.value);
        locations.value = material ? await getRemainderLocations(material) : [];
        queried.value = !!material;
      } catch { notice.value += '；列表刷新失败，请重新查询柜位'; noticeType.value = 'warning'; }
    }
  } catch (error: any) {
    notice.value = error?.message || '操作结果未确认，请重新读取柜位核对绑定'; noticeType.value = 'error';
  } finally { busy.value = false; emit('lock-change', false); }
}

onBeforeUnmount(() => { cellVersion++; materialVersion++; });
</script>

<style scoped>
.cabinet-binding { padding: 4px 16px 20px; }
label { display: block; font-weight: 600; margin: 14px 0 8px; }
.ant-btn, .ant-alert { margin-top: 12px; }
.actions { display: flex; gap: 12px; }
.actions .ant-btn { flex: 1; }
p { margin: 10px 0; }
</style>
