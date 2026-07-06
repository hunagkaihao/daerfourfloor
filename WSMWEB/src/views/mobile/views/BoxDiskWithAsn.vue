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
        <a-row class="input-row">
            <a-col :span="6">
                <div class="htext">
                    <h1>收料条形码:</h1>
                </div>
            </a-col>
            <a-col :span="17">
                <a-input v-model:value="QRcode" placeholder="扫描收料条形码" @keyup.enter="scangoodsCode" :allowClear="true"
                    @focus="focusFn" class="modern-input" ref="focus2">
                    <template #suffix>
                        <scan-outlined class="scan-icon" />
                    </template>
                </a-input>
            </a-col>
        </a-row>

        <div v-if="selectedAsn" class="asn-info">
            <div class="asn-info__title">已选ASN</div>
            <div class="asn-group">
                <div class="asn-group__header">ASN：{{ selectedAsn.ccode || '-' }}</div>
                <a-table
                    :columns="selectedAsnColumns"
                    :dataSource="[selectedAsn]"
                    :pagination="false"
                    size="small"
                    bordered
                    rowKey="rowKey"
                >
                    <template #asnReselect>
                        <span class="asn-action-text" @click.stop="reselectAsn">重选</span>
                    </template>
                </a-table>
            </div>
        </div>

        <div v-else-if="groupedIncompleteAsnList.length > 0" class="asn-info">
            <div class="asn-info__title">
                未完成ASN（{{ incompleteAsnList.length }}条 / {{ groupedIncompleteAsnList.length }}单）
            </div>
            <div
                v-for="group in groupedIncompleteAsnList"
                :key="group.ccode"
                class="asn-group"
            >
                <div class="asn-group__header">ASN：{{ group.ccode }}</div>
                <a-table
                    :columns="incompleteAsnColumns"
                    :dataSource="group.items"
                    :pagination="false"
                    size="small"
                    bordered
                    rowKey="rowKey"
                >
                    <template #asnSelect="{ record }">
                        <span class="asn-action-text" @click.stop="selectAsn(record)">选择</span>
                    </template>
                </a-table>
            </div>
        </div>

        <p style="margin-left: 20px">收料码数量:{{ goods.length }}</p>
        <div style=" overflow:auto;" :style="{
                height: goodheight + 'vh'
            }">
            <a-card style="margin: 10px 5px 20px 5px" v-for="(i, index) in goods">
                <template #title>
                    {{ i.materialName }}{{ i.specs }}
                </template>
                <template #extra>
                    <a-button type="primary" @click="deletegood(index)" :icon="h(DeleteOutlined)"></a-button>
                </template>
                <a-row>
                    <a-col :span="12">
                        <p>箱号:{{ i.boxNumber }}</p>
                        <p>收料码:{{ i.dataCode }}</p>
                    </a-col>
                    <a-col :span="12">
                        <p>入库数量:<a-input-number size="large" v-model:value="i.incellshu" :min="0" /></p>
                    </a-col>
                </a-row>
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
                <a-button @click="openCancelModal" type="primary" class="modern-btn">
                    组盘取消
                </a-button>
                <a-button @click="incell" type="primary" class="modern-btn">
                    组盘确认
                </a-button>
            </div>
        </div>
    </div>
    <GoodsDetail @register="registerGoodsDetailModal"></GoodsDetail>
</template>

<script lang="ts" setup>
import { ref, h, computed, onMounted, onUnmounted } from 'vue';
import { ScanOutlined, DeleteOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
import { useMessage } from '/@/hooks/web/useMessage';
import { materialsWithCodeTipGet, DataItem, stockCreateAndBindBoxWithAsn, GoodsInBox, getIncompleteAsnByMaterialCode, IncompleteAsnItem, incompleteAsnColumns, selectedAsnColumns, groupIncompleteAsnByCode } from './BoxDiskWithAsn';
import { stocksQuery, stocksDisBindBox, stockRemoveDirect, diskcolumns } from '../views/Stock';
import { PagedStockQueryDto, StockCreateDto } from '/@/services/ServiceProxies';
import Header from '../header/Header.vue';
import { useModal } from '/@/components/Modal';
import moment from 'moment';

const { createConfirm } = useMessage();

const focus1 = ref<any>();
const focus2 = ref<any>();
let QRcode = ref<string>('');
let boxCode = ref<string>('');

var goods = ref<DataItem[]>([]);
var incompleteAsnList = ref<IncompleteAsnItem[]>([]);
var selectedAsn = ref<IncompleteAsnItem | null>(null);
const groupedIncompleteAsnList = computed(() => groupIncompleteAsnByCode(incompleteAsnList.value));
var dataSource = ref<any[]>([]);
let screenHeight = ref(window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight);
let screenWidth = ref((window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth) - 8);
let YHOne = ref();
let showtable = ref(true);
let goodheight = ref(36);
const tableRef = ref<any>();

onMounted(() => {
    setTimeout(() => {
        if (focus1.value) {
            focus1.value.focus();
        }
    }, 100);
});

const focusFn = (e: any) => {
    e.target.setAttribute('readonly', 'readonly');
    setTimeout(() => {
        e.target.removeAttribute('readonly');
    }, 200);
};

function formatQty(value?: number | null) {
    if (value === null || value === undefined || Number.isNaN(Number(value))) {
        return '0';
    }
    return String(Number(value));
}

function selectAsn(item: IncompleteAsnItem) {
    selectedAsn.value = { ...item, rowKey: item.rowKey || `${item.ccode}-${item.cordercode}` };
}

function reselectAsn() {
    selectedAsn.value = null;
}

// 扫码收料码
async function scangoodsCode() {
    if (QRcode.value.includes(',')) {
        const isDuplicate = goods.value.some(item => item.receivingMaterialBarcode === QRcode.value);
        if (isDuplicate) {
            message.error("该收料码已扫描，不能重复扫描");
            return;
        }
        const parts = QRcode.value.split(',');

        const dataCode = parts[0];
        const batchCode = parts[1] || '';
        let processNo = parts[2] || '';
        const grade = parts[3] || '';
        const supplierProductionDate = parts[6] || '';
        const boxNumber = parts[7] || '0';


        const processMap: Record<string, string> = {
            "11": "来料检验",
            "20": "热处理工序",
            "23": "双端面工序",
            "34": "工序34",
            "47": "工序47"
        };
        processNo = processMap[processNo] || processNo;

        let findGoods: GoodsInBox[] = [];
        await materialsWithCodeTipGet(dataCode).then((res) => {
            findGoods = res;
        });

        if (findGoods == undefined || findGoods.length === 0) {
            message.error("未查询到的该物料");
            incompleteAsnList.value = [];
            selectedAsn.value = null;
            return;
        }

        selectedAsn.value = null;
        try {
            incompleteAsnList.value = await getIncompleteAsnByMaterialCode(findGoods[0].materialCode);
        } catch (error) {
            incompleteAsnList.value = [];
            console.error('获取未完成ASN失败', error);
        }

        let goodsItemData: DataItem = new DataItem();
        goodsItemData.goodsId = findGoods[0].goodsId;
        goodsItemData.materialCode = findGoods[0].materialCode;
        goodsItemData.materialName = findGoods[0].materialName;
        goodsItemData.goodsSpec = findGoods[0].goodsSpec;
        goodsItemData.quantity = Number(parts[4]) || 0;
        goodsItemData.countInOnePkgOrBox = Number(parts[4]) || 0;
        goodsItemData.goodsUnits = "PCS";
        goodsItemData.ProcessNo = processNo;
        goodsItemData.grade = grade;
        goodsItemData.goodsBatchNo = batchCode;
        goodsItemData.supplierProductionDate = supplierProductionDate;
        goodsItemData.dataCode = dataCode;
        goodsItemData.receivingMaterialBarcode = QRcode.value;
        goodsItemData.boxNumber = boxNumber;
        goodsItemData.incellshu = Number(parts[4]) || 0;

        goods.value.push(goodsItemData);
        message.success(`扫码成功，箱号：${boxNumber}`);

        QRcode.value = '';
        setTimeout(() => {
            if (focus2.value) {
                focus2.value.focus();
            }
        }, 100);
    }
}

const scanboxCode = async () => {
    if (boxCode.value.length > 10) {
        var le = boxCode.value.length;
        boxCode.value = boxCode.value.slice(le - 10, le);
    }

    var params = new PagedStockQueryDto();
    params.cellCode = boxCode.value;
    await stocksQuery(params).then((res) => {
        dataSource.value.length = 0;
        res.forEach((e: any) => {
            dataSource.value.push(e);
        });
    }).catch((err: any) => {
        console.error('库存查询失败', err);
    });
}

function formatDate(dateStr: string) {
    if (!dateStr || dateStr.trim() === '' || dateStr.toLowerCase() === 'invalid date') {
        return '无';
    }
    try {
        return moment(dateStr).format('YYYY-MM-DD');
    } catch {
        return '无';
    }
}

const deletegood = (index: number) => {
    goods.value.splice(index, 1);
}

const deleteStock = async (record: any, index: number) => {
    createConfirm({
        title: '确认删除',
        content: `您确定要删除物料 ${record.materialName} 的库存吗？`,
        okText: '确定',
        cancelText: '取消',
        onOk: async () => {
            try {
                await stockRemoveDirect(record.id);
                message.success('删除成功');
                dataSource.value.splice(index, 1);
            } catch (error) {
                message.error('删除失败');
            }
        }
    });
};

const incell = async () => {
    let stockCreateDto: StockCreateDto[] = [];

    if (goods.value.length == 0) {
        message.error("没有物料信息");
        return;
    }

    if (boxCode.value == '') {
        message.error("没有容器信息");
        return;
    }

    if (!selectedAsn.value?.cordercode) {
        message.error("请先选择ASN订单");
        return;
    }

    goods.value.forEach((e) => {
        let p = new StockCreateDto();
        p.totalCount = e.incellshu;
        p.barcode = e.materialCode;
        p.materialCode = e.materialCode;
        p.boxNumber = e.boxNumber || '0';
        p.batchCode = e.goodsBatchNo || '';
        p.receivingMaterialBarcode = e.receivingMaterialBarcode || '';
        p.grade = e.grade || '';
        p.processNo = e.ProcessNo || '';
        p.countInOnePkgOrBox = e.countInOnePkgOrBox || undefined;

        if (e.supplierProductionDate && e.supplierProductionDate.trim() !== '' && e.supplierProductionDate.toLowerCase() !== 'invalid date') {
            p.supplierProductionDate = new Date(e.supplierProductionDate);
        } else {
            p.supplierProductionDate = new Date();
        }

        stockCreateDto.push(p);
    });

    try {
        await stockCreateAndBindBoxWithAsn(boxCode.value, selectedAsn.value.cordercode, stockCreateDto).then((res: any) => {
            if (res.success == true) {
                message.success(res.message);
                goods.value.length = 0;
                dataSource.value.length = 0;
                incompleteAsnList.value = [];
                selectedAsn.value = null;
                QRcode.value = '';
                boxCode.value = '';
                setTimeout(() => {
                    if (focus1.value) {
                        focus1.value.focus();
                    }
                }, 100);
            } else if (res.success == false) {
                message.error(res.message);
            } else {
                message.error("接口推送异常，入库失败");
            }
        }).catch((error: any) => {
            message.error(error.message || '操作失败');
        });
    } catch (err) {
        message.error(err);
    }
}

const openCancelModal = () => {
    if (boxCode.value == '') {
        message.error("没有容器信息");
        return;
    }

    if (dataSource.value.length === 0) {
        message.error("没有已组盘信息");
        return;
    }

    createConfirm({
        title: '确认取消组盘',
        content: '您确定要取消当前容器的组盘吗？此操作将清空所有已组盘的物料信息。',
        okText: '确定',
        cancelText: '取消',
        onOk: () => {
            diskcancel();
        }
    });
}

const diskcancel = async () => {
    if (boxCode.value == '') {
        message.error("没有容器信息");
        return;
    }

    try {
        await stocksDisBindBox(boxCode.value).then((res: any) => {
            if (res.success == true) {
                message.success(res.message);
                goods.value.length = 0;
                dataSource.value.length = 0;
                incompleteAsnList.value = [];
                selectedAsn.value = null;
                QRcode.value = '';
                boxCode.value = '';
                setTimeout(() => {
                    if (focus1.value) {
                        focus1.value.focus();
                    }
                }, 100);
            } else if (res.success == false) {
                message.error(res.message);
            } else {
                message.error("接口推送异常，解绑失败");
            }
        }).catch((error: any) => {
            message.error(error.message || '操作失败');
        });
    } catch (err) {
        message.error(err);
    }
}

const openGoodinfo = (record: any) => {
    console.log('查看物料详情', record);
};

const [registerGoodsDetailModal, { openModal: openGoodsDetailModal }] = useModal();
</script>

<style scoped lang="less">
.box-disk-container {
    min-height: 100vh;
    background: #ffffff;
    padding: 0;
    position: relative;
}

.input-row {
    margin: 10px 0;
    padding: 0 16px;
}

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

.asn-info {
    margin: 10px 16px;
    padding: 10px 12px;
    background-color: #f5f9ff;
    border: 1px solid #d6e4ff;
    border-radius: 6px;
    overflow-x: auto;

    &__title {
        font-size: 13px;
        font-weight: 600;
        color: #1890ff;
        margin-bottom: 8px;
    }

    :deep(.ant-table-thead > tr > th) {
        padding: 6px 4px;
        font-size: 12px;
        white-space: nowrap;
    }

    :deep(.ant-table-tbody > tr > td) {
        padding: 6px 4px;
        font-size: 12px;
    }
}

.asn-group {
    margin-bottom: 10px;
    border: 1px solid #e6f4ff;
    border-radius: 6px;
    overflow: hidden;
    background: #fff;

    &:last-child {
        margin-bottom: 0;
    }

    &__header {
        padding: 8px 10px;
        font-size: 13px;
        font-weight: 600;
        color: #1890ff;
        background: linear-gradient(135deg, #f0f7ff 0%, #e6f4ff 100%);
        border-bottom: 1px solid #d6e4ff;
        word-break: break-all;
    }

    :deep(.ant-table) {
        margin: 0;
    }

    :deep(.ant-table-wrapper) {
        border-radius: 0;
    }
}

.asn-action-text {
    color: #1890ff;
    cursor: pointer;
    user-select: none;
    white-space: nowrap;
}

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

.scan-icon {
    color: #1890ff !important;
    font-size: 18px;
    transition: all 0.2s ease;

    &:hover {
        color: #40a9ff !important;
    }
}

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
    padding: 0 2px;
}

::v-deep(.ant-input-number-input) {
    height: 22px;
}

::v-deep(.ant-table-placeholder) {
    padding: 0 0px;
}

p {
    margin-bottom: 0em;
}
</style>
