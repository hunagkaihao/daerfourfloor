<template>
    <div class="box-disk-container">
        <Header numb="组盘"></Header>
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
                        <a-col :span="12">stockCreateAndBindBox
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
                                    <p>入库包数:</p>
                                </a-col>
                                <a-col :span="9">
                                    <a-input-number v-model:value="i.baoshu" :min="0" @change="scanbaoshu(index)" />
                                </a-col>
                            </a-row>
                        </a-col>
                        <a-col :span="12">
                            <a-row align="middle">
                                <a-col :span="9">
                                    <p>散件数量:</p>
                                </a-col>
                                <a-col :span="9">
                                    <a-input-number v-model:value="i.sanjianshu" :min="0" @change="scanbaoshu(index)" />
                                </a-col>
                            </a-row>
                        </a-col>
                    </a-row>
                    <a-row class="goods-input-row" align="middle">
                        <a-col :span="12">
                            <a-row align="middle">
                                <a-col :span="9">
                                    <p>入库数量:</p>
                                </a-col>
                                <a-col :span="9">
                                    <a-input-number size="large" v-model:value="i.incellshu" :min="0"
                                        @change="scanincellshu(index)" />
                                </a-col>
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
                        <!--<span @click="OpenGoodsDetail(record)" style="margin-right: 10px">查看</span>-->
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
import { ref, h, onMounted, onUnmounted,Ref} from 'vue';
import { ScanOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
import { useMessage } from '/@/hooks/web/useMessage';
import { materialsWithCodeTipGet,DataItem,stockCreateAndBindBox } from '/@/views/mobile/views/Material';
import { stocksGetInBox, columns, stocksDisBindBox,stockRemoveDirect, stocksQuery } from '../views/Stock';
import { getLaneCellStatusByCellCode } from '/@/views/warehouse/cells/Cell';
import LaneCellChips from '../components/LaneCellChips.vue';
import { PagedStockQueryDto, CellLaneStatusDto } from '/@/services/ServiceProxies';
// 已删除的IncellByPeople.ts文件，使用内联实现
const barcodeGet = async () => {
  message.info('功能已禁用');
  return null;
};

import { StockCreateDto } from '/@/services/ServiceProxies';
import Header from '../header/Header.vue'
import { useModal } from '/@/components/Modal';
// 使用从Stock.ts导入的列定义
const diskcolumns = columns;
import { DeleteOutlined } from '@ant-design/icons-vue';
// 已删除的GoodsDetail.vue文件，使用简单的替代组件
const GoodsDetail = { template: '<div>功能已禁用</div>' };
const [registerGoodsDetailModal, { openModal: openGoodsDetailModal }] = useModal();
const focus1 = ref<any>();
const focus2 = ref<any>();
let QRcode = ref<string>('');
let boxCode = ref<string>('');
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
    
    // 监听组盘取消成功事件
    window.addEventListener('boxDiskCancelSuccess', handleBoxDiskCancelSuccess);
    
    // 页面加载完成后，自动聚焦到容器码输入框
    setTimeout(() => {
        if (focus1.value) {
            focus1.value.focus();
        }
    }, 100);
})

onUnmounted(() => {
    window.removeEventListener('boxDiskCancelSuccess', handleBoxDiskCancelSuccess);
})

// 处理组盘取消成功事件
const handleBoxDiskCancelSuccess = () => {
    // 刷新数据
    goods.value.length = 0;
    QRcode.value = '';
    scanboxCode();
    boxCode.value = '';
}
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
        const barcode = QRcode.value.trim();
        const { materialCode, batchCode, processNo, grade, fullBoxQty, boxNumber } = parseReceivingBarcode(barcode);
        
        // 检查是否重复扫码
        const isDuplicate = goods.value.some(item => item.dataCode === barcode);
        if (isDuplicate) {
            message.error("该收料码已扫描，不能重复扫描");
            return;
        }
        
        let findGoods:GoodsInBox[] = [];
        await materialsWithCodeTipGet(materialCode).then((res)=>{
            findGoods = res
        });
        if (findGoods == undefined || findGoods.length === 0) {
            message.error("未查询到的该物料")
            return;
        }
        
        let goodsItemData: DataItem = new DataItem();
        goodsItemData.goodsId = findGoods[0].goodsId;
        goodsItemData.materialCode = findGoods[0].materialCode;
        goodsItemData.materialName = findGoods[0].materialName;
        goodsItemData.goodsSpec = findGoods[0].goodsSpec;
        goodsItemData.quantity = fullBoxQty as unknown as number;
        goodsItemData.countInOnePkgOrBox = fullBoxQty as unknown as number;
        goodsItemData.goodsUnits = "PCS";
        goodsItemData.processNo = processNo;
        goodsItemData.grade = grade;
        goodsItemData.goodsBatchNo = batchCode;
        goodsItemData.dataCode = barcode;
        goodsItemData.boxNumber = boxNumber;
        // 设置默认值：入库包数1，散件数量0
        goodsItemData.baoshu = 1;
        goodsItemData.sanjianshu = 0;
        goodsItemData.incellshu = goodsItemData.baoshu * goodsItemData.countInOnePkgOrBox + goodsItemData.sanjianshu;
        goods.value.push(goodsItemData);
        message.success(boxNumber ? `扫码成功，箱号：${boxNumber}` : '扫码成功');
        // 清空输入框，准备下一次扫码
        QRcode.value = '';
        // 自动聚焦到收料码输入框
        setTimeout(() => {
            if (focus2.value) {
                focus2.value.focus();
            }
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
        dataSource.value.length = 0;
        res.forEach((e) => {
            dataSource.value.push(e);
        });
    }).catch((err) => {
        // 库存查询失败不影响容器查询结果
    });

    setTimeout(() => {
        if (focus2.value) {
            focus2.value.focus();
        }
    }, 100);
}
function scanbaoshu(index) {
    if (goods.value[index].countInOnePkgOrBox != 0 && goods.value[index].countInOnePkgOrBox != null) {
        goods.value[index].incellshu = goods.value[index].baoshu * goods.value[index].countInOnePkgOrBox + goods.value[index].sanjianshu
    }

}
function scanincellshu(index) {
    if (goods.value[index].countInOnePkgOrBox != 0 && goods.value[index].countInOnePkgOrBox != null) {
        goods.value[index].sanjianshu = goods.value[index].incellshu % goods.value[index].countInOnePkgOrBox;
        goods.value[index].baoshu = (goods.value[index].incellshu - goods.value[index].sanjianshu) / goods.value[index].countInOnePkgOrBox;
    }
}

const OpenGoodsDetail = (record: Recordable) => {
    openGoodsDetailModal(true, {
        record: record,
    });
};


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

const incell = async () => {
    let stockCreateDto = new Array<StockCreateDto>()
    if (goods.value.length == 0) {
        message.error("没有物料信息")
        return
    }
    if (boxCode.value == '') {
        message.error("没有容器信息")
        return
    }
    goods.value.forEach((e) => {
        let p = new StockCreateDto()
        p.totalCount = e.incellshu
        p.barcode = e.materialCode
        p.materialCode = e.materialCode
        p.boxNumber = e.boxNumber || undefined
        p.receivePkgOrBoxCount = e.baoshu || undefined
        p.countInOnePkgOrBox = e.countInOnePkgOrBox || undefined
        p.batchCode = e.goodsBatchNo || ''
        p.grade = e.grade || ''
        p.processNo = e.processNo || ''
        // 处理生产日期，如果为空则使用当前日期
        if (e.supplierProductionDate && e.supplierProductionDate.trim() !== '') {
            p.supplierProductionDate = new Date(e.supplierProductionDate)
        } else {
            p.supplierProductionDate = new Date()
        }
        stockCreateDto.push(p)
    })
    console.log(stockCreateDto)
    try{
        await stockCreateAndBindBox(boxCode.value,stockCreateDto).then((res) => {
            if (res.success == true) {
                message.success(res.message)
                goods.value.length = 0
                QRcode.value = ''
                boxCode.value = ''
                laneCellStatusList.value = []
                setTimeout(() => {
                if (focus1.value) {
                    focus1.value.focus();
                }
            }, 100);
            } else if(res.success == false){
                message.error(res.message)
            }else{
                message.error("接口推送异常，入库失败")
            }
        }).catch((error) => {
            message.error(error.message || '操作失败')
        })
    }catch(err){
        message.error(err)
    }
}
// 打开组盘取消弹窗
const openCancelModal = () => {
    if (boxCode.value == '') {
        message.error("没有容器信息")
        return
    }
    
    if (dataSource.value.length === 0) {
        message.error("没有已组盘信息")
        return
    }

    // 打开确认弹框
    createConfirm({
        title: '确认取消组盘',
        content: '您确定要取消当前容器的组盘吗？此操作将清空所有已组盘的物料信息。',
        okText: '确定',
        cancelText: '取消',
        onOk: () => {
            // 执行组盘取消操作
            diskcancel();
        }
    });
}

const diskcancel = async () => {
    if (boxCode.value == '') {
        message.error("没有容器信息")
        return
    }

    try{
        await stocksDisBindBox(boxCode.value).then((res) => {
            if (res.success == true) {
                message.success(res.message)
                goods.value.length = 0
                QRcode.value = ''
                boxCode.value = ''
                laneCellStatusList.value = []
                setTimeout(() => {
                    if (focus1.value) {
                        focus1.value.focus();
                    }
                }, 100);
            } else if(res.success == false){
                message.error(res.message)
            }else{
                message.error("接口推送异常，解绑失败")
            }
        }).catch((error) => {
            message.error(error.error.message)
        })
    }catch(err){
        message.error(err)
    }
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
