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
                    <a-row style="margin-top:10px">
                        <a-col :span="9" style="text-align:center ;line-height: 32px;">
                            <p>开始时间:</p>
                        </a-col>
                        <a-col :span="15"><a-date-picker  format="YYYY/MM/DD" v-model:value="date1" /></a-col>

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
                        <a-col :span="9" style="text-align:center ;line-height: 32px;">
                            <p>结束时间:</p>
                        </a-col>
                        <a-col :span="15"><a-date-picker  format="YYYY/MM/DD" v-model:value="date2" /></a-col>

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
// 移除API调用，保留前端界面
const columns = [
  {
    title: '领用单号',
    dataIndex: 'pickListCode',
    key: 'pickListCode',
  },
  {
    title: '部门',
    dataIndex: 'departmentName',
    key: 'departmentName',
  },
  {
    title: '领用人',
    dataIndex: 'pickerName',
    key: 'pickerName',
  },
  {
    title: '状态',
    dataIndex: 'status',
    key: 'status',
  },
  {
    title: '创建时间',
    dataIndex: 'creationTime',
    key: 'creationTime',
  }
];

// 移除API调用
const pagedPickItemsGet = async (queryDto) => {
  // 不调用接口，返回空数据
  return { items: [], totalCount: 0 };
};

const allDepartmentsGet = async () => {
  // 不调用接口，返回空数据
  return [];
};
import { router } from '/@/router';
import moment from 'moment';
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
let date1 = ref(moment().subtract(1, 'days'));
let date2 = ref(moment().subtract(1, 'days'));
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
    // 移除API调用，不执行任何操作
    dataSource.value = []
    pagination.value.total = 0
    count.value = 0
}

const reflash = async()=>{
    // 移除API调用，不执行任何操作
    pagination.value.current = 1
    await data()
}
function openOut(record){
    // 移除跳转功能，不执行任何操作
    console.log('下架功能已禁用', record)
}
let screenHeight = ref(window.innerHeight || document.documentElement.clientHeight ||document.body.clientHeight);
let YHOne = ref();
const tableRef = ref<any>();
    onMounted(async() => {
    
    //console.log(tableRef.value.$el.querySelector('.ant-table-thead').clientHeight);
    YHOne.value = screenHeight.value - 158 - tableRef.value.$el.querySelector('.ant-table-thead').clientHeight;
    // 移除API调用，不加载部门数据
    dapdata.value = []
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
const mode2 = ref<any>('day');
const value = ref<[Dayjs, Dayjs]>();



const handleChange = (val: [Dayjs, Dayjs]) => {
  value.value = val;
};



const handlePanelChange2 = (val: [Dayjs, Dayjs], mode: any[]) => {
  value.value = val;

};
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
