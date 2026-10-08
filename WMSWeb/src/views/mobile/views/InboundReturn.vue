<template>
  <div class="inbound-return-page">
    <Header numb="物料回库组盘" />

    <div class="section">
      <div class="field-label">收料条形码</div>
      <a-input
        ref="barcodeInputRef"
        v-model:value="barcode"
        placeholder="扫描完整收料条形码"
        :disabled="loading || submitting"
        allow-clear
        @keyup.enter="queryStocks"
      >
        <template #suffix><ScanOutlined /></template>
      </a-input>
      <a-button type="primary" block :loading="loading" :disabled="submitting" @click="queryStocks">
        查询库存库位
      </a-button>
    </div>

    <a-alert
      type="info"
      show-icon
      message="选择库位后，AGV会搬运该托盘的全部物料到入库区。"
      class="notice"
    />

    <div v-if="queriedBarcode" class="section">
      <div class="section-title">物料 {{ materialCode }} · 共 {{ cellRows.length }} 个库位</div>
      <a-table
        :data-source="cellRows"
        :columns="columns"
        :pagination="false"
        :loading="loading"
        :scroll="{ x: 940 }"
        :expanded-row-keys="expandedRowKeys"
        :custom-row="stockRow"
        :row-selection="rowSelection"
        row-key="id"
        size="small"
        @expand="expandCell"
      >
        <template #availability="{ record }">
          <a-tag :color="record.hasTask ? 'default' : 'green'">
            {{ record.hasTask ? '不可搬运' : '可搬运' }}
          </a-tag>
        </template>
        <template #palletType="{ record }">
          <a-tag :color="record.cellMaterialCount > 1 ? 'orange' : 'blue'">
            {{ record.cellMaterialCount == null ? '未统计' : record.cellMaterialCount > 1 ? '混托' : '单物料' }}
          </a-tag>
        </template>
        <template #boxCount="{ record }">{{ record.cellTotalBoxCount ?? '未记录' }}</template>
        <template #details="{ record }">
          <a-button type="link" size="small"
            @click.stop="expandCell(!expandedRowKeys.includes(record.id), record)">
            {{ expandedRowKeys.includes(record.id) ? '收起明细' : '展开明细' }}
          </a-button>
        </template>
        <template #expandedRowRender="{ record }">
          <div class="pallet-detail">
            <div class="section-title">库位 {{ record.cellCode }} · 全部物料明细</div>
            <a-table :data-source="record.cellStocks || []" :columns="detailColumns" :pagination="false"
              :scroll="{ x: 880 }" row-key="id" size="small">
              <template #materialCode="{ record: detail }">
                {{ detail.materialCode }} <a-tag v-if="detail.materialCode === materialCode" color="green">扫码物料</a-tag>
              </template>
              <template #stockBoxCount="{ record: detail }">{{ detail.totalPagOrBoxInTime ?? '未记录' }}</template>
            </a-table>
          </div>
        </template>
      </a-table>
      <div class="hint">混托库位默认展开全部物料，可点击“收起明细”折叠。总数量包含该库位的所有物料，单位见明细；箱数缺失显示“未记录”。有未完成任务或库位不可用时不能选择。</div>
    </div>

    <div v-if="selectedStock" class="section">
      <a-descriptions title="已选库存" :column="1" bordered size="small">
        <a-descriptions-item label="物料">{{ selectedStock.materialName }}</a-descriptions-item>
        <a-descriptions-item label="来源库位">{{ selectedStock.cellCode }}</a-descriptions-item>
        <a-descriptions-item label="容器">{{ selectedStock.boxCode }}</a-descriptions-item>
        <a-descriptions-item label="批次">{{ selectedStock.batchCode || '-' }}</a-descriptions-item>
        <a-descriptions-item label="数量">
          {{ selectedStock.totalCountInTime }} {{ selectedStock.unit }}
        </a-descriptions-item>
        <a-descriptions-item label="库位物料种类">{{ selectedStock.cellMaterialCount ?? '-' }}</a-descriptions-item>
        <a-descriptions-item label="库位总数量">{{ selectedStock.cellTotalCount ?? '-' }}（单位见明细）</a-descriptions-item>
        <a-descriptions-item label="库位总箱数">{{ selectedStock.cellTotalBoxCount ?? '未记录' }}</a-descriptions-item>
      </a-descriptions>
      <div class="field-label">目标入库库位</div>
      <a-input
        ref="targetInputRef"
        v-model:value="targetCellCode"
        placeholder="扫描或输入目标入库库位"
        :disabled="submitting"
        allow-clear
        @keyup.enter="submitTask"
      >
        <template #suffix><ScanOutlined /></template>
      </a-input>
      <a-button type="primary" block :disabled="!canSubmit" :loading="submitting" @click="submitTask">
        下发回库搬运任务
      </a-button>
    </div>

    <div v-if="successMessage" class="section">
      <a-alert type="success" show-icon :message="successMessage" description="等待AGV搬运完成后，可进入容器组盘继续操作。" />
      <a-button block @click="goToGrouping">进入容器组盘</a-button>
    </div>
    <a-button v-if="queriedBarcode || barcode || successMessage" block :disabled="loading || submitting" @click="reset">
      重新扫码
    </a-button>
  </div>
</template>

<script lang="ts" setup>
import { computed, nextTick, onMounted, ref, watch } from 'vue';
import { ScanOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
import { router } from '/@/router';
import Header from '../header/Header.vue';
import { createInboundReturnTask, getInboundReturnStocks, parseInboundReturnBarcode } from './InboundReturn';
import type { InboundReturnStock } from './InboundReturn';

/** 当前扫描输入与已查询条码分开保存，编辑扫码内容后必须重新查询。 */
const barcode = ref('');
const queriedBarcode = ref('');
const materialCode = ref('');
const stocks = ref<InboundReturnStock[]>([]);
const selectedStock = ref<InboundReturnStock | null>(null);
/** 展开的库位行ID，扫码查询时默认展开混托库位，人工可以收起。 */
const expandedRowKeys = ref<string[]>([]);
const targetCellCode = ref('');
const successMessage = ref('');
const loading = ref(false);
const submitting = ref(false);
const barcodeInputRef = ref<any>();
const targetInputRef = ref<any>();

const columns = [
  { title: '库位', dataIndex: 'cellCode', width: 100 },
  { title: '托盘', key: 'palletType', width: 80, slots: { customRender: 'palletType' } },
  { title: '扫码物料名称', dataIndex: 'materialName', width: 150 },
  { title: '物料种类', dataIndex: 'cellMaterialCount', width: 85 },
  { title: '库位总数量', dataIndex: 'cellTotalCount', width: 100 },
  { title: '库位总箱数', key: 'boxCount', width: 100, slots: { customRender: 'boxCount' } },
  { title: '容器', dataIndex: 'boxCode', width: 100 },
  { title: '状态', key: 'availability', width: 100, slots: { customRender: 'availability' } },
  { title: '物料明细', key: 'details', width: 110, slots: { customRender: 'details' } },
];

/** 整个位的库存明细，保持物料及批次独立显示，不能将其他物料替换成扫码物料。 */
const detailColumns = [
  { title: '物料编号', key: 'materialCode', width: 180, slots: { customRender: 'materialCode' } },
  { title: '物料名称', dataIndex: 'materialName', width: 150 },
  { title: '规格', dataIndex: 'specs', width: 100 },
  { title: '数量', dataIndex: 'totalCountInTime', width: 80 },
  { title: '单位', dataIndex: 'unit', width: 60 },
  { title: '箱数', key: 'stockBoxCount', width: 80, slots: { customRender: 'stockBoxCount' } },
  { title: '批次', dataIndex: 'batchCode', width: 110 },
  { title: '等级', dataIndex: 'grade', width: 60 },
  { title: '箱号', dataIndex: 'boxNumber', width: 80 },
];

/** 同库位多个命中批次只展示一行汇总，保留可选命中库存的ID供后端核对来源。 */
const cellRows = computed(() => {
  const rows = new Map<string, InboundReturnStock>();
  stocks.value.forEach(stock => {
    const key = stock.cellId || stock.cellCode || stock.id!;
    const existing = rows.get(key);
    if (!existing || (existing.hasTask && !stock.hasTask)) rows.set(key, stock);
  });
  return [...rows.values()];
});

/** 控制库位明细展开；展开信息不改变所选来源库存。 */
function expandCell(expanded: boolean, record: InboundReturnStock) {
  if (!record.id) return;
  expandedRowKeys.value = expanded
    ? [...new Set([...expandedRowKeys.value, record.id])]
    : expandedRowKeys.value.filter(id => id !== record.id);
}

const canSubmit = computed(() => {
  const stock = selectedStock.value;
  return !loading.value && !submitting.value && !!queriedBarcode.value
    && !!stock?.id && !stock.hasTask && !!targetCellCode.value.trim();
});

const rowSelection = computed(() => ({
  type: 'radio' as const,
  selectedRowKeys: selectedStock.value?.id ? [selectedStock.value.id] : [],
  getCheckboxProps: (record: InboundReturnStock) => ({ disabled: !!record.hasTask || loading.value || submitting.value }),
  onChange: (_keys: string[], rows: InboundReturnStock[]) => { if (rows[0]) selectStock(rows[0]); },
}));

/** 收料码变化立即使旧选择失效，避免把新条码和旧库存一起提交。 */
watch(barcode, () => {
  queriedBarcode.value = '';
  materialCode.value = '';
  stocks.value = [];
  expandedRowKeys.value = [];
  selectedStock.value = null;
  targetCellCode.value = '';
  successMessage.value = '';
}, { flush: 'sync' });

onMounted(() => barcodeInputRef.value?.focus());

/** 鼠标或触屏点击行与单选框使用同一选择逻辑。 */
function stockRow(record: InboundReturnStock) {
  return { onClick: () => selectStock(record) };
}

/** 人工选择可搬运库存后聚焦目标库位；任务执行期间不能切换来源。 */
async function selectStock(stock: InboundReturnStock) {
  if (loading.value || submitting.value) return;
  if (stock.hasTask) {
    message.warning('该库存有未完成任务或库位不可用，请选择其他库位');
    return;
  }
  selectedStock.value = stock;
  targetCellCode.value = '';
  await nextTick();
  targetInputRef.value?.focus();
}

/** 扫描完整收料码后列出命中库位及全部物料，混托明细默认展开，不自动选择搬运来源。 */
async function queryStocks() {
  if (loading.value || submitting.value) return;
  const parsed = parseInboundReturnBarcode(barcode.value);
  if (!parsed) {
    message.error('请扫描以英文逗号分隔的6段或8段完整收料条形码');
    return;
  }
  selectedStock.value = null;
  targetCellCode.value = '';
  stocks.value = [];
  queriedBarcode.value = '';
  successMessage.value = '';
  loading.value = true;
  expandedRowKeys.value = [];
  try {
    stocks.value = await getInboundReturnStocks(parsed.barcode);
    expandedRowKeys.value = cellRows.value.filter(row => (row.cellMaterialCount || 0) > 1)
      .map(row => row.id!).filter(Boolean);
    queriedBarcode.value = parsed.barcode;
    materialCode.value = parsed.materialCode;
    if (!stocks.value.length) message.warning('仓库中没有该物料可回库的库存');
  } catch (error: any) {
    message.error(error?.response?.data?.message || error?.message || '库存查询失败');
  } finally {
    loading.value = false;
  }
}

/** 提交查询时的库存身份及人工目标，防止重复点击；成功仅表示下发，不代表搬运完成。 */
async function submitTask() {
  if (!canSubmit.value || !selectedStock.value) return;
  const stock = selectedStock.value;
  const endCellCode = targetCellCode.value.trim();
  if (!stock.cellCode || !stock.boxCode) {
    message.error('所选库存未绑定有效库位或容器，请重新查询');
    return;
  }
  if (endCellCode === stock.cellCode) {
    message.error('目标入库库位不能与来源库位相同');
    return;
  }
  submitting.value = true;
  try {
    const result = await createInboundReturnTask({
      receivingMaterialBarcode: queriedBarcode.value,
      stockId: stock.id!,
      sourceCellCode: stock.cellCode,
      boxCode: stock.boxCode,
      endCellCode,
    });
    if (!result.success) {
      message.error(result.message || '回库搬运任务下发失败');
      return;
    }
    successMessage.value = result.message;
    // 当前整盘已被锁定，相同容器下的其他库存行同时禁止再次下发。
    stocks.value.forEach(item => { if (item.boxCode === stock.boxCode) item.hasTask = true; });
    selectedStock.value = null;
    targetCellCode.value = '';
    message.success(result.message);
  } catch (error: any) {
    message.error(error?.response?.data?.message || error?.message || '回库搬运任务下发失败');
  } finally {
    submitting.value = false;
  }
}

/** 重置查询并聚焦扫码框，供下一次人工回库申请使用。 */
async function reset() {
  if (loading.value || submitting.value) return;
  barcode.value = '';
  queriedBarcode.value = '';
  materialCode.value = '';
  stocks.value = [];
  expandedRowKeys.value = [];
  selectedStock.value = null;
  targetCellCode.value = '';
  successMessage.value = '';
  await nextTick();
  barcodeInputRef.value?.focus();
}

/** 搬运完成后由人工进入既有容器组盘页面继续操作。 */
function goToGrouping() {
  router.replace('/boxDisk');
}
</script>

<style scoped>
.inbound-return-page { min-height: 100vh; padding: 12px; background: #f5f5f5; }
.section { padding: 12px; margin-bottom: 12px; background: #fff; border-radius: 8px; }
.field-label, .section-title { margin: 8px 0; font-weight: 500; }
.section .ant-btn { margin-top: 12px; }
.notice { margin-bottom: 12px; }
.hint { margin-top: 8px; color: #888; font-size: 12px; }
.pallet-detail { padding: 8px; background: #fafafa; }
</style>
