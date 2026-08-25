<template>
    <div class="components-input-demo-presuffix">
        <Header numb="人工入库"></Header>
        <a-row style="margin-top: 10px;">
            <a-col :span="6">
                <div class="htext">
                    <h1 autofocus="autofocus">收料条形码:</h1>
                </div>
            </a-col>
            <a-col :span="17">
                <a-input v-model:value="goodsCode" placeholder="扫描收料条形码" @keyup.enter="scangoodsCode" :allowClear="true"
                    @focus="focusFn">
                    <!-- <template #prefix>
                        <scan-outlined />
                    </template> -->
                    <template #suffix>
                        <scan-outlined />
                    </template>
                </a-input>
            </a-col>
        </a-row>
        <a-row style="margin-top: 10px;">
            <a-col :span="6">
                <div class="htext">
                    <h1 autofocus="autofocus">入库容器:</h1>
                </div>
            </a-col>
            <a-col :span="17">
                <a-input v-model:value="boxCode" placeholder="扫描料箱" @keyup.enter="scanboxCode" ref="Ref"
                    :allowClear="true" @focus="focusFn">
                    <!-- <template #prefix>
                     <scan-outlined />
                    </template> -->
                    <template #suffix>
                        <scan-outlined />
                    </template>

                </a-input>
            </a-col>
        </a-row>
        <a-row style="margin-top: 10px;">
            <a-col :span="6">
                <div class="htext">
                    <h1 autofocus="autofocus">入库库位:</h1>
                </div>
            </a-col>
            <a-col :span="17">
                <a-input v-model:value="cellCode" placeholder="扫描入库库位" @keyup.enter="scancellCode" ref="Ref"
                    :allowClear="true" @focus="focusFn">
                    <!-- <template #prefix>
                     <scan-outlined />
                    </template> -->
                    <template #suffix>
                        <scan-outlined />
                    </template>

                </a-input>
            </a-col>
        </a-row>
        <!-- <a-row style="margin-top: 10px;">
            <a-col :span="6">
                <div class="htext">
                    <h1 autofocus="autofocus">包数:</h1>
                </div>
            </a-col>
            <a-col :span="17">
                <a-input v-model:value="cellCode" placeholder="扫描入库库位" @keyup.enter="scancellCode" ref="Ref"
                    :allowClear="true" @focus="focusFn">


                </a-input>
            </a-col>
        </a-row>
        <a-row style="margin-top: 10px;">
            <a-col :span="6">
                <div class="htext">
                    <h1 autofocus="autofocus">散件数量:</h1>
                </div>
            </a-col>
            <a-col :span="17">
                <a-input v-model:value="cellCode" placeholder="扫描入库库位" @keyup.enter="scancellCode" ref="Ref"
                    :allowClear="true" @focus="focusFn">


                </a-input>
            </a-col>
        </a-row> -->
        <p style="margin-left: 20px">收料码数量:{{ goods.length }}</p>
        <div style=" overflow:auto;" :style="{
                height: goodheight + 'vh'
            }">
            <a-card style="margin: 10px 5px 20px 5px" v-for="(i, index) in goods">
                <template #title>
                    物料:{{ i.materialName }}{{ i.specs }}
                </template>
                <template #extra>
                    <a-button style="margin-right: 10px;" type="primary" @click="openGoodinfo(i)">详情</a-button>
                    <a-button type="primary" @click="deletegood(index)" :icon="h(DeleteOutlined)"></a-button>
                </template>
                <a-row>
                    <a-col :span="12">
                        <p>物料编号:{{ i.materialCode }}</p>
                        <p>收料仓库:{{ i.targetWarehouseName }}</p>
                        <p>合格放行数量:{{ i.passCnt }}</p>
                        <p>检验编号:{{ i.checkNo }}</p>
                        <a-row>
                            <a-col :span="9">
                                <p>入库包数:</p>
                            </a-col>
                            <a-col :span="9"><a-input-number id="inputNumber" v-model:value="i.baoshu" :min="0"
                                    @change="scanbaoshu(index)" /></a-col>
                        </a-row>
                        <a-row>
                            <a-col :span="9">
                                <p>入库数量:</p>
                            </a-col>
                            <a-col :span="9"><a-input-number size="large" v-model:value="i.incellshu" :min="0"
                                    @change="scanincellshu(index)" /></a-col>
                        </a-row>
                    </a-col>
                    <a-col :span="12">
                        <p>计量单位:{{ i.unit }}</p>
                        <p>检验单号:{{ i.checkOrderCode }}</p>
                        <p>最小包装数:{{ i.countInOnePkgOrBox }}</p>
                        <p>供应商编号:{{ i.supplierCode }}</p>
                        <a-row>
                            <a-col :span="9">
                                <p>散件数量:</p>
                            </a-col>
                            <a-col :span="9"><a-input-number v-model:value="i.sanjianshu" :min="0"
                                    @change="scanbaoshu(index)" /></a-col>
                        </a-row>
                    </a-col>
                </a-row>
            </a-card>
        </div>
        <div v-show="showtable">
            <p style="margin-left: 20px">已组盘信息:{{ dataSource.length }}</p>
            <a-table ref="tableRef" :dataSource="dataSource" :columns="columns" :pagination="false"
                :scroll="{ x: screenWidth, y: 128 }">
                <template #bodyCell="{ column, record }">
                    <template v-if="column.key === 'operation'">
                        <span @click="OpenGoodsDetail(record)">查看</span>
                    </template>
                </template>
            </a-table>


            <div class="tab-bar">
                <a-button @click="incell" type="primary" class="btn_4">
                    入库确认
                </a-button>

            </div>
        </div>
    </div>
    <GoodsDetail @register="registerGoodsDetailModal"></GoodsDetail>
</template>
<script lang="ts" setup>
import { ref, h, onMounted,computed } from 'vue';
import { ScanOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
// 已删除的IncellByPeople.ts文件，使用内联实现
const barcodeGet = async () => {
  message.info('功能已禁用');
  return null;
};
const stocksCreateAndBindToCell = async () => {
  message.info('功能已禁用');
  return null;
};
const stocksGetInCell = async () => {
  message.info('功能已禁用');
  return [];
};
import { StockCreateDto } from '/@/services/ServiceProxies';
import { useMessage } from '/@/hooks/web/useMessage';
import Header from '../header/Header.vue'
import { useModal } from '/@/components/Modal';
// 已删除的IncellByPeople.ts文件，使用内联实现
const columns = [];
import { DeleteOutlined } from '@ant-design/icons-vue';
// 已删除的GoodsDetail.vue文件，使用简单的替代组件
const GoodsDetail = { template: '<div>功能已禁用</div>' };
import { useUserStore } from '/@/store/modules/user';
const userStore = useUserStore();
 const getUserInfo = computed(() => {
  const { realName = '', avatar, desc } = userStore.getUserInfo || {};
  return { realName, avatar: avatar || desc };
 });
const { createConfirm } = useMessage();
const [registerGoodsDetailModal, { openModal: openGoodsDetailModal }] = useModal();
let goodsCode = ref<string>('');
let cellCode = ref<string>('');
let boxCode = ref<string>('')
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
})
var lock = true
//扫码收料码
async function scangoodsCode() {
    if(goodsCode.value.length > 10){
        var le = goodsCode.value.length
        goodsCode.value = goodsCode.value.slice(le-10,le)
    }
    
    if(lock == true){
        lock = false
        try {
            goods.value.forEach((e) => {
                if (e.barcode == goodsCode.value) {
                    throw new Error("已找到，退出循环");
                }
            })
        } catch {
            message.error("收料码已扫描")
            lock = true
            return
        }
        try {
            await barcodeGet(goodsCode.value).then((res) => {
                res.baoshu = 0
                res.incellshu = res.passCnt
                res.sanjianshu = 0
                goods.value.push(res)
                message.success("收料码添加成功")
                lock = true
            }).catch((err) => {
                message.error(err.error.message)
                lock = true
            })
        } catch {
            message.error("网络错误,请求失败")
            lock = true
        }
    }else{
        message.error("处理中，请等待")
    }

}

const scancellCode = async () => {
    if(cellCode.value.length > 10){
        var le = cellCode.value.length
        cellCode.value = cellCode.value.slice(le-10,le)
    }
    await stocksGetInCell(cellCode.value).then((res) => {
        dataSource.value.length = 0
        res.forEach((e) => {
            dataSource.value.push(e)
        })
    }).catch((err) => {
        message.error(err.error.message)
    })
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

const openGoodinfo = (record: Recordable) => {
    openGoodsDetailModal(true, {
        record: record,
    });
};

const deletegood = (index) => {
    goods.value.splice(index, 1)
}
const OpenGoodsDetail = (record: Recordable) => {
    openGoodsDetailModal(true, {
        record: record,
    });
};
const scanboxCode = ()=>{
    
}
const incell = async () => {
    let stockCreateDto = new Array<StockCreateDto>()
    if (goods.value.length == 0) {
        message.error("没有物料信息")
        return
    }
    if (cellCode.value == '') {
        message.error("没有库位信息")
        return
    }
    goods.value.forEach((e) => {
        let p = new StockCreateDto()
        p.barcode = e.barcode
        p.totalCount = e.incellshu
        p.receivePkgOrBoxCount = e.baoshu || undefined
        p.countInOnePkgOrBox = e.countInOnePkgOrBox || undefined
        stockCreateDto.push(p)
    })
    console.log(stockCreateDto)
    try{
        await stocksCreateAndBindToCell(cellCode.value,getUserInfo.value.realName,stockCreateDto).then((res) => {
            if (res.success == true) {
                message.success(res.message)
                goods.value.length = 0
                goodsCode.value = ''
                scancellCode()
                cellCode.value = ''
            } else if(res.success == false){
                message.error(res.message)
            }else{
                message.error("接口推送异常，入库失败")
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
.btn_4 {
    margin: auto;
    margin-top: 10px;
    margin-bottom: 10px;
}

.htext {
    text-align: center;
    line-height: 30px;
}

.tab-bar {
    display: flex;
    position: fixed;
    left: 0;
    right: 0;
    bottom: 0;
    height: 49px;
    background-color: #fff;
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
