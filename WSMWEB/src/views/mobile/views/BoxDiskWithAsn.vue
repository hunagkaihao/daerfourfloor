<template>
    <div class="box-disk-container">
        <Header numb="组盘(ASN)"></Header>
        <a-row class="input-row">
            <a-col :span="6">
                <div class="htext">
                    <h1>绑定库位:</h1>
                </div>
            </a-col>
            <a-col :span="17">
                <a-input v-model:value="boxCode" placeholder="扫描库位" @keyup.enter="scanboxCode" ref="focus1"
                    :allowClear="true" @focus="focusFn" class="modern-input" autofocus>
                    <template #suffix>
                        <scan-outlined class="scan-icon" />
                    </template>
                </a-input>
            </a-col>
        </a-row>
        <LaneCellChips :list="laneCellStatusList" :current-cell-code="boxCode" />
        <a-row class="input-row">
            <a-col :span="6">
                <div class="htext">
                    <h1>ASN码:</h1>
                </div>
            </a-col>
            <a-col :span="17">
                <a-input v-model:value="asnCode" placeholder="输入ASN码" @keyup.enter="loadAsnData" :allowClear="true"
                    @focus="focusFn" class="modern-input" ref="asnInputRef">
                    <template #suffix>
                        <search-outlined class="scan-icon" @click="loadAsnData" />
                    </template>
                </a-input>
            </a-col>
        </a-row>

        <div v-if="asnOrderGroups.length > 0" class="asn-info">
            <div class="asn-info__title">已加载ASN订单（{{ asnOrderGroups.length }}单）</div>
            <div v-for="group in asnOrderGroups" :key="group.orderCode" class="asn-group">
                <div class="asn-group__header">订单：{{ group.orderCode }}（{{ group.items.length }}种物料）</div>
                <div style="overflow-x: auto; white-space: nowrap;">
                <a-table :columns="asnOrderColumns" :dataSource="group.items" :pagination="false"
                    size="small" bordered rowKey="rowKey" :scroll="{ x: 500 }">
                    <template #action>
                        <a-button type="primary" size="small" @click="selectAsnOrder(group)">选择</a-button>
                    </template>
                </a-table>
                </div>
            </div>
        </div>

        <p style="margin-left: 20px">收料码数量:{{ goods.length }}</p>
        <div style="overflow:auto;" :style="{ height: goodheight + 'vh' }">
            <a-card style="margin: 10px 5px 20px 5px" v-for="(i, index) in goods">
                <template #title>{{ i.materialName }}</template>
                <template #extra>
                    <a-button type="primary" @click="deletegood(index)" :icon="h(DeleteOutlined)"></a-button>
                </template>
                <div class="goods-info">
                    <a-row><a-col :span="12"><p>物料编号:{{ i.materialCode }}</p></a-col>
                           <a-col :span="12"><p>批次号:{{ i.goodsBatchNo || '-' }}</p></a-col></a-row>
                    <a-row><a-col :span="12"><p>工序号:{{ i.processNo || '-' }}</p></a-col>
                           <a-col :span="12"><p>等级:{{ i.grade || '-' }}</p></a-col></a-row>
                    <a-row class="goods-info-bold">
                        <a-col :span="12"><p>整箱数量:{{ i.quantity || '-' }}</p></a-col>
                        <a-col :span="12"><p>箱号:{{ i.boxNumber ?? '-' }}</p></a-col>
                    </a-row>
                    <a-row class="goods-input-row" align="middle">
                        <a-col :span="12">
                            <a-row align="middle">
                                <a-col :span="9"><p>入库包数:</p></a-col>
                                <a-col :span="9"><a-input-number v-model:value="i.baoshu" :min="0" @change="scanbaoshu(index)" /></a-col>
                            </a-row>
                        </a-col>
                        <a-col :span="12">
                            <a-row align="middle">
                                <a-col :span="9"><p>散件数量:</p></a-col>
                                <a-col :span="9"><a-input-number v-model:value="i.sanjianshu" :min="0" @change="scanbaoshu(index)" /></a-col>
                            </a-row>
                        </a-col>
                    </a-row>
                    <a-row class="goods-input-row" align="middle">
                        <a-col :span="12">
                            <a-row align="middle">
                                <a-col :span="9"><p>入库数量:</p></a-col>
                                <a-col :span="9"><a-input-number size="large" v-model:value="i.incellshu" :min="0" @change="scanincellshu(index)" /></a-col>
                            </a-row>
                        </a-col>
                    </a-row>
                </div>
            </a-card>
        </div>
        <div v-show="showtable">
            <p style="margin-left: 20px">已组盘信息:{{ dataSource.length }}</p>
            <a-table ref="tableRef" :dataSource="dataSource" :columns="diskcolumns" :pagination="false"
                :scroll="{ x: screenWidth, y: 128 }">
                <template #bodyCell="{ column, record, index }">
                    <template v-if="column.key === 'operation'">
                        <span @click="deleteStock(record, index)" style="color: #ff4d4f">删除</span>
                    </template>
                </template>
            </a-table>
            <div class="tab-bar">
                <a-button @click="openCancelModal" type="primary" class="modern-btn">组盘取消</a-button>
                <a-button @click="incell" type="primary" class="modern-btn">组盘确认</a-button>
            </div>
        </div>
    </div>
</template>
<script lang="ts" setup>
import { ref, h, onMounted, computed } from 'vue';
import { SearchOutlined, ScanOutlined, DeleteOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
import { useMessage } from '/@/hooks/web/useMessage';
import { materialsWithCodeTipGet, DataItem, stockCreateAndBindBox, validateAsn } from '/@/views/mobile/views/Material';
import { stockCreateAndBindBoxWithAsn } from './BoxDiskWithAsn';
import { stocksGetInBox, columns, stocksDisBindBox, stockRemoveDirect, stocksQuery } from '../views/Stock';
import { getLaneCellStatusByCellCode } from '/@/views/warehouse/cells/Cell';
import LaneCellChips from '../components/LaneCellChips.vue';
import { PagedStockQueryDto, CellLaneStatusDto, StockCreateDto } from '/@/services/ServiceProxies';
import Header from '../header/Header.vue'

const diskcolumns = columns;
const { createConfirm } = useMessage();

const focus1 = ref<any>();
const focus2 = ref<any>();
const asnInputRef = ref<any>();
let QRcode = ref<string>('');
let boxCode = ref<string>('');
let asnCode = ref<string>('');
let loadingAsn = ref(false);
let asnCodeValidated = ref('');
const laneCellStatusList = ref<CellLaneStatusDto[]>([]);

const fetchLaneCellStatus = async () => {
    if (!boxCode.value.trim()) { laneCellStatusList.value = []; return; }
    try { const r = await getLaneCellStatusByCellCode(boxCode.value); laneCellStatusList.value = r || []; } catch { laneCellStatusList.value = []; }
};

interface AsnOrderItem { rowKey: string; materialCode: string; materialName: string; specs: string; quantity: number; }
interface AsnOrderGroup { orderCode: string; items: AsnOrderItem[]; }
const asnOrderGroups = ref<AsnOrderGroup[]>([]);
const asnOrderColumns = [
    { title: '物料编码', dataIndex: 'materialCode', key: 'materialCode', align: 'center', width: 100 },
    { title: '物料名称', dataIndex: 'materialName', key: 'materialName', align: 'left', width: 140, ellipsis: true },
    { title: '规格', dataIndex: 'specs', key: 'specs', align: 'center', width: 80 },
    { title: '数量', dataIndex: 'quantity', key: 'quantity', align: 'center', width: 60 },
    { title: '操作', key: 'action', width: 60, align: 'center', slots: { customRender: 'action' } },
];

async function loadAsnData() {
    if (!asnCode.value.trim()) return;
    loadingAsn.value = true;
    try {
        const res = await validateAsn(asnCode.value.trim());
        if (res && res.success && res.data && res.data.length > 0) {
            const groupMap = new Map<string, AsnOrderItem[]>();
            res.data.forEach((item: any) => {
                // 过滤已入库数量 >= 应入库数量的已完成项
                const alreadyIn = item.alreadyStockInQuantity || 0;
                const planQty = item.iquantity || 0;
                if (alreadyIn >= planQty) return;

                const orderCode = item.cordercode || item.ccode || 'default';
                if (!groupMap.has(orderCode)) groupMap.set(orderCode, []);
                groupMap.get(orderCode)!.push({
                    rowKey: `${orderCode}_${item.cinvcode}_${Date.now()}`,
                    materialCode: item.cinvcode || '',
                    materialName: item.cinvname || '',
                    specs: item.cinvstd || '',
                    quantity: planQty - alreadyIn,
                });
            });
            asnOrderGroups.value = Array.from(groupMap.entries()).map(([orderCode, items]) => ({ orderCode, items }));
            asnCodeValidated.value = asnCode.value.trim();
            message.success(`ASN ${asnCode.value.trim()} 已加载，共 ${asnOrderGroups.value.length} 单`);
            asnCode.value = '';
        } else { message.warning(res?.message || '未找到ASN数据'); }
    } finally { loadingAsn.value = false; }
}

function selectAsnOrder(group: AsnOrderGroup) {
    group.items.forEach((item) => {
        const g = new DataItem();
        g.materialCode = item.materialCode;
        g.materialName = item.materialName;
        g.goodsSpec = item.specs;
        g.quantity = item.quantity;
        g.incellshu = item.quantity;
        g.baoshu = 0; g.sanjianshu = 0;
        g.countInOnePkgOrBox = item.quantity;
        g.dataCode = `${item.materialCode}_${Date.now()}`;
        g.goodsBatchNo = group.orderCode;
        (goods.value as any[]).push(g);
    });
    asnOrderGroups.value = asnOrderGroups.value.filter(g => g.orderCode !== group.orderCode);
}

var goods = ref<any[]>([]);
var dataSource = ref<any[]>([]);
let screenHeight = ref(window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight);
let screenWidth = ref((window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth) - 8)
let YHOne = ref();
let showtable = ref(true)
let goodheight = ref(10)
const tableRef = ref<any>();
onMounted(() => {
    YHOne.value = screenHeight.value - 42 - tableRef.value.$el.querySelector('.ant-table-thead').clientHeight;
    window.onresize = () => {
        var showHeight = window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight
        if (showHeight - screenHeight.value >= 0) { showtable.value = true; goodheight.value = 36 }
        else { showtable.value = false; goodheight.value = 60 }
    }
    setTimeout(() => { if (focus1.value) focus1.value.focus(); }, 100);
})

const scanboxCode = async () => {
    if (boxCode.value.length > 10) boxCode.value = boxCode.value.slice(boxCode.value.length - 10, boxCode.value.length)
    await fetchLaneCellStatus();
    var params = new PagedStockQueryDto();
    params.cellCode = boxCode.value;
    await stocksQuery(params).then((res) => { dataSource.value.length = 0; res.forEach((e: any) => dataSource.value.push(e)); }).catch(() => {});
    setTimeout(() => { if (focus2.value) focus2.value.focus(); }, 100);
}

function scanbaoshu(index: number) {
    if (goods.value[index].countInOnePkgOrBox) { goods.value[index].incellshu = goods.value[index].baoshu * goods.value[index].countInOnePkgOrBox + goods.value[index].sanjianshu }
}
function scanincellshu(index: number) {
    if (goods.value[index].countInOnePkgOrBox) { goods.value[index].sanjianshu = goods.value[index].incellshu % goods.value[index].countInOnePkgOrBox; goods.value[index].baoshu = (goods.value[index].incellshu - goods.value[index].sanjianshu) / goods.value[index].countInOnePkgOrBox; }
}
const deletegood = (index: number) => goods.value.splice(index, 1)
const deleteStock = async (record: any, index: number) => {
    createConfirm({ title: '确认删除', content: `删除物料 ${record.materialName} 的库存？`, okText: '确定', cancelText: '取消',
        onOk: async () => { try { await stockRemoveDirect(record.id); message.success('删除成功'); dataSource.value.splice(index, 1); if (boxCode.value) await scanboxCode(); } catch { message.error('删除失败'); } } });
};

const incell = async () => {
    let stockCreateDto: StockCreateDto[] = [];
    if (goods.value.length == 0) { message.error("没有物料信息"); return; }
    if (boxCode.value == '') { message.error("没有容器信息"); return; }
    goods.value.forEach((e: any) => {
        let p = new StockCreateDto(); p.totalCount = e.incellshu; p.barcode = e.materialCode; p.materialCode = e.materialCode;
        p.boxNumber = e.boxNumber || undefined; p.receivePkgOrBoxCount = e.baoshu || undefined;
        p.countInOnePkgOrBox = e.countInOnePkgOrBox || undefined; p.batchCode = e.goodsBatchNo || '';
        p.grade = e.grade || ''; p.processNo = e.processNo || '';
        stockCreateDto.push(p);
    });
    try {
        if (asnCodeValidated.value) {
            const grouped = new Map<string, StockCreateDto[]>();
            stockCreateDto.forEach(dto => {
                const oc = (goods.value.find((g: any) => g.materialCode === dto.materialCode)?.goodsBatchNo) || asnCodeValidated.value;
                if (!grouped.has(oc)) grouped.set(oc, []);
                grouped.get(oc)!.push(dto);
            });
            let ok = true;
            for (const [oc, dtos] of grouped) { const res = await stockCreateAndBindBoxWithAsn(boxCode.value, oc, dtos); if (!res || res.success !== true) { ok = false; message.error(res?.message || '组盘失败'); } }
            if (ok) { message.success('组盘完成，ASN已更新'); goods.value.length = 0; QRcode.value = ''; laneCellStatusList.value = []; setTimeout(() => { if (focus1.value) focus1.value.focus(); }, 100); }
        } else {
            await stockCreateAndBindBox(boxCode.value, stockCreateDto).then((res) => {
                if (res.success == true) { message.success(res.message); goods.value.length = 0; QRcode.value = ''; boxCode.value = ''; laneCellStatusList.value = []; setTimeout(() => { if (focus1.value) focus1.value.focus(); }, 100); }
                else { message.error(res.message || '入库失败'); }
            }).catch((error) => { message.error(error.message || '操作失败'); });
        }
    } catch (err) { message.error(err); }
}

const openCancelModal = () => {
    if (boxCode.value == '') { message.error("没有容器信息"); return; }
    if (dataSource.value.length === 0) { message.error("没有已组盘信息"); return; }
    createConfirm({ title: '确认取消组盘', content: '确定取消？', okText: '确定', cancelText: '取消', onOk: () => { diskcancel(); } });
}
const diskcancel = async () => {
    if (boxCode.value == '') { message.error("没有容器信息"); return; }
    try { await stocksDisBindBox(boxCode.value).then((res) => { if (res.success == true) { message.success(res.message); goods.value.length = 0; QRcode.value = ''; boxCode.value = ''; laneCellStatusList.value = []; setTimeout(() => { if (focus1.value) focus1.value.focus(); }, 100); } else { message.error(res.message); } }).catch((error) => { message.error(error.error.message); }); } catch (err) { message.error(err); }
}
function focusFn(e: any) { e.target.setAttribute('readonly', 'readonly'); setTimeout(() => { e.target.removeAttribute('readonly'); }, 200); }
</script>
<style scoped>
.input-row { margin: 10px 0; padding: 0 16px; }
.htext { text-align: center; line-height: 32px; }
.htext h1 { color: #333; font-size: 14px; font-weight: 500; margin: 0; letter-spacing: 0.3px; }
.modern-input { height: 32px; border-radius: 6px !important; border: 1px solid #d9d9d9 !important; background: #fff !important; transition: all 0.2s ease !important; box-shadow: 0 1px 3px rgba(0,0,0,0.1) !important; }
.modern-input:focus, .modern-input:hover { border-color: #1890ff !important; box-shadow: 0 1px 6px rgba(24,144,255,0.2) !important; }
.scan-icon { color: #1890ff !important; font-size: 18px; }
.modern-btn { margin: auto; height: 32px !important; border-radius: 6px !important; background: #1890ff !important; border: none !important; font-size: 14px !important; font-weight: 500 !important; color: #fff !important; }
.tab-bar { display: flex; align-items: center; position: fixed; left: 0; right: 0; bottom: 0; height: 60px; background: #fff; border-top: 1px solid #f0f0f0; padding-bottom: 10px; z-index: 1000; }
::v-deep(.ant-table-thead > tr > th) { padding: 5px 0; }
::v-deep(.ant-table-tbody > tr > td) { padding: 5px 0; }
::v-deep(.ant-card-head) { padding: 0; font-size: 14px; min-height: 0; }
::v-deep(.ant-card-head-title) { padding: 0; white-space: normal; }
::v-deep(.ant-card-extra) { padding: 0; }
::v-deep(.ant-card-body) { padding: 4px 10px 6px; }
::v-deep(.ant-table-header.ant-table-hide-scrollbar) { margin-bottom: -20px; padding-bottom: 10px; overflow: scroll; opacity: 1; }
::v-deep(.ant-table-hide-scrollbar) { scrollbar-color: initial !important; }
::v-deep(.ant-table-placeholder) { padding: 0; }
p { margin-bottom: 0; }
.goods-info { padding: 0 4px; }
.goods-info p { margin-bottom: 2px; line-height: 1.3; }
.goods-info-bold p { font-weight: 600; color: #262626; margin-bottom: 2px; }
.goods-input-row p { margin-bottom: 0; }
.asn-info { margin: 8px 12px; background: #fff; border-radius: 6px; padding: 8px 12px; box-shadow: 0 1px 4px rgba(0,0,0,0.06); max-height: 20vh; overflow-y: auto; }
.asn-info__title { font-weight: 600; margin-bottom: 4px; color: #333; font-size: 13px; position: sticky; top: 0; background: #fff; z-index: 1; padding-bottom: 2px; }
:deep(.asn-group) { margin-bottom: 4px; }
:deep(.asn-group .ant-table) { font-size: 11px; }
:deep(.asn-group .ant-table-thead > tr > th) { padding: 1px 4px !important; font-size: 11px; line-height: 1.4; }
:deep(.asn-group .ant-table-tbody > tr > td) { padding: 1px 4px !important; font-size: 11px; line-height: 1.4; }
:deep(.asn-group__header) { font-size: 12px; padding: 4px 8px; }
:deep(.asn-group .ant-btn-sm) { font-size: 11px; padding: 0 6px; height: 20px; line-height: 20px; }
.asn-info__title { font-weight: 600; margin-bottom: 8px; color: #333; position: sticky; top: 0; background: #fff; z-index: 1; padding-bottom: 4px; }
.asn-group { margin-bottom: 8px; border: 1px solid #e8e8e8; border-radius: 4px; overflow: hidden; }
.asn-group__header { background: #f8f9fa; padding: 6px 12px; font-size: 13px; font-weight: 500; color: #555; border-bottom: 1px solid #e8e8e8; }
::v-deep(.ant-input-number-input) { height: 22px; }
</style>