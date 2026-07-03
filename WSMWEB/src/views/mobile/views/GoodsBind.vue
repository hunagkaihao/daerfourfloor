<template>
    <div class="components-input-demo-presuffix">
        <Header numb="拆箱领用"></Header>
       
        <a-row style="margin-top: 5px;">
            <a-col :span="6">
                <div class="htext">
                    <h1 autofocus="autofocus">收料条形码:</h1>
                </div>
            </a-col>
            <a-col :span="17">
                <a-input v-model:value="goodsCode" placeholder="扫描收料条形码"  
                    :allowClear="true" @focus="focusFn">
                    <template #suffix>
                        <scan-outlined />
                    </template>
                </a-input>
            </a-col>
        </a-row>
        <a-row style="margin-top: 5px;">
            <a-col :span="6">
                <div class="htext">
                    <h1 autofocus="autofocus">出库容器:</h1>
                </div>
            </a-col>
            <a-col :span="17">
                <a-input v-model:value="boxCode" placeholder="输入容器编号"  @keyup.enter="scanboxCode"
                    :allowClear="true" >
                </a-input>
            </a-col>
        </a-row>
       
        <div style="height: 26vh; overflow:auto;">
            <a-card style="margin:5px" v-show="showtable" >
                <template #title>

                    物料:{{good.materialName}}{{good.specs}}

                </template>

                <a-row>
                    <a-col :span="12">
                        <p>物料编号:{{good.materialCode}}</p>
                        <p>计量单位:{{good.unit}}</p>
                        <p>通知日期:{{good.pickListDate}}</p>
                        <p>部门:{{good.deptName}}</p>
                        <p>领用成品编号:{{good.goodsCode}}</p>                       
                        <!-- <p>领用成品名称:{{good.goodsName}}</p> -->
                        <p>领料单号:{{good.pickListCode}}</p>
                        <p>容器内数量:{{good.countInCell}}</p>
                    </a-col>
                    <a-col :span="12">
                        <p>领用生产批号:{{good.pickBatch}}</p>
                        <p>领用单位:{{good.deptName}}</p>
                        <p>加工单位:{{good.gysName}}</p>
                        <p>领用成品名称:{{good.goodsName}}</p>
                        <!-- <p>领用成品名称:{{good.goodsSpecs}}</p> -->
                        <p>领用数量:{{good.countToPick}}</p>
                        <p>已领数量:{{good.pickedCount}}</p>
                    </a-col>
                </a-row>
            </a-card>

        </div>
        <a-table row-key="pickListCode" :row-selection="{ selectedRowKeys: state.selectedRowKeys,  onChange: onSelectChange,type: 'radio', }" :dataSource="dataSource" :columns="ZZoutcellcolumns" :pagination="false" :scroll="{ x: screenWidth, y: 128 }">
            <template #bodyCell="{ column, record }">
                <template v-if="column.key === 'operation'">
                    <span @click="OpenGoodsDetail(record)">查看</span>
                </template>
            </template>
        </a-table>
        <a-row style="margin-top: 5px;">
            <a-col :span="6">
                <div class="htext">
                    <h1 autofocus="autofocus">目标容器:</h1>
                </div>
            </a-col>
            <a-col :span="17">
                <a-input v-model:value="newboxCode" placeholder="扫描目标容器"  ref="Ref"
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
        <a-row style="margin-top: 5px;">
            <a-col :span="6">
                <div class="htext">
                    <h1 autofocus="autofocus">领用数量:</h1>
                </div>
            </a-col>
            <a-col :span="17">
                <a-input v-model:value="moveCount" placeholder="输入领用数量"  
                    :allowClear="true">


                </a-input>
            </a-col>
        </a-row>

      
        <div class="tab-bar">
            <a-button @click="Incell" type="primary" class="btn_4">
                物料绑定
            </a-button>

        </div>
    </div>
    <GoodsDetail @register="registerGoodsDetailModal"></GoodsDetail>
    <BoxModal @register="registerBoxModal" ></BoxModal>
</template>
<script lang="ts" setup>
import { defineComponent,reactive, ref, computed, onMounted, h } from 'vue';
import { ScanOutlined, TableOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
import { PickItemDto } from '/@/services/ServiceProxies';
import { useMessage } from '/@/hooks/web/useMessage';
import Header from '../header/Header.vue'
// 已删除的TiaoboIncell.ts文件，使用内联实现
const stockWithBarcodeGetInCell = async () => {
  message.info('功能已禁用');
  return null;
};
const ZZoutcellcolumns = [];
const stocksGetInCell = async () => {
  message.info('功能已禁用');
  return [];
};
const pickOutByBox = async () => {
  message.info('功能已禁用');
  return null;
};
const getByBarcodeBoxCode = async () => {
  message.info('功能已禁用');
  return null;
};
// 已删除的GoodsDetail.vue文件，使用简单的替代组件
const GoodsDetail = { template: '<div>功能已禁用</div>' };
// 已删除的BoxModal.vue文件，使用简单的替代组件
const BoxModal = { template: '<div>功能已禁用</div>' };
import { useModal } from '/@/components/Modal';
import { useUserStore } from '/@/store/modules/user';
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
let newcellCode = ref<string>('');
let newboxCode = ref<string>('');
let oldcellCode = ref<string>('');
let moveCount = ref()
var goods = ref([]);
let good = ref()
good.value = new PickItemDto()
var dataSource = ref([]);
let screenHeight = ref(window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight);
let screenWidth = ref((window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth) - 8)
let pickListCode = ''
let uniqueCode = ''
let showtable = ref(true)
let goodheight = ref(36)
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



const OpenGoodsDetail = (record: Recordable) => {
    openGoodsDetailModal(true, {
        record: record,
    });
};


const Incell = async () => {
    if (goodsCode.value == "") {
        message.error("没有收料码信息")
        return
    }

    await pickOutByBox(goodsCode.value,boxCode.value,moveCount.value,pickListCode,uniqueCode,newboxCode.value,newcellCode.value).then((res) => {
        if (res.success == true) {
            message.success(res.message)   
            moveCount.value = ''
            scanboxCode()
            openBoxModal(true,{record:boxCode.value})
        } else {
            message.error(res.message)
        }
    }).catch((error) => {
        message.error(error.error.message)
    })

}
const scanboxCode = async () => {
    reset()
    await getByBarcodeBoxCode(goodsCode.value,boxCode.value).then((res) => {
        good.value = res.pickDto
        res.items.forEach((e) => {
            dataSource.value.push(e)
        })
        if (dataSource.value.length > 0) {
            state.selectedRowKeys = [dataSource.value[0].pickListCode];
            pickListCode = dataSource.value[0].pickListCode
            uniqueCode = dataSource.value[0].uniqueCode
            if(dataSource.value[0].countInCell > dataSource.value[0].countInRemaining){
                moveCount.value = dataSource.value[0].countInRemaining
            }else{
                moveCount.value = dataSource.value[0].countInCell
            }
            
        }
        message.success("扫描容器成功")
    }).catch((err)=>{
        message.error(err.error.message)
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
function reset(){
    moveCount.value = ''
    dataSource.value.length = 0
    good.value = new PickItemDto()
}
</script>

<style scoped lang="less">
.btn_4 {
    margin: auto;
    margin-top: 5px;
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