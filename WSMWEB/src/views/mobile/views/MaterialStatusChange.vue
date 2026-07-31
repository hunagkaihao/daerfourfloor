<template>
    <div class="box-disk-container">
        <Header numb="物料状态变更"></Header>
        <a-row class="input-row">
            <a-col :span="6">
                <div class="htext">
                    <h1 >绑定库位:</h1>
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
        <a-row class="input-row">
            <a-col :span="6">
                <div class="htext">
                    <h1>到货单号:</h1>
                </div>
            </a-col>
            <a-col :span="17">
                <a-input v-model:value="asnCode" placeholder="扫描到货单号" @keyup.enter="handleAsnCodeEnter"
                    :allowClear="true" @focus="focusFn" class="modern-input" ref="asnInputRef">
                    <template #suffix>
                        <scan-outlined class="scan-icon" />
                    </template>
                </a-input>
            </a-col>
        </a-row>
      
        <p style="margin-left: 20px">收料码数量:{{ goods.length }}</p>
        <div style=" overflow:auto;" :style="{
                height: goodheight + 'vh'
            }">
            <a-card style="margin: 10px 5px 20px 5px" v-for="(i, index) in goods">
                <template #title>
                    {{ i.materialName }}
                </template>
                <template #extra>
                    <a-button type="primary" @click="deletegood(index)" :icon="h(DeleteOutlined)"></a-button>
                </template>
                <div class="goods-info">
                    <a-row>
                        <a-col :span="12">
                            <p>物料编号:{{ i.materialCode }}</p>
                        </a-col>
                        <a-col :span="12">
                            <p>批次号:{{ i.goodsBatchNo || '-' }}</p>
                        </a-col>
                    </a-row>
                    <a-row>
                        <a-col :span="12">
                            <p>箱号:{{ i.processNo || '-' }}</p>
                        </a-col>
                        <a-col :span="12">
                            <p>等级:{{ i.grade || '-' }}</p>
                        </a-col>
                    </a-row>
                    <a-row class="goods-info-bold">
                        <a-col :span="12">
                            <p>整箱数量:{{ i.quantity || '-' }}</p>
                        </a-col>
                        <a-col :span="12">
                            <p>箱数号:{{ i.boxNumber ?? '-' }}</p>
                        </a-col>
                    </a-row>
                    <a-row class="goods-input-row" align="middle">
                        <a-col :span="12">
                            <a-row align="middle">
                                <a-col :span="9">
                                    <p>抽检数量:</p>
                                </a-col>
                                <a-col :span="9">
                                    <p class="inspection-count">{{ i.inspectionCount || '-' }}</p>
                                </a-col>
                            </a-row>
                        </a-col>
                        <a-col :span="12">
                            <a-row align="middle">
                                <a-col :span="9">
                                    <p>检验合格数:</p>
                                </a-col>
                                <a-col :span="9">
                                    <a-input-number size="large" v-model:value="i.incellshu" :min="0" :max="i.quantity" style="width:100%" />
                                </a-col>
                            </a-row>
                        </a-col>
                    </a-row>
                    <a-row class="goods-input-row" align="middle">
                         <a-col :span="12">
                            <a-row align="middle">  
                                <a-col :span="9">   
                                    <p>物料抽检状态:</p>
                                </a-col>
                                <a-col :span="9">  
                                    <a-select
                                        size="small"
                                        v-model:value="i.inspectionstatus"
                                        @change="updateInspectionStatus(index)"
                                        style="width:60%;">
                                        <a-select-option :value="2">合格</a-select-option>
                                        <a-select-option :value="3">不合格</a-select-option>
                                    </a-select>
                                </a-col>
                            </a-row>
                        </a-col>
                    </a-row>
                </div>
            </a-card>
        </div>
        <div v-show="showtable">
            <p style="margin-left: 20px">已组盘信息:{{ dataSource.length }}</p>
            <a-table ref="tableRef" :dataSource="dataSource" :columns="diskcolumns" :pagination="false" rowKey="id"
                :scroll="{ x: screenWidth, y: 128 }">
                <template #bodyCell="{ column, record, index }">
                    <template v-if="column.key === 'operation'">
                        <!--<span @click="OpenGoodsDetail(record)" style="margin-right: 10px">查看</span>-->
                        <span @click="deleteStock(record, index)" style="color: #ff4d4f">删除</span>
                    </template>
                </template>
            </a-table>

            <div class="tab-bar">
                <a-button @click="confirmNotQualified" type="primary" danger class="modern-btn">
                    确认不合格
                </a-button>
                <a-button @click="confirmQualified" type="primary" class="modern-btn">
                    确认合格
                </a-button>
            </div>
        </div>
    </div>
</template>
<script lang="ts" setup>
import { ref, h, onMounted, Ref} from 'vue';
import { ScanOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
import { useMessage } from '/@/hooks/web/useMessage';
import { columns, stockRemoveDirect, stocksQuery, findStockByCellAndMaterial, confirmInspectionQualified, setInspectionNotQualified, pushCGRKDAdd } from './Stock';
import { getLaneCellStatusByCellCode } from '/@/views/warehouse/cells/Cell';
import LaneCellChips from '../components/LaneCellChips.vue';
import { PagedStockQueryDto, CellLaneStatusDto, ErpAsnDto } from '/@/services/ServiceProxies';
import Header from '../header/Header.vue'
import { validateAsn } from './Material'
// 使用从Stock.ts导入的列定义
const diskcolumns = columns;
import { DeleteOutlined } from '@ant-design/icons-vue';
const focus1 = ref<any>();
const focus2 = ref<any>();
const asnInputRef = ref<any>();
let QRcode = ref<string>('');
let boxCode = ref<string>('');
let asnCode = ref<string>('');
const asnDataList = ref<ErpAsnDto[]>([]);
const laneCellStatusList = ref<CellLaneStatusDto[]>([]);
 const { createConfirm } = useMessage();

const fetchLaneCellStatus = async () => {
    if (!boxCode.value.trim()) {
        laneCellStatusList.value = [];
        return;
    }
    try {
        const result = await getLaneCellStatusByCellCode(boxCode.value);
        laneCellStatusList.value = result || [];
    } catch (error: any) {
        laneCellStatusList.value = [];
        message.error(error?.error?.message || error?.message || '查询同巷道库位失败');
    }
};

var goods = ref([]
);
var dataSource = ref([]
);
let screenHeight = ref(window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight);
let screenWidth = ref((window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth) - 8)
let YHOne = ref();
let showtable = ref(true)
let goodheight = ref(36)
const tableRef = ref<any>();
onMounted(() => {
    console.log(tableRef.value.$el.querySelector('.ant-table-thead').clientHeight);
    YHOne.value = screenHeight.value - 42 - tableRef.value.$el.querySelector('.ant-table-thead').clientHeight;
    window.onresize = () => {
        var showHeight = window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight
        console.log(showHeight - screenHeight.value)
        if (showHeight - screenHeight.value >= 0) {

            showtable.value = true
            goodheight.value = 36
        } else {
            showtable.value = false
            goodheight.value = 60
        }
    }
    
    // 页面加载完成后，自动聚焦到容器码输入框
    setTimeout(() => {
        if (focus1.value) {
            focus1.value.focus();
        }
    }, 100);
})



var lock = true

function parseReceivingBarcode(barcode: string) {
    const parts = barcode.split(',');
    const materialCode = parts[0]?.trim() || '';
    const batchCode = parts[1]?.trim() || '';
    let processNo = parts[2]?.trim() || '';
    const grade = parts[3]?.trim() || '';
    const fullBoxQty = parts[4]?.trim() || '';
    // 老码(8段): 第8段为箱号；新码(6段): 无箱号
    const boxNumberRaw = parts.length >= 8 ? parts[7]?.trim() : '';
    const boxNumber = boxNumberRaw || undefined;

    const processMap: Record<string, string> = {
        "11": "来料检验",
        "20": "热处理工序",
        "23": "双端面工序",
        "34": "你的工序名称",
        "47": "工序47"
    };
    processNo = processMap[processNo] || processNo;

    return { materialCode, batchCode, processNo, grade, fullBoxQty, boxNumber };
}

//扫码收料码
async function scangoodsCode() {
    if (QRcode.value.includes(',')) {
        if (!boxCode.value) {
            message.error("请先扫描库位");
            QRcode.value = '';
            return;
        }
        const barcode = QRcode.value.trim();
        const { materialCode, batchCode, processNo, grade, fullBoxQty, boxNumber } = parseReceivingBarcode(barcode);

        const isDuplicate = goods.value.some(item => item.dataCode === barcode);
        if (isDuplicate) {
            message.error("该收料码已扫描，不能重复扫描");
            return;
        }

        const stock = await findStockByCellAndMaterial(boxCode.value, materialCode);
        if (!stock) {
            message.error("未找到该库位下的抽检中库存记录");
            QRcode.value = '';
            return;
        }

        const goodsItem: any = {
            stockId: stock.id,
            materialCode: stock.materialCode || materialCode,
            materialName: stock.materialName || '',
            goodsBatchNo: stock.batchCode || batchCode,
            quantity: stock.totalCountInTime || 0,
            inspectionCount: stock.inspectionCount || 0,
            inspectionstatus: undefined,
            dataCode: barcode,
            incellshu: null,
        };
        goods.value.push(goodsItem);
        message.success('扫码成功');
        QRcode.value = '';
        setTimeout(() => {
            if (focus2.value) focus2.value.focus();
        }, 100);
    }
}

const scanboxCode = async () => {
    if(boxCode.value.length > 10){
        var le = boxCode.value.length
        boxCode.value = boxCode.value.slice(le-10,le)
    }

    await fetchLaneCellStatus();

    // 再查询该库位的库存信息，使用同一个表格显示
    var params = new PagedStockQueryDto();
    params.cellCode = boxCode.value;
    await stocksQuery(params).then((res) => {
        // 清空现有数据，添加库存信息
        dataSource.value = [...res];
    }).catch((err) => {
        // 库存查询失败不影响容器查询结果
    });

    setTimeout(() => {
        if (focus2.value) {
            focus2.value.focus();
        }
    }, 100);
}


const updateInspectionStatus = (index: number) => {
    const item = goods.value[index];
    if (item.inspectionstatus === 2) {
        message.info(`物料 ${item.materialName} 将标记为合格`);
    } else if (item.inspectionstatus === 3) {
        message.info(`物料 ${item.materialName} 将标记为不合格`);
    }
}

const deletegood = (index) => {
    goods.value.splice(index, 1)
}

// 删除单个物料库存
const deleteStock = async (record: Recordable, index: number) => {
    createConfirm({
        title: '确认删除',
        content: `您确定要删除物料 ${record.materialName} 的库存吗？`,
        okText: '确定',
        cancelText: '取消',
        onOk: async () => {
            try {
                // 这里需要调用删除单个库存的API
                await stockRemoveDirect(record.id);
                message.success('删除成功');
                // 重新查询当前库位的库存和同巷道库位状态
                if (boxCode.value) {
                    await scanboxCode();
                }
            } catch (error) {
                message.error('删除失败');
            }
        }
    });
};

const handleAsnCodeEnter = async () => {
    if (!asnCode.value.trim()) return
    try {
        const res = await validateAsn(asnCode.value.trim())
        if (res.success && res.data && res.data.length > 0) {
            asnDataList.value = res.data
            message.success(`到货单加载成功，${res.data.length}条物料`)
        } else {
            message.warning(res.message || '到货单数据为空')
        }
    } catch (err: any) {
        message.error(err?.message || '到货单加载失败')
    }
    setTimeout(() => {
        if (focus2.value) focus2.value.focus()
    }, 100)
}

const confirmQualified = async () => {
    if (goods.value.length === 0) {
        message.error("没有物料信息");
        return;
    }
    if (!boxCode.value) {
        message.error("没有库位信息");
        return;
    }
    if (!asnCode.value.trim()) {
        message.error("请先扫描到货单号");
        return;
    }

    let hasQualified = false;
    let hasNotQualified = false;

    for (const item of goods.value) {
        if (item.inspectionstatus === 2) {
            hasQualified = true;
            if (!item.incellshu || item.incellshu <= 0) {
                message.error(`物料 ${item.materialName} 检验合格数必须大于0`);
                return;
            }
            if (item.incellshu > item.quantity) {
                message.error(`物料 ${item.materialName} 检验合格数不能超过整箱数量(${item.quantity})`);
                return;
            }
        } else if (item.inspectionstatus === 3) {
            hasNotQualified = true;
        }
    }

    if (!hasQualified && !hasNotQualified) {
        message.error("请先选择合格或不合格状态");
        return;
    }

    const qualifiedItems: any[] = []
    try {
        for (const item of goods.value) {
            if (item.inspectionstatus === 2) {
                const res = await confirmInspectionQualified(item.stockId, item.incellshu);
                if (!res.success) {
                    message.error(`物料 ${item.materialName} 确认合格失败: ${res.message}`);
                    continue;
                }
                message.success(`物料 ${item.materialName} 确认合格成功`);
                qualifiedItems.push(item)
            } else if (item.inspectionstatus === 3) {
                const res = await setInspectionNotQualified(item.stockId);
                if (res.success) {
                    message.success(`物料 ${item.materialName} 已标记为不合格`);
                } else {
                    message.error(`物料 ${item.materialName} 设置不合格失败: ${res.message}`);
                }
            }
        }

        if (qualifiedItems.length > 0) {
            const asnData = asnDataList.value
            const orderMap: Record<string, any> = {}

            for (const item of qualifiedItems) {
                const asnItem = asnData.find(
                    a => a.cinvcode === item.materialCode && a.cbatch === item.goodsBatchNo
                )
                const orderCode = asnItem?.cordercode || ''
                if (!orderMap[orderCode]) {
                    orderMap[orderCode] = {
                        AddType: 3,
                        cOrderCode: orderCode,
                        cwarehousecode: asnItem?.cwhcode || '',
                        cmemo: '',
                        CMAKER: asnItem?.cmaker || '',
                        Details: [],
                    }
                }
                orderMap[orderCode].Details.push({
                    sourceautoid: asnItem?.autoid ? Number(asnItem.autoid) : 0,
                    cinvcode: item.materialCode,
                    cbatch: item.goodsBatchNo,
                    fquantity: item.incellshu,
                })
            }

            const entries = Object.values(orderMap)
            const cgParams = {
                Cmd: 'CGRKDAdd',
                taskid: '',
                maker: entries[0]?.CMAKER || '',
                id: 0,
                Data: JSON.stringify(entries),
            }
            const pushRes = await pushCGRKDAdd(cgParams)
            if (pushRes.success) {
                message.success(`采购入库单已推送，共${qualifiedItems.length}条物料`)
            } else {
                message.warning(`采购入库单推送失败: ${pushRes.message}`)
            }
        }

        goods.value.length = 0;
        QRcode.value = '';
        asnCode.value = '';
        asnDataList.value = [];
        if (boxCode.value) {
            await scanboxCode();
        }
        setTimeout(() => {
            if (focus1.value) focus1.value.focus();
        }, 100);
    } catch (error: any) {
        message.error(error?.message || '操作失败');
    }
}
const confirmNotQualified = async () => {
    if (goods.value.length === 0) {
        message.error("没有物料信息");
        return;
    }
    if (!boxCode.value) {
        message.error("没有库位信息");
        return;
    }

    createConfirm({
        title: '确认不合格',
        content: `您确定要将当前 ${goods.value.length} 个物料全部标记为不合格吗？`,
        okText: '确定',
        cancelText: '取消',
        onOk: async () => {
            let hasError = false;
            for (const item of goods.value) {
                if (!item.stockId) {
                    message.error(`物料 ${item.materialName} 缺少库存ID`);
                    hasError = true;
                    continue;
                }
                try {
                    const res = await setInspectionNotQualified(item.stockId);
                    if (res.success) {
                        message.success(`物料 ${item.materialName} 已标记为不合格`);
                    } else {
                        message.error(`物料 ${item.materialName} 设置不合格失败: ${res.message}`);
                        hasError = true;
                    }
                } catch (error: any) {
                    message.error(`物料 ${item.materialName} 操作失败: ${error?.message || '未知错误'}`);
                    hasError = true;
                }
            }

            if (!hasError) {
                goods.value.length = 0;
                QRcode.value = '';
                if (boxCode.value) {
                    await scanboxCode();
                }
            }
            setTimeout(() => {
                if (focus1.value) focus1.value.focus();
            }, 100);
        }
    });
}
//软件盘弹出屏蔽
function focusFn(e) {
    e.target.setAttribute('readonly', 'readonly');
    setTimeout(() => {
        e.target.removeAttribute('readonly');
    }, 200);
}

</script>

<style scoped lang="less">

/* 主容器样式 */
.box-disk-container {
    min-height: 100vh;
    background: #ffffff;
    padding: 0;
    position: relative;
    overflow-x: hidden;
}

/* 输入行样式 */
.input-row {
    margin: 10px 0;
    padding: 0 16px;
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

.goods-info {
    padding: 0 4px;

    ::v-deep(.ant-row) {
        margin-bottom: 0;
    }

    ::v-deep(.ant-row + .ant-row) {
        margin-top: 2px;
    }

    p {
        margin-bottom: 2px;
        line-height: 1.3;
    }
}

.goods-info-bold p {
    font-weight: 600;
    color: #262626;
    margin-bottom: 2px;
}

.goods-input-row {
    margin-top: 2px;

    p {
        margin-bottom: 0;
    }
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
