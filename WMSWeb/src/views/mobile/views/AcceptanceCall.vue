<template>
    <div class="components-input-demo-presuffix">
        <Header numb="领用通知单"></Header>
        <div>
            <a-row>
                <a-col :span="12">
                    <a-row>
                        <a-col :span="9" style="text-align:center ;line-height: 32px;">
                            <p>部门:</p>
                        </a-col>
                        <a-col :span="15"><a-select id="inputNumber" style="width: 110px" v-model:value="pickType" @change="reflash"
                            :options="dapdata">
                               
                            </a-select></a-col>
                    </a-row>
                    <a-row style="margin-top:10px">
                        <a-col :span="9" style="text-align:center ; line-height: 32px;">
                            <p>查询条件:</p>
                        </a-col>
                        <a-col :span="15"><a-select v-model:value="findtype" style="width: 110px" @change="reflash">
                            <a-select-option value="materialNameTip">物料名称</a-select-option>
                            <a-select-option value="materialSpecsTip">规格</a-select-option>
                                <a-select-option value="materialCode">物料编号</a-select-option>
                                <a-select-option value="batchTip">生产批号</a-select-option>
                                </a-select></a-col>
                        
                    </a-row>
                    
                </a-col>
                <a-col :span="12">
                    <a-row>
                        <a-col :span="9" style="text-align:center ;line-height: 32px;">
                            <p>排序条件:</p>
                        </a-col>
                        <a-col :span="15"><a-select id="inputNumber" style="width: 110px" v-model:value="orderBy" @change="reflash"
                                >
                                <a-select-option value="1">物料/规格</a-select-option>
                                <a-select-option value="2">生产批号</a-select-option>
                                
                        </a-select></a-col>
                        </a-row>
                        <a-row style="margin-top:10px">
                        <a-col :span="9" style="text-align:center ;line-height: 32px;">
                            <p>查询输入:</p>
                        </a-col>
                        <a-col :span="15"><a-input v-model:value="fliter" :allowClear="true"></a-input></a-col>
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
      

            <a-table ref="tableRef"  :dataSource="dataSource" :row-key="record => record.pickItemId" :columns="columns" :pagination="pagination" @change="handleTableChange"  :scroll="{y:YHOne,x:400}">
                <template #bodyCell="{ column,record }">
                    <template v-if="column.key === 'operation'">
                        <span style="color:coral;font-weight: bold;" @click="openOut(record)">下架</span>
                    </template>
                </template>
            </a-table>
       


    </div>
</template>
<script lang="ts" setup>
import { ref,h,onMounted,defineComponent,computed,onUnmounted} from 'vue';
import { message } from 'ant-design-vue';
import Header from '../header/Header.vue'
// 已删除的Acceptance.ts文件，使用内联实现
const columns = [];
const pagedPickItemsGet = async () => {
  message.info('功能已禁用');
  return { items: [], totalCount: 0 };
};
const allDepartmentsGet = async () => {
  message.info('功能已禁用');
  return [];
};
import { router } from '/@/router';
import { SyncOutlined  } from '@ant-design/icons-vue';
import {
    PagedPickItemQueryDto
  } from '/@/services/ServiceProxies';
 defineComponent({
    // 需要和路由的name一致
    name:"acceptanceCall"
  });
let pickType = ref("")
let findtype = ref("materialNameTip")
let fliter = ref(null)
let orderBy = ref("1")
let dataSource = ref();
let count = ref();
let dapdata = ref([]);
const pagination = ref({
    current: 1,
    defaultPageSize: 10,
    total: 10,
    //showTotal: () => `共 ${11} 条`
})

const handleTableChange = (pag, filters, sorter) => {
            pagination.value.current = pag.current;
            data();
        };
var pageparam = new PagedPickItemQueryDto()
const data = async()=>{
    var params =  new PagedPickItemQueryDto()
    params.departmentId = pickType.value
    params.pageSize = 10
    params.pageIndex =  pagination.value.current
    if(findtype.value == "materialNameTip" ){
        params.materialNameTip = fliter.value
        params.queryBy = 2
    }
    if(findtype.value == "materialSpecsTip" ){
        params.materialSpecsTip = fliter.value
        params.queryBy = 3
    }
    if(findtype.value == "materialCode" ){
        params.materialCode = fliter.value
        params.queryBy = 1
    }
    if(findtype.value == "batchTip" ){
        params.batchTip = fliter.value
        params.queryBy = 4
    }
    params.orderBy = orderBy.value
    pageparam = params
    await pagedPickItemsGet(params).then((res)=>{
        dataSource.value =res.items
        pagination.value.total = res.totalCount
        count.value = dataSource.value.length
    })
}

const reflash = async()=>{
    pagination.value.current = 1
    await data().then((re)=>{
        message.success('查询成功')
    })
}
function openOut(record){
    console.log(record)
    const query = record
    router.replace({ path: '/acceptanceOut',  query  });
}
let screenHeight = ref(window.innerHeight || document.documentElement.clientHeight ||document.body.clientHeight);
let YHOne = ref();
const tableRef = ref<any>();
    onMounted(async() => {
    
    //console.log(tableRef.value.$el.querySelector('.ant-table-thead').clientHeight);
    YHOne.value = screenHeight.value - 158 - tableRef.value.$el.querySelector('.ant-table-thead').clientHeight;
    await allDepartmentsGet().then((res)=>{
        res.forEach((r: any) => {
            dapdata.value.push({
              value: r.id,
              label: r.departmentName,
            });
        })
        pickType.value = res[0].id
    })
    const searchCondition = JSON.parse(window.sessionStorage.getItem('searchCondition'))
    if (searchCondition) {
        console.log(searchCondition.searchValue)
        pickType.value = searchCondition.searchValue.departmentId
        pagination.value.current = searchCondition.searchValue.pageIndex
        orderBy.value = searchCondition.searchValue.orderBy
        findtype.value = searchCondition.searchValue.queryBy
        if(searchCondition.searchValue.queryBy == 1){
            findtype.value = "materialCode"
            fliter.value = searchCondition.searchValue.materialCode
        }
        if(searchCondition.searchValue.queryBy == 2){
            findtype.value = "materialNameTip"
            fliter.value = searchCondition.searchValue.materialNameTip
        }
        if(searchCondition.searchValue.queryBy == 3){
            findtype.value = "materialSpecsTip"
            fliter.value = searchCondition.searchValue.materialSpecsTip
        }
        if(searchCondition.searchValue.queryBy == 4){
            findtype.value = "batchTip"
            fliter.value = searchCondition.searchValue.batchTip
        }

    }
    data()
  })  

  onUnmounted(()=>{
    // 1、设置搜索条件
    const searchCondition = {
        searchValue: pageparam, // 这是当前的搜索值
    }
    // 2、把它存储到 sessionStorage (使用JSON.stringify()将其转化为字符串)
    window.sessionStorage.setItem('searchCondition', JSON.stringify(searchCondition))
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

::v-deep(.ant-table-hide-scrollbar.ant-table-hide-scrollbar) {
    scrollbar-color: initial !important;
}
p {
    margin-bottom: 0em
}
</style>
