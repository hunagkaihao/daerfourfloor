<template>
    <div class="box-disk-container">
        <Header numb="物料组盘"></Header>
        <U8MaterialInspection ref="inspectionRef" :disabled="submitting || barcodeEditorVisible || goods.length > 0"
            @loaded="onInspectionLoaded" @busy-change="inspectionBusy = $event" />
        <a-row class="input-row">
            <a-col :span="6">
                <div class="htext">
                    <h1 >绑定库位:</h1>
                </div>
            </a-col>
            <a-col :span="17">
                <a-input v-model:value="boxCode" :disabled="workflowBusy || goods.length > 0" placeholder="扫描库位" @keyup.enter="scanboxCode" ref="focus1"
                    :allowClear="true" @focus="focusFn" class="modern-input" autofocus>
                    <template #suffix>
                        <scan-outlined class="scan-icon" />
                    </template>
                </a-input>
            </a-col>
        </a-row>
        <LaneCellChips :list="laneCellStatusList" :current-cell-code="boxCode" />
        <div v-for="check in groupingQuantityChecks" :key="check.line.autoid" class="quantity-check">
            <div>物料 {{ check.line.cinvcode }} / ASN原批号 {{ check.line.cbatch || '-' }}</div>
            <div>应组 {{ check.expected }}；已保存 {{ check.grouped }}；本次待提交 {{ check.pending }}</div>
            <div>{{ check.grouped === check.expected ? '已保存数量与ASN一致' : check.remaining === 0 ? '本次提交后数量与ASN一致' : `还需扫码 ${check.remaining}` }}</div>
        </div>
        <a-row class="input-row">
            <a-col :span="6">
                <div class="htext">
                    <h1>收料条形码:</h1>
                </div>
            </a-col>
            <a-col :span="17">
                <a-input v-model:value="QRcode" :disabled="workflowBusy" placeholder="扫描收料条形码" @keyup.enter="scangoodsCode" :allowClear="true"
                    @focus="focusFn" class="modern-input" ref="focus2">
                    <template #suffix>
                        <scan-outlined class="scan-icon" />
                    </template>
                </a-input>
            </a-col>
        </a-row>

        <a-alert v-if="groupingResultMessages.length" class="grouping-result" type="info" show-icon
            message="组盘及采购入库结果">
            <template #description>
                <p v-for="(result, index) in groupingResultMessages" :key="index">{{ result }}</p>
            </template>
        </a-alert>
        <p style="margin-left: 20px">收料码数量:{{ goods.length }}</p>
        <div class="goods-list" :style="{
                height: goodheight + 'vh'
            }">
            <a-card v-for="(i, index) in goods" :key="i.asnBoxScanId" class="goods-card">
                <template #title>
                    {{ i.materialName }}
                </template>
                <template #extra>
                    <a-button type="primary" :disabled="workflowBusy" @click="deletegood(index)" :icon="h(DeleteOutlined)"></a-button>
                </template>
                <dl class="goods-info">
                    <div class="goods-field"><dt>物料编号</dt><dd>{{ i.materialCode }}</dd></div>
                    <div class="goods-field"><dt>箱码批号</dt><dd>{{ i.goodsBatchNo || '-' }}</dd></div>
                    <div class="goods-field"><dt>ASN原批号</dt><dd>{{ i.asnBatchCode || '-' }}</dd></div>
                    <div class="goods-field"><dt>工序</dt><dd>{{ i.processNo || '-' }}</dd></div>
                    <div class="goods-field"><dt>等级</dt><dd>{{ i.grade || '-' }}</dd></div>
                    <div class="goods-field goods-field--quantity"><dt>整箱数量</dt><dd>{{ i.quantity }}</dd></div>
                    <div class="goods-field"><dt>箱号</dt><dd>{{ i.boxNumber ?? '-' }}</dd></div>
                    <div class="goods-field goods-field--wide"><dt>采购订单</dt><dd>{{ i.orderCode }}</dd></div>
                    <div class="goods-field goods-field--quantity"><dt>组盘数量</dt><dd>{{ i.incellshu }}</dd></div>
                </dl>
                <a-button class="barcode-edit-button" :disabled="workflowBusy"
                    @click="openBarcodeEditor(i.asnBoxScanId)">更改物料字段</a-button>
            </a-card>
        </div>
        <AModal v-model:visible="barcodeEditorVisible" title="更改物料字段" width="94%"
            :style="{ maxWidth: '560px', top: '16px' }" :bodyStyle="{ maxHeight: '65vh', overflowY: 'auto' }"
            :maskClosable="false" :keyboard="false" :okText="editingScanId ? '保存修改' : '保存并录入一箱'"
            cancelText="取消" @ok="saveBarcodeFields" @cancel="closeBarcodeEditor">
            <div class="barcode-editor">
                <p>请按实物修改本箱字段，保存后按修改内容校验并组盘。</p>
                <div class="barcode-preview"><strong>修改前条码：</strong>{{ barcodeBeforeEdit }}</div>
                <label v-for="(field, index) in barcodeFieldLabels" :key="index" class="barcode-editor-field">
                    <span>{{ field }}</span>
                    <a-input v-model:value="barcodeEditFields[index]" :inputmode="index === 4 ? 'decimal' : 'text'"
                        allow-clear />
                </label>
                <div class="barcode-preview"><strong>修改后条码：</strong>{{ editedBarcode }}</div>
                <p v-if="barcodeEditError" class="barcode-edit-error" role="alert">{{ barcodeEditError }}</p>
            </div>
        </AModal>
        <div v-show="showtable">
            <p style="margin-left: 20px">已组盘信息:{{ dataSource.length }}</p>
            <a-table ref="tableRef" :dataSource="dataSource" :columns="diskcolumns" :pagination="false" rowKey="id"
                :scroll="{ x: screenWidth, y: 128 }">
                <template #bodyCell="{ column, record, index }">
                    <template v-if="column.key === 'operation'">
                        <!--<span @click="OpenGoodsDetail(record)" style="margin-right: 10px">查看</span>-->
                        <a-button type="link" danger :disabled="workflowBusy" @click="deleteStock(record)">删除</a-button>
                    </template>
                </template>
            </a-table>

            <div class="tab-bar">
                <a-button @click="openCancelModal" :disabled="workflowBusy" type="primary" class="modern-btn">
                    组盘取消
                </a-button>
                <a-button @click="confirmGrouping" :disabled="workflowBusy" :loading="submitting" type="primary" class="modern-btn">
                    组盘确认
                </a-button>
                <a-button @click="createInboundTask" :disabled="workflowBusy || goods.length > 0" :loading="submitting" type="primary" class="modern-btn">
                    创建入库搬运任务
                </a-button>
            </div>
        </div>
    </div>
</template>
<script lang="ts" setup>
import U8MaterialInspection from "../components/U8MaterialInspection.vue";
import { computed, ref, h, nextTick, onMounted, watch } from 'vue';
import { ScanOutlined, DeleteOutlined } from '@ant-design/icons-vue';
import { message, Modal as AModal } from 'ant-design-vue';
import { useMessage } from '/@/hooks/web/useMessage';
import { columns, stockRemoveDirect, stocksQuery, stocksGetInCell, stocksDisBindBox, createStockTask } from './Stock';
import { createAsnBoxItem } from './AsnBoxGrouping';
import { stockCreateAndBindBoxWithAsn } from './BoxDiskWithAsn';
import type { AsnMaterialInspectionLine } from './U8InboundApi';
import { getLaneCellStatusByCellCode } from '/@/views/warehouse/cells/Cell';
import LaneCellChips from '../components/LaneCellChips.vue';
import { PagedStockQueryDto, CellLaneStatusDto, StockCreateDto } from '/@/services/ServiceProxies';
import Header from '../header/Header.vue';

const diskcolumns = columns;
const { createConfirm } = useMessage();
const focus1 = ref<any>();
const focus2 = ref<any>();
/** 刷新物料检验单及其关联ASN的剩余组盘数量。 */
const inspectionRef = ref<{ reload: () => Promise<void> }>();
const QRcode = ref('');
/** 编辑的是当前输入或一条待提交箱码，取消时不改动原记录。 */
const barcodeEditorVisible = ref(false);
const editingScanId = ref('');
const barcodeBeforeEdit = ref('');
const barcodeEditFields = ref<string[]>([]);
const barcodeEditError = ref('');
/** 沿用箱码八段顺序，未明确业务含义的扩展段原样保留。 */
const barcodeFieldLabels = ['物料码', '箱码批号', '工序', '等级', '每箱数量', '扩展字段（第6段）', '扩展字段（第7段）', '箱号'];
const editedBarcode = computed(() => barcodeEditFields.value.map(value => value.trim()).join(','));
/** 沿用既有页面命名，实际存储扫描的库位编码，容器通过库位库存解析。 */
const boxCode = ref('');
const laneCellStatusList = ref<CellLaneStatusDto[]>([]);
/** 当前有效查询的ASN及来源明细，检验状态变化时由顶部组件同步。 */
const loadedAsnCode = ref('');
const loadedAsnLines = ref<AsnMaterialInspectionLine[]>([]);
/** 已扫码但尚未提交的实物箱，每箱独立计数，提交重试沿用原扫码记录标识。 */
const goods = ref<ReturnType<typeof createAsnBoxItem>[]>([]);
/** 保留各采购订单的组盘与自动推送结果，推送失败不等同于本地组盘失败。 */
const groupingResultMessages = ref<string[]>([]);
const dataSource = ref<any[]>([]);
/** 组盘、取消、删除和搬运共用操作锁，防止重复提交。 */
const submitting = ref(false);
const inspectionBusy = ref(false);
const queryingCell = ref(false);
const workflowBusy = computed(() => submitting.value || inspectionBusy.value || queryingCell.value || barcodeEditorVisible.value);
/** 提交结果尚未确认的记录需保持内容不变，避免网络结果不明时修改重试数据。 */
const attemptedScanIds = new Set<string>();
/** 搬运请求失败时保留原容器，重试不得改为搬运同库位的另一托盘。 */
const transportBoxCode = ref('');
const screenHeight = ref(window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight);
const screenWidth = ref((window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth) - 8);
const showtable = ref(true);
const goodheight = ref(36);
const tableRef = ref<any>();

/** 按来源明细核对累计数量；区分已保存和本次待提交，反包后的箱码批号不影响累计。 */
const groupingQuantityChecks = computed(() => loadedAsnLines.value.filter(line => line.status !== 4).map(line => {
    const grouped = Number(line.alreadyStockInQuantity || 0);
    const expected = Number(line.iquantity || 0);
    const pending = goods.value.filter(item => item.asnLineId === line.autoid)
        .reduce((sum, item) => sum + Number(item.incellshu), 0);
    return { line, grouped, expected, pending, remaining: Math.max(0, expected - grouped - pending) };
}));

/** 组盘来源直接采用顶部选中检验单对应的ASN明细，不再单独展示明细选择框。 */
function onInspectionLoaded(code: string, lines: AsnMaterialInspectionLine[]) {
    loadedAsnCode.value = code;
    loadedAsnLines.value = lines;
}

watch(boxCode, () => {
    dataSource.value = [];
    laneCellStatusList.value = [];
    transportBoxCode.value = '';
}, { flush: 'sync' });

onMounted(() => {
    window.onresize = () => {
        const height = window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight;
        showtable.value = height >= screenHeight.value;
        goodheight.value = showtable.value ? 36 : 60;
    };
    setTimeout(() => focus1.value?.focus(), 100);
});

/** 刷新指定库位库存与巷道状态，响应只写回仍在查看的库位。 */
async function refreshCell(code: string) {
    const params = new PagedStockQueryDto();
    params.cellCode = code;
    const [stocks, laneCells] = await Promise.all([
        stocksQuery(params),
        getLaneCellStatusByCellCode(code).catch((error: any) => {
            message.warning(error?.error?.message || error?.message || '查询同巷道库位失败');
            return [];
        }),
    ]);
    if (boxCode.value !== code) return;
    dataSource.value = stocks || [];
    laneCellStatusList.value = laneCells || [];
}

/** 扫描库位沿用既有的末10位编码规则，待提交箱码存在时不能换库位。 */
async function scanboxCode() {
    if (workflowBusy.value || goods.value.length) return;
    boxCode.value = boxCode.value.trim().slice(-10);
    if (!boxCode.value) return;
    queryingCell.value = true;
    dataSource.value = [];
    try {
        await refreshCell(boxCode.value);
        focus2.value?.focus();
    } catch (error: any) {
        message.error(error?.error?.message || error?.message || '查询库位库存失败');
    } finally { queryingCell.value = false; }
}

/** 每次扫码录入一箱，成功后先清空输入再恢复焦点，允许下一箱使用完全相同的标签。 */
async function scangoodsCode() {
    if (workflowBusy.value) return;
    const barcode = QRcode.value.trim();
    if (!barcode) return;
    if (!boxCode.value.trim() || !loadedAsnCode.value) {
        message.warning('请先按物料查询检验单、选择组盘来源并扫描库位'); return;
    }
    try {
        const checks = loadedAsnLines.value.map(line => ({
            asnLineId: line.autoid,
            canGroup: line.isInspectionReportPushed && line.materialInspectionStatus === 2,
        }));
        const scanId = globalThis.crypto?.randomUUID?.() ||
            Date.now().toString(36) + Math.random().toString(36).slice(2);
        goods.value.push(createAsnBoxItem(barcode, loadedAsnLines.value,
            checks, goods.value, scanId));
        QRcode.value = '';
        await nextTick();
        focus2.value?.focus();
    } catch (error: any) { message.error(error.message || '箱码校验失败'); }
}

/** 打开八段箱码编辑；传入扫码标识修改已有箱，否则编辑输入框内尚未录入的条码。 */
function openBarcodeEditor(scanId = '') {
    if (workflowBusy.value) return;
    if (scanId && attemptedScanIds.has(scanId)) {
        message.warning('该箱已提交过组盘，请先核对组盘结果，暂不能更改字段'); return;
    }
    const item = scanId ? goods.value.find(row => row.asnBoxScanId === scanId) : undefined;
    if (scanId && !item) return;
    const barcode = (item?.dataCode || QRcode.value).trim();
    if (!barcode) { message.warning('请先扫描或输入收料条形码'); return; }
    const fields = barcode.split(',').map(value => value.trim());
    while (fields.length < 8) fields.push('');
    editingScanId.value = scanId;
    barcodeBeforeEdit.value = barcode;
    barcodeEditFields.value = fields;
    barcodeEditError.value = '';
    barcodeEditorVisible.value = true;
}

/** 关闭弹窗并恢复收料扫码焦点，未保存的字段修改直接丢弃。 */
async function closeBarcodeEditor() {
    barcodeEditorVisible.value = false;
    editingScanId.value = '';
    barcodeEditError.value = '';
    await nextTick();
    focus2.value?.focus();
}

/** 重新解析修改后的箱码并校验来源、检验结果、数量和混托规则；原记录校验成功后才替换。 */
async function saveBarcodeFields() {
    if (!barcodeEditorVisible.value || submitting.value || inspectionBusy.value || queryingCell.value) return;
    barcodeEditError.value = '';
    try {
        if (!boxCode.value.trim() || !loadedAsnCode.value)
            throw new Error('请先选择检验单组盘来源并扫描库位，再保存箱码');
        if (barcodeEditFields.value.some(value => /[,，\r\n]/.test(value)))
            throw new Error('单个字段不能包含逗号或换行，请分别填写各段内容');
        const index = editingScanId.value
            ? goods.value.findIndex(item => item.asnBoxScanId === editingScanId.value) : -1;
        if (editingScanId.value && (index < 0 || attemptedScanIds.has(editingScanId.value)))
            throw new Error('该箱记录已变更或已提交，请关闭弹窗后核对组盘结果');
        const queued = goods.value.filter((_, itemIndex) => itemIndex !== index);
        const checks = loadedAsnLines.value.map(line => ({
            asnLineId: line.autoid,
            canGroup: line.isInspectionReportPushed && line.materialInspectionStatus === 2,
        }));
        // 编辑同一箱沿用扫码标识；从输入框修正后首次录入才生成新标识。
        const scanId = editingScanId.value || globalThis.crypto?.randomUUID?.() ||
            Date.now().toString(36) + Math.random().toString(36).slice(2);
        const item = createAsnBoxItem(editedBarcode.value, loadedAsnLines.value, checks, queued, scanId);
        if (index < 0) goods.value.push(item);
        else goods.value.splice(index, 1, item);
        QRcode.value = '';
        await closeBarcodeEditor();
        message.success(index < 0 ? '已按修改后的箱码录入一箱' : '物料字段已更新');
    } catch (error: any) {
        barcodeEditError.value = error?.message || '修改后的箱码校验失败';
    }
}

/** 移除尚未保存的箱码，不修改已组盘库存。 */
function deletegood(index: number) {
    if (!workflowBusy.value) goods.value.splice(index, 1);
}

/** 删除当前库位的指定库存后，同步刷新库存列表和ASN剩余数量。 */
function deleteStock(record: any) {
    if (workflowBusy.value) return;
    const code = boxCode.value;
    createConfirm({
        title: '确认删除', content: `您确定要删除物料 ${record.materialName} 的库存吗？`,
        okText: '确定', cancelText: '取消',
        onOk: async () => {
            if (workflowBusy.value || code !== boxCode.value) return;
            submitting.value = true;
            try {
                const result = await stockRemoveDirect(record.id);
                if (!result?.success) throw new Error(result?.message || '删除失败');
                message.success(result.message || '删除成功');
                await refreshCell(code);
                await inspectionRef.value?.reload();
            } catch (error: any) {
                message.error(error?.error?.message || error?.message || '删除失败');
            } finally { submitting.value = false; }
        },
    });
}

/** 保存组盘后由后端按本次单据独立核对账实并逐单推送采购入库；已保存订单移出队列，搬运另行操作。 */
async function confirmGrouping() {
    if (workflowBusy.value) return;
    if (!goods.value.length || !boxCode.value.trim()) {
        message.error('请先查询物料检验单、选择来源、扫描库位并逐箱扫码'); return;
    }
    submitting.value = true;
    groupingResultMessages.value = [];
    const code = boxCode.value;
    try {
        if (!loadedAsnCode.value) await inspectionRef.value?.reload();
        const asnCode = loadedAsnCode.value;
        if (!asnCode) throw new Error('尚未选择有效的检验单组盘来源，请重试');
        const orders = new Map<string, typeof goods.value>();
        for (const item of goods.value) {
            if (!item.orderCode) throw new Error('箱码来源缺少采购订单');
            if (!orders.has(item.orderCode)) orders.set(item.orderCode, []);
            orders.get(item.orderCode)!.push(item);
        }
        for (const [orderCode, boxes] of orders) {
            const dtos = boxes.map(item => {
                const dto = new StockCreateDto();
                dto.totalCount = item.incellshu;
                dto.barcode = item.materialCode;
                dto.materialCode = item.materialCode;
                dto.boxNumber = item.boxNumber;
                dto.receivePkgOrBoxCount = 1;
                dto.countInOnePkgOrBox = item.countInOnePkgOrBox;
                dto.batchCode = item.goodsBatchNo;
                dto.grade = item.grade;
                dto.processNo = item.processNo;
                dto.asnCode = asnCode;
                (dto as any).asnLineId = item.asnLineId;
                (dto as any).asnBoxScanId = item.asnBoxScanId;
                return dto;
            });
            boxes.forEach(item => attemptedScanIds.add(item.asnBoxScanId));
            const result = await stockCreateAndBindBoxWithAsn(code, orderCode, dtos);
            // 服务端明确拒绝时可继续修正字段；网络中断或未确认的响应则保留原记录重试。
            if (result?.success === false) boxes.forEach(item => attemptedScanIds.delete(item.asnBoxScanId));
            if (!result?.success) throw new Error(result?.message || '组盘失败');
            groupingResultMessages.value.push(`采购订单 ${orderCode}：${result.message || '组盘已保存，未返回采购入库结果，请核对推送状态'}`);
            const saved = new Set(boxes.map(item => item.asnBoxScanId));
            goods.value = goods.value.filter(item => !saved.has(item.asnBoxScanId));
            saved.forEach(scanId => attemptedScanIds.delete(scanId));
        }
        QRcode.value = '';
        message.success('组盘已保存，请查看组盘及采购入库结果');
    } catch (error: any) {
        const errorMessage = error?.error?.message || error?.message || '组盘失败，请核对已组盘信息后重试';
        groupingResultMessages.value.push(errorMessage);
        message.error(errorMessage);
    } finally {
        try { await refreshCell(code); }
        catch (error: any) { message.warning(error?.message || '组盘库存刷新失败，请重新扫描库位'); }
        try { await inspectionRef.value?.reload(); }
        finally { submitting.value = false; }
    }
}

/** 完成取消或搬运后清空当前托盘，保留顶部ASN检验查询结果。 */
function resetPallet() {
    goods.value = [];
    groupingResultMessages.value = [];
    attemptedScanIds.clear();
    QRcode.value = '';
    boxCode.value = '';
    dataSource.value = [];
    laneCellStatusList.value = [];
    transportBoxCode.value = '';
    focus1.value?.focus();
}

/** 从库位库存解析唯一实际容器；正在搬运的库存不能再次取消或创建任务。 */
async function getCellBox(code: string) {
    const stocks = await stocksGetInCell(code);
    if ((stocks || []).some((row: any) => row.hasTask)) throw new Error('当前库位已有搬运任务，请先核对任务状态');
    const boxes = [...new Set((stocks || []).map((row: any) => row.boxCode).filter(Boolean))] as string[];
    if (!stocks?.length) return '';
    if (boxes.length !== 1 || stocks.some((row: any) => !row.boxCode))
        throw new Error('当前库位未找到唯一容器，请核对库位库存');
    return boxes[0];
}

/** 取消当前托盘已保存的组盘，并清空尚未提交的扫码队列。 */
function openCancelModal() {
    if (workflowBusy.value) return;
    if (!boxCode.value.trim()) { message.error('请先扫描库位'); return; }
    const code = boxCode.value;
    createConfirm({
        title: '确认取消组盘',
        content: '确定取消当前库位托盘的组盘？将删除该托盘已保存的组盘库存、回退对应ASN已组盘数量，并清空尚未提交的箱码。',
        okText: '确定', cancelText: '取消',
        onOk: async () => {
            if (workflowBusy.value || code !== boxCode.value) return;
            submitting.value = true;
            try {
                const box = await getCellBox(code);
                if (box) {
                    const result = await stocksDisBindBox(box);
                    if (!result?.success) throw new Error(result?.message || '取消组盘失败');
                } else if (!goods.value.length) throw new Error('没有已组盘信息或待提交箱码');
                resetPallet();
                loadedAsnCode.value = '';
                loadedAsnLines.value = [];
                message.success('组盘已取消，当前托盘组盘数量已清除');
                try { await inspectionRef.value?.reload(); }
                catch (error: any) { message.warning(error?.message || '组盘已取消，ASN数量刷新失败，请重新查询'); }
            } catch (error: any) {
                message.error(error?.error?.message || error?.message || '取消组盘失败');
            } finally { submitting.value = false; }
        },
    });
}

/** 独立创建当前托盘的入库搬运任务；失败保留组盘数据，仅重试任务接口。 */
async function createInboundTask() {
    if (workflowBusy.value) return;
    if (!boxCode.value.trim()) { message.error('请先扫描库位'); return; }
    if (goods.value.length) { message.warning('请先确认或移除尚未提交的箱码'); return; }
    submitting.value = true;
    const code = boxCode.value;
    try {
        const box = await getCellBox(code);
        if (!box) throw new Error('当前库位没有已组盘的容器');
        if (transportBoxCode.value && transportBoxCode.value !== box)
            throw new Error('当前库位容器已变更，请核对原托盘搬运情况');
        transportBoxCode.value = box;
        const result = await createStockTask(box, code, '');
        if (!result?.success) throw new Error(result?.message || '任务接口未确认成功');
        message.success(result.message || '入库搬运任务已创建');
        resetPallet();
    } catch (error: any) {
        message.error(error?.error?.message || error?.message || '搬运任务创建失败，请核对任务状态后重试');
    } finally { submitting.value = false; }
}

/** 扫码输入时临时抑制软键盘弹出。 */
function focusFn(e: any) {
    e.target.setAttribute('readonly', 'readonly');
    setTimeout(() => e.target.removeAttribute('readonly'), 200);
}
</script>

<style scoped lang="less">

/* 主容器样式 */
.box-disk-container {
    min-height: 100vh;
    background: #ffffff;
    padding: 0 0 72px;
    position: relative;
    overflow-x: hidden;
}

/* 输入行样式 */
.input-row {
    margin: 10px 0;
    padding: 0 16px;
}

/* 按ASN来源展示数量核对，区分已落账数量和当前扫码队列。 */
.quantity-check {
    margin: 0 16px 10px;
    padding: 8px 10px;
    border: 1px solid #e8e8e8;
    border-radius: 4px;
    background: #fafafa;
    font-size: 13px;
    line-height: 1.6;
    overflow-wrap: anywhere;
}

/* 采购入库结果持续展示，避免PDA上的短暂提示消失后无法核对失败原因。 */
.grouping-result {
    margin: 0 16px 10px;
    overflow-wrap: anywhere;

    p { white-space: pre-line; }
}

/* 文字标签样式 */
.htext {
    text-align: center;
    line-height: 32px;
    
    h1 {
        color: #333333;
        font-size: 14px;
        font-weight: 500;
        margin: 0;
        letter-spacing: 0.3px;
    }
}

/* 现代化输入框样式 */
.modern-input {
    height: 32px;
    border-radius: 6px !important;
    border: 1px solid #d9d9d9 !important;
    background: #ffffff !important;
    transition: all 0.2s ease !important;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1) !important;
    
    &:focus,
    &:hover {
        border-color: #1890ff !important;
        box-shadow: 0 1px 6px rgba(24, 144, 255, 0.2) !important;
    }
    
    &::placeholder {
        color: rgba(0, 0, 0, 0.45) !important;
        font-weight: 400;
    }
}

/* 扫描图标样式 */
.scan-icon {
    color: #1890ff !important;
    font-size: 18px;
    transition: all 0.2s ease;
    
    &:hover {
        color: #40a9ff !important;
    }
}

/* 现代化按钮样式 */
.modern-btn {
    margin: auto;
    height: 32px !important;
    border-radius: 6px !important;
    background: #1890ff !important;
    border: none !important;
    font-size: 14px !important;
    font-weight: 500 !important;
    color: white !important;
    box-shadow: none !important;
    transition: all 0.2s ease !important;
    
    &:hover {
        background: #40a9ff !important;
        box-shadow: none !important;
    }
    
    &:active {
        background: #096dd9 !important;
    }
}

/* 底部操作栏样式 */
.tab-bar {
    display: flex;
    align-items: center;
    position: fixed;
    left: 0;
    right: 0;
    bottom: 0;
    height: 60px;
    background: #ffffff;
    border-top: 1px solid #f0f0f0;
    box-shadow: none;
    padding-bottom: 10px;
    z-index: 1000;
}

::v-deep(.ant-table-thead > tr > th) {
    padding: 5px 0px;

}

::v-deep(.ant-table-tbody > tr > td) {
    padding: 5px 0px;
}

::v-deep(.ant-card-head) {
    padding: 0px;
    font-size: 14px;
    min-height: 0px;
}

::v-deep(.ant-card-head-title) {
    padding: 0px;
    white-space: normal;
}

::v-deep(.ant-card-extra) {
    padding: 0px;
}

::v-deep(.ant-card-body) {
    padding: 4px 10px 6px;
}

/* 卡片字段独立换行，长批号和订单号在各自区域内折行，避免PDA窄屏文字重叠。 */
.goods-list {
    overflow-y: auto;
    overflow-x: hidden;
    padding: 0 8px 8px;
}
.goods-card {
    margin: 10px 0 16px;

    :deep(.ant-card-head-wrapper) { align-items: flex-start; gap: 8px; }
    :deep(.ant-card-head-title) { min-width: 0; line-height: 1.6; overflow-wrap: anywhere; }
    :deep(.ant-card-extra) { flex-shrink: 0; }
}
/* 编辑入口与字段表单分行显示，窄屏和软键盘弹出时仍可滚动填写。 */
.barcode-edit-button {
    margin-top: 8px;
}
.barcode-editor {
    display: grid;
    gap: 12px;
}
.barcode-editor-field {
    display: grid;
    gap: 4px;
    min-width: 0;
}
.barcode-preview {
    padding: 8px;
    background: #fafafa;
    line-height: 1.6;
    overflow-wrap: anywhere;
}
.barcode-edit-error {
    color: #ff4d4f;
    line-height: 1.6;
    overflow-wrap: anywhere;
}
.goods-info {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 6px 16px;
    margin: 0;
    padding: 4px 0;
}
.goods-field {
    display: grid;
    grid-template-columns: 5.5em minmax(0, 1fr);
    align-items: start;
    gap: 6px;
    min-width: 0;
    line-height: 1.6;

    dt, dd { margin: 0; min-width: 0; overflow-wrap: anywhere; }
    dt { font-weight: normal; color: #666; }
    &--wide { grid-column: 1 / -1; }
    &--quantity dd { font-weight: 600; }
}
@media (max-width: 480px) {
    .goods-info { grid-template-columns: minmax(0, 1fr); }
}

::v-deep(.ant-input-number-input) {
    height: 22px;
}

::v-deep(.ant-table-header.ant-table-hide-scrollbar) {
    margin-bottom: -20px;
    padding-bottom: 10px;
    overflow: scroll;
    opacity: 1;
}

::v-deep(.ant-table-hide-scrollbar) {
    scrollbar-color: initial !important;
}
::v-deep(.ant-table-placeholder){
    padding: 0 0px;
}
p {
    margin-bottom: 0em
}
</style>
