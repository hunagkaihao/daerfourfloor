<template>
    <div class="components-input-demo-presuffix">
        <Header numb="手工出库"></Header>
     
        <a-row class="input-row">
            <a-col :span="6">
                <div class="htext">
                    <h1 autofocus="autofocus">收料条形码:</h1>
                </div>
            </a-col>
            <a-col :span="17">
                <a-input v-model:value="goodsCode" placeholder="扫描收料条形码" @keyup.enter="scangoodsCode" 
                    :allowClear="true" @focus="focusFn" class="modern-input">
                    <template #suffix>
                        <scan-outlined class="scan-icon" />
                    </template>
                </a-input>
            </a-col>
        </a-row>
        
        <a-row class="input-row">
            <a-col :span="6">
                <div class="htext">
                    <h1 autofocus="autofocus">出库容器:</h1>
                </div>
            </a-col>
            <a-col :span="17">
                <a-input v-model:value="boxCode" placeholder="扫描容器码" @keyup.enter="scanboxCode" ref="Ref"
                    :allowClear="true" @focus="focusFn" class="modern-input">
                    <!-- <template #prefix>
                     <scan-outlined />
                    </template> -->
                    <template #suffix>
                        <scan-outlined class="scan-icon" />
                    </template>

                </a-input>
            </a-col>
        </a-row>
        <a-row class="input-row">
            <a-col :span="6">
                <div class="htext">
                    <h1 autofocus="autofocus">领用数量:</h1>
                </div>
            </a-col>
            <a-col :span="17">
                <a-input v-model:value="moveCount" placeholder="输入领用数量" 
                    :allowClear="true" class="modern-input">
                </a-input>
            </a-col>
        </a-row>
        <div style="height: 30vh; overflow:auto;">
            <a-card style="margin:5px" v-for="(i, index) in goods">
                <template #title>
                    物料:{{ i.materialName }}{{ i.specs }}
                </template>
                <template #extra>
                    <!-- <a-button style="margin-right: 10px;" type="primary" @click="openGoodinfo(i)">详情</a-button> -->
                    <!-- <a-button type="primary" @click="deletegood(index)" :icon="h(DeleteOutlined)"></a-button> -->
                </template>
                <a-row>
                    <a-col :span="12">
                        <p>物料编号:{{ i.materialCode }}</p>
                        <p>计量单位:{{ i.materialCode }}</p>
                        <p>入仓日期:{{ i.stockInDate }}</p>
                    </a-col>
                    <a-col :span="12">
                        <p>检验编号:{{ i.checkNo }}</p>
                        <p>状态:{{ i.unit }}</p>
                        <p>结存数量:{{ i.totalCountInTime }}</p>

                    </a-col>
                </a-row>
            </a-card>

        </div>

        <p style="margin-left: 20px">领用单信息</p>
        <a-table :row-selection="{ selectedRowKeys: state.selectedRowKeys, onChange: onSelectChange,type: 'radio', }" :row-key="record => record.id" :dataSource="dataSource" :columns="ZZoutcellcolumns" :pagination="false" :scroll="{ x: screenWidth, y: 128 }">
            <template #bodyCell="{ column, record }">
                <template v-if="column.key === 'operation'">
                    <span @click="OpenGoodsDetail(record)">查看</span>
                </template>
            </template>
        </a-table>
        <div class="tab-bar">
            <a-button @click="Incell" type="primary" class="modern-btn">
                确认出库
            </a-button>

        </div>
    </div>
    <GoodsDetail @register="registerGoodsDetailModal"></GoodsDetail>
    <BoxModal @register="registerBoxModal" ></BoxModal>
</template>
<script lang="ts" setup>
import { defineComponent, ref,reactive, computed, onMounted, h } from 'vue';
import { ScanOutlined, TableOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
import { useMessage } from '/@/hooks/web/useMessage';
import Header from '../header/Header.vue'
// 已删除的TiaoboIncell.ts文件，使用内联实现
const ZZoutcellcolumns = [];
const pickOutByZZ = async () => {
  message.info('功能已禁用');
  return null;
};
const getByBarcodeBoxCode = async () => {
  message.info('功能已禁用');
  return null;
};
// 已删除的GoodsDetail.vue文件，使用简单的替代组件
const GoodsDetail = { template: '<div>功能已禁用</div>' };
import { useModal } from '/@/components/Modal';
import { useUserStore } from '/@/store/modules/user';
// 已删除的BoxModal.vue文件，使用简单的替代组件
const BoxModal = { template: '<div>功能已禁用</div>' };
const userStore = useUserStore();
 const getUserInfo = computed(() => {
  const { realName = '', avatar, desc } = userStore.getUserInfo || {};
  return { realName, avatar: avatar || desc };
 });
const { createConfirm } = useMessage();
const [registerGoodsDetailModal, { openModal: openGoodsDetailModal }] = useModal();
const [registerBoxModal, { openModal: openBoxModal }] = useModal();
let goodsCode = ref<string>('');
let boxCode = ref<string>('');
let oldboxCode = ref<string>('');
let moveCount = ref("")
var goods = ref([]);
goods.value = []
var dataSource = ref([]);
let screenHeight = ref(window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight);
let screenWidth = ref((window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth) - 8)
let YHOne = ref();
let pickListCode = ''
let uniqueCode = ''
let showtable = ref(true)
let goodheight = ref(36)
const tableRef = ref<any>();
onMounted(() => {
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
const scangoodsCode = async () => {
    if(goodsCode.value.length > 10){
        var le = goodsCode.value.length
        goodsCode.value = goodsCode.value.slice(le-10,le)
    }

    if (goodsCode.value == '') {
        message.error("请扫码收料码")
        return
    }
    
    
}
const scanoldboxCode =()=>{
    if(oldboxCode.value.length > 10){
        var le = oldboxCode.value.length
        oldboxCode.value = oldboxCode.value.slice(le-10,le)
    }
}
const scanboxCode = async () => {
    await getByBarcodeBoxCode(goodsCode.value,boxCode.value).then((res) => {
        dataSource.value.length = 0
        res.items.forEach((e) => {
            dataSource.value.push(e)
        })
        message.success("扫描容器成功")
        state.selectedRowKeys = [];
        pickListCode = ""
        uniqueCode = ""
    }).catch((err)=>{
        message.error(err.error.message)
    })
}
const openGoodinfo = (record: Recordable) => {
    openGoodsDetailModal(true, {
        record: record,
    });
};
const OpenGoodsDetail = (record: Recordable) => {
    openGoodsDetailModal(true, {
        record: record,
    });
};

const deletegood = (index) => {
    goods.value.splice(index, 1)
}
const Incell = async () => {
    if (goodsCode.value == "") {
        message.error("没有收料码信息")
        return
    }
    if (boxCode.value == "") {
        message.error("没有容器信息")
        return
    }
    await pickOutByZZ(goodsCode.value,boxCode.value,moveCount.value,pickListCode,uniqueCode).then((res) => {
        if (res.success == true) {
            message.success(res.message)
            openBoxModal(true,{record:boxCode.value})
            oldboxCode.value = ''
            goodsCode.value = ''
            goods.value.length = 0
            boxCode.value = ''
        } else {
            message.error(res.message)
        }
    }).catch((error) => {
        message.error(error.error.message)
    })

}
const state = reactive<{
  selectedRowKeys: Key[];
  loading: boolean;
}>({
  selectedRowKeys: [], // Check here to configure the default column
  loading: false,
});
const hasSelected = computed(() => state.selectedRowKeys.length > 0);


const onSelectChange = (selectedRowKeys: Key[],selectedRows) => {
  console.log('selectedRowKeys changed: ', selectedRows,selectedRows[0].pickListCode);
  state.selectedRowKeys = selectedRowKeys;
  pickListCode = selectedRows[0].pickListCode
  uniqueCode = selectedRows[0].uniqueCode
  if(selectedRows[0].countInCell > selectedRows[0].countInRemaining){
        moveCount.value = selectedRows[0].countInRemaining
    }else{
        moveCount.value = selectedRows[0].countInCell
    }
};
//软件盘弹出屏蔽
function focusFn(e) {
    e.target.setAttribute('readonly', 'readonly');
    setTimeout(() => {
        e.target.removeAttribute('readonly');
    }, 200);
}

</script>

<style scoped lang="less">

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
    padding: 0 16px;
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
