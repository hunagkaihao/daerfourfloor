<template>
  <div class="inventory-count-page">
    <Header numb="盘点" />
    <div class="inventory-count-form">
      <label>扫码物料码（必填）</label>
      <a-input ref="scanInput" v-model:value="barcode" :disabled="locked"
        placeholder="请扫描条码或输入物料编码" allow-clear @pressEnter="identifyBarcode" @blur="identifyBarcode" />
      <div class="material-info">
        <p>物料码：{{ material?.materialCode || '-' }}</p>
        <p>产商代号：{{ material?.manufacturerCode || '-' }}</p>
        <p>等级：{{ material?.grade || '-' }}</p>
      </div>
      <label>整箱数量（只/箱）</label>
      <a-input-number v-model:value="quantityPerBox" :disabled="locked" :min="1" :max="999999999" placeholder="请输入每箱只数" />
      <label>实际箱数（箱）</label>
      <a-input-number v-model:value="actualBoxes" :disabled="locked" :min="0" :max="999999999" placeholder="请输入实际整箱数，可为0" />
      <label>实际零头（只）</label>
      <a-input-number v-model:value="remainderQuantity" :disabled="locked" :min="0" :max="999999999" placeholder="请输入零头只数，可为0" />
      <div class="total">实际数量：<strong>{{ total === null ? '-' : total }}</strong> 只</div>
      <p class="hint">仅物料码必填，其余可留空。三项数量全部填写后计算实际数量，空值与0分别记录。</p>
      <p class="hint">实际数量 = 整箱数量 × 实际箱数 + 实际零头。</p>
      <a-alert v-if="notice" :message="notice" :type="noticeType" show-icon />
      <a-button type="primary" block :loading="saving" :disabled="saving || exporting" @click="submit">
        {{ pending ? '重试保存' : '提交保存' }}
      </a-button>
      <div class="export-section">
        <label>导出日期</label>
        <input v-model="exportDate" type="date" class="export-date" :disabled="exporting" />
        <p class="hint">按保存日期导出当天全部盘点明细。</p>
        <a-button block :loading="exporting" :disabled="saving || locked || !exportDate" @click="exportData">导出 Excel</a-button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, onMounted, ref } from 'vue';
import Header from '../header/Header.vue';
import { downloadByData } from '/@/utils/file/download';
import { exportInventoryCounts, saveInventoryCount } from './InventoryCountApi';
import type { InventoryCountInput } from './InventoryCountApi';
import { inventoryQuantities, inventoryToday, inventoryTotal, parseInventoryBarcode } from './InventoryCountHelpers';
import { newRemainderRequestId } from './RemainderCabinetHelpers';

const barcode = ref('');
const quantityPerBox = ref<number | undefined>();
const actualBoxes = ref<number | undefined>();
/** 实际零头默认0只；手动清空时仍按空值保存。 */
const remainderQuantity = ref<number | undefined>(0);
const exportDate = ref(inventoryToday());
const saving = ref(false);
const exporting = ref(false);
/** 保存结果未确认时锁定原始输入，只有同一笔数据可以重试。 */
const pending = ref<InventoryCountInput | null>(null);
const locked = computed(() => saving.value || pending.value !== null);
const notice = ref('');
const noticeType = ref<'success' | 'error' | 'warning'>('warning');
const scanInput = ref<any>();
const material = computed(() => {
  try { return parseInventoryBarcode(barcode.value); } catch { return null; }
});
const total = computed(() => inventoryTotal(quantityPerBox.value, actualBoxes.value, remainderQuantity.value));

/** 回车或离开扫码框时显示解析错误，输入过程中不弹窗。 */
function identifyBarcode() {
  if (!barcode.value.trim()) return;
  try { parseInventoryBarcode(barcode.value); }
  catch (error: any) { notice.value = error.message; noticeType.value = 'warning'; }
}

/** 首次提交生成稳定标识；超时后锁定原数据并以相同标识重试。 */
async function submit() {
  if (saving.value || exporting.value) return;
  if (!pending.value) {
    try {
      parseInventoryBarcode(barcode.value);
      const quantities = inventoryQuantities(quantityPerBox.value, actualBoxes.value, remainderQuantity.value);
      pending.value = { requestId: newRemainderRequestId(), barcode: barcode.value.trim(),
        quantityPerBox: quantities.quantityPerBox, actualBoxes: quantities.actualBoxes, remainderQuantity: quantities.remainderQuantity };
    } catch (error: any) {
      notice.value = error.message; noticeType.value = 'warning'; return;
    }
  }
  saving.value = true;
  try {
    const result = await saveInventoryCount({ ...pending.value! });
    if (!result.success) throw new Error(result.message || '盘点保存未完成');
    notice.value = result.message || '盘点保存成功'; noticeType.value = 'success';
    pending.value = null;
    barcode.value = ''; quantityPerBox.value = undefined; actualBoxes.value = undefined; remainderQuantity.value = 0;
    saving.value = false;
    await nextTick(); scanInput.value?.focus();
  } catch (error: any) {
    const status = error?.response?.status;
    // 明确的参数错误未落库，允许修正；超时、断网和服务器异常保留原请求，避免重复盘点。
    if (status >= 400 && status < 500) pending.value = null;
    notice.value = (error?.response?.data?.error?.message || error?.message || '保存失败') +
      (pending.value ? '；保存结果未确认，请点击“重试保存”，重试不会重复登记' : '');
    noticeType.value = 'error';
  } finally { saving.value = false; }
}

/** 下载所选保存日期的完整Excel；失败响应可能是Blob，解析业务提示后再显示。 */
async function exportData() {
  if (exporting.value || saving.value || locked.value || !exportDate.value) return;
  exporting.value = true;
  const date = exportDate.value;
  try {
    const file = await exportInventoryCounts(date);
    downloadByData(file, `盘点数据_${date}.xlsx`, 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet');
    notice.value = `${date}盘点数据已导出`; noticeType.value = 'success';
  } catch (error: any) {
    let message = error?.response?.data?.error?.message || error?.message || '导出失败';
    const data = error?.response?.data;
    if (data instanceof Blob) {
      try { message = JSON.parse(await data.text())?.error?.message || message; } catch { /* 保留原始错误提示。 */ }
    }
    notice.value = message; noticeType.value = 'error';
  } finally { exporting.value = false; }
}

onMounted(() => scanInput.value?.focus());
</script>

<style scoped>
.inventory-count-page { min-height: 100vh; background: #fff; }
.inventory-count-form { max-width: 600px; margin: 0 auto; padding: 16px; }
label { display: block; margin: 12px 0 6px; font-weight: 500; }
.ant-input-number { width: 100%; }
.material-info { margin-top: 12px; padding: 12px; background: #f5f7fa; border-radius: 4px; }
.material-info p { margin: 0 0 4px; overflow-wrap: anywhere; }
.total { margin-top: 20px; font-size: 20px; color: #1890ff; }
.hint { margin: 8px 0 12px; color: #666; font-size: 12px; }
.ant-alert { margin-bottom: 12px; }
.export-section { margin-top: 28px; padding-top: 12px; border-top: 1px solid #eee; }
.export-date { width: 100%; min-height: 36px; padding: 4px 10px; border: 1px solid #d9d9d9; border-radius: 2px; }
</style>
