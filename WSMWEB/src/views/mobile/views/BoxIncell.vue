<template>
    <div class="box-incell-container">
        <Header numb="容器入库"></Header>
        <a-row class="input-row">
            <a-col :span="6">
                <div class="htext">
                    <h1 autofocus="autofocus">入库料箱:</h1>
                </div>
            </a-col>
            <a-col :span="11">
                <a-input v-model:value="boxCode" placeholder="扫描料箱码" @keyup.enter="scanboxCode" :allowClear="true"
                    @focus="focusFn" class="modern-input">
                    <!-- <template #prefix>
                        <scan-outlined />
                    </template> -->
                    <template #suffix>
                        <scan-outlined class="scan-icon" />
                    </template>
                </a-input>
            </a-col>
            <a-col :span="6" style="margin-left: 10px;">
                <a-button type="primary" @click="selectBox" class="modern-btn">
                    选择
                </a-button>
            </a-col>
        </a-row>
        <a-row class="input-row">
            <a-col :span="6">
                <div class="htext">
                    <h1 autofocus="autofocus">目标库位:</h1>
                </div>
            </a-col>
            <a-col :span="17">
                <a-input v-model:value="endcellCode" placeholder="目标库位" :allowClear="true"
                    @focus="focusFn" class="modern-input">

                </a-input>
            </a-col>
            <!-- <a-col :span="6" style="margin-left: 10px;">
                <a-button type="primary" @click="selectCell">
                    选择
                </a-button>
            </a-col> -->
        </a-row>

        <div v-show="showtable">
            <p style="margin-left: 20px">容器物料信息:{{ dataSource.length }}</p>
            <a-table ref="tableRef" :dataSource="dataSource" :columns="columns" :pagination="false"
                :scroll="{ x: screenWidth, y: 128 }">
                <template #bodyCell="{ column, record }">
                    <template v-if="column.key === 'operation'">
                        <span @click="OpenGoodsDetail(record)">查看</span>
                    </template>
                </template>
            </a-table>


            <div class="tab-bar">
                <a-button @click="incell" type="primary" class="modern-btn">
                    入库确认
                </a-button>

            </div>
        </div>
    </div>
    <GoodsDetail @register="registerGoodsDetailModal"></GoodsDetail>
    <BoxSelectModal @register="registerBoxSelectModal"  @event="eventFnboxselect"></BoxSelectModal>
    <CellSelectModal @register="registerCellSelectModal"  @event="eventFn"></CellSelectModal>
</template>
<script lang="ts" setup>
import { ref,  onMounted } from 'vue';
import { ScanOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
import { stocksGetInBox, columns,CreateCtuBasicIn } from '../views/Stock';


const getByStock = async () => {
  message.info('功能已禁用');
  return null;
};
import Header from '../header/Header.vue'
import { useModal } from '/@/components/Modal';

// 已删除的GoodsDetail.vue文件，使用简单的替代组件
const GoodsDetail = { template: '<div>功能已禁用</div>' };
// 已删除的BoxSelectModal.vue文件，使用简单的替代组件
const BoxSelectModal = { template: '<div>功能已禁用</div>' };
// 已删除的CellSelectModal.vue文件，使用简单的替代组件
const CellSelectModal = { template: '<div>功能已禁用</div>' };


const [registerGoodsDetailModal, { openModal: openGoodsDetailModal }] = useModal();
const [registerBoxSelectModal, { openModal: openBoxSelectModal }] = useModal();
const [registerCellSelectModal, { openModal: openCellSelectModal }] = useModal();
let boxCode = ref<string>('');
let endcellCode = ref<string>('');
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

//扫码容器码
const scanboxCode = async () => {
    if(boxCode.value.length > 10){
        var le = boxCode.value.length
        boxCode.value = boxCode.value.slice(le-10,le)
    }
    await stocksGetInBox(boxCode.value).then((res) => {
        dataSource.value.length = 0
        res.forEach((e) => {
            dataSource.value.push(e)
        })
    }).catch((err) => {
        message.error(err.error.message)
    })
    if(endcellCode.value == ''){
        await getByStock('',boxCode.value).then((res)=>{
        endcellCode.value = res.cellCode
        })
    }
}






const OpenGoodsDetail = (record: Recordable) => {
    openGoodsDetailModal(true, {
        record: record,
    });
};
const selectCell = ()=>{
    openCellSelectModal(true,{
        record:boxCode.value
    })
}
const selectBox = ()=>{
    openBoxSelectModal(true,{
        record:boxCode.value
    })
}
const incell = async () => {

    try{
        await CreateCtuBasicIn(boxCode.value,endcellCode.value).then((res) => {
            if (res.success == true) {
                message.success(res.message)
                goods.value.length = 0
                boxCode.value = ''
                endcellCode.value = ''
            } else if(res.success == false){
                message.error(res.message)
            }else{
                message.error("接口推送异常，入库失败")
            }
        }).catch((error) => {
            message.error(error.error.message)
        })
    }catch(err){

    }
}
const eventFnboxselect = async(val) => {
    boxCode.value = val
    //分配库位
    scanboxCode()
}

const eventFn = (val) => {
    endcellCode.value = val
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
.box-incell-container {
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
