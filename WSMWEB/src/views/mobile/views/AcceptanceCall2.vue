<template>
    <div class="components-input-demo-presuffix">
        <Header numb="检验入库下架通知单"></Header>
        <div>
            <a-row>
                <a-col :span="12">
                    <a-row style="margin-top:10px">
                        <a-col :span="9" style="text-align:center ; line-height: 32px;">
                            <p>查询条件:</p>
                        </a-col>
                        <a-col :span="15"><a-select v-model:value="findtype" style="width: 110px" @change="data">
                            <a-select-option value="materialCode">物料编号</a-select-option>
                            <a-select-option value="materialName">物料名称</a-select-option>
                                <a-select-option value="checkNo">检验编号</a-select-option>
                                <!-- <a-select-option value="checkNoTip ">复检通知单</a-select-option> -->
                                </a-select></a-col>
                        
                    </a-row>
                    
                </a-col>
                <a-col :span="12">
                    <a-row style="margin-top:10px">
                        <a-col :span="9" style="line-height: 32px;">
                            <p>查询输入:</p>
                        </a-col>
                        <a-col :span="15"><a-input v-model:value="fliter"></a-input></a-col>
                    </a-row>
                    <a-row style="margin-top:10px">
                        <a-col :span="15" style=" line-height: 32px;">
                            <p>通知单数量:{{ count }}</p>
                        </a-col>
                        <a-col :span="9"><a-button type="primary" @click="reflash" >查询</a-button></a-col>
                    </a-row>
                </a-col>
            </a-row>
        </div>



        <a-table ref="tableRef"  :dataSource="dataSource" :columns="columns2" :pagination="true" :scroll="{y:YHOne}">
                <template #bodyCell="{ column,record }">
                    <template v-if="column.key === 'operation'">
                        <span style="color:coral;font-weight: bold;" @click="openOut(record)">下架</span>
                    </template>
                </template>
            </a-table>
   


    </div>
</template>
<script lang="ts" setup>
import {  ref,  h ,onMounted} from 'vue';
import { ScanOutlined, TableOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
import { useMessage } from '/@/hooks/web/useMessage';
import Header from '../header/Header.vue'
// 已删除的Overdue.ts文件，使用内联实现
const columns2 = [];
const getPagedCheckInItems = async () => {
  message.info('功能已禁用');
  return { items: [], totalCount: 0 };
};
import { router } from '/@/router';
import { SyncOutlined  } from '@ant-design/icons-vue';
import {
    PagedCheckItemQueryDto
  } from '/@/services/ServiceProxies';
let good = ref("fan")


let findtype = ref("materialName")
let fliter = ref()
let dataSource = ref();
let count = ref();
const data = async()=>{
    var params =  new PagedCheckItemQueryDto()
    if(findtype.value == "materialCode" ){
        params.materialCode = fliter.value
        params.queryBy = 1
    }
    if(findtype.value == "materialName" ){
        params.materialName = fliter.value
        params.queryBy = 2
    }
    if(findtype.value == "checkNo" ){
        params.checkNo = fliter.value
        params.queryBy = 3
    }
    await getPagedCheckInItems(params).then((res)=>{
        console.log(res)
        dataSource.value =res
        count.value = dataSource.value.length
    })
}
data()
const reflash = async()=>{
    await data().then((re)=>{
        message.success('刷新成功')
    })
}
function openOut(record){
    console.log(record)
    const query = record
    router.replace({ path: '/acceptanceCall2',  query  });
}
let screenHeight = ref(window.innerHeight || document.documentElement.clientHeight ||document.body.clientHeight);
let YHOne = ref();
const tableRef = ref<any>();
    onMounted(() => {
    console.log(tableRef.value.$el.querySelector('.ant-table-thead').clientHeight);
    YHOne.value = screenHeight.value - 118 - tableRef.value.$el.querySelector('.ant-table-thead').clientHeight;
    
  })  




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
    font-size: 14;
    min-height: 0px;
}

::v-deep(.ant-card-head-title) {
    padding: 0px;
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

p {
    margin-bottom: 0em
}
</style>
