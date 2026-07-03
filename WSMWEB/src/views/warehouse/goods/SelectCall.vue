<template>
  <BasicModal
    width="98%"
    :title="t('选择通知单')"
    :canFullscreen="true"
    @ok="submit"
    @cancel="cancel"
    @register="registerModal"
    @visible-change="visibleChange"
    :destroyOnClose="true"
    :maskClosable="false"
  >
    <div>
            <a-row>
                <a-col :span="6">
                    <a-row>
                        <a-col :span="9" style="text-align:center ;line-height: 32px;">
                            <p>部门:</p>
                        </a-col>
                        <a-col :span="15"><a-select id="inputNumber" style="width: 110px" v-model:value="pickType" @change="reflash"
                            :options="dapdata">
                               
                            </a-select></a-col>
                    </a-row>
                    <a-row style="margin-top:10px">
                        <a-col :span="9" style="text-align:center ;line-height: 32px;">
                            <p>生产批号:</p>
                        </a-col>
                        <a-col :span="15"><a-input v-model:value="batchNo" :allowClear="true"></a-input></a-col>
                        
                    </a-row>
                    <a-row style="margin-top:10px">
                        <a-col :span="9" style="text-align:center ;line-height: 32px;">
                            <p>时间:</p>
                        </a-col>
                        <a-col :span="15"><a-range-picker format="MM/DD" v-model:value="date" /></a-col>
                        
                    </a-row>
                </a-col>
                <a-col :span="6">
                    <a-row>
                        <a-col :span="9" style="text-align:center ;line-height: 32px;">
                            <p>材料编号:</p>
                        </a-col>
                        <a-col :span="15"><a-input v-model:value="materialCode" :allowClear="true"></a-input></a-col>
                        </a-row>
                        <a-row style="margin-top:10px">
                        <a-col :span="9" style="text-align:center ;line-height: 32px;">
                            <p>材料名称:</p>
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
                <a-col :span="6">
                    <a-row>
                        <a-col :span="9" style="text-align:center ;line-height: 32px;">
                            <p>成品编号:</p>
                        </a-col>
                        <a-col :span="15"><a-input id="inputNumber" v-model:value="goodsCode" :allowClear="true"
                            >
                               
                            </a-input></a-col>
                    </a-row>
                    <a-row style="margin-top:10px">
                        <a-col :span="9" style="text-align:center ;line-height: 32px;">
                            <p>成品名称:</p>
                        </a-col>
                        <a-col :span="15"><a-input v-model:value="goodsName" :allowClear="true"></a-input></a-col>
                        
                    </a-row>
                   
                </a-col>
                <a-col :span="6">
                    <a-row>
                        <a-col :span="9" style="text-align:center ;line-height: 32px;">
                            <p>领料单号:</p>
                        </a-col>
                        <a-col :span="15"><a-input  v-model:value="orderNo" :allowClear="true"
                            >
                               
                            </a-input></a-col>
                    </a-row>
                    <a-row style="margin-top:10px">
                        <a-col :span="9" style="text-align:center ;line-height: 32px;">
                            <p>车间开单人:</p>
                        </a-col>
                        <a-col :span="15"><a-input v-model:value="pickManName" :allowClear="true"></a-input></a-col>
                        
                    </a-row>
                   
                </a-col>
            </a-row>
        </div>
      

            <a-table ref="tableRef"  :dataSource="dataSource" :row-selection="rowSelection" :row-key="record => record.pickItemId" :columns="columns" :pagination="pagination" @change="handleTableChange" :scroll="{ x: 1500, y: 400 }" >
                <!-- <template #bodyCell="{ column,record }">
                    <template v-if="column.key === 'operation'">
                        <span style="color:coral;font-weight: bold;" @click="openOut(record)">选择</span>
                    </template>
                </template> -->
            </a-table>
  </BasicModal>
</template>

<script lang="ts" setup>
  import { defineComponent ,ref,onMounted} from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { CellAddDto } from '/@/services/ServiceProxies';
  import { useI18n } from '/@/hooks/web/useI18n';
  import moment from 'moment';
  import {
    
  } from '/@/services/ServiceProxies';
  import {  columns,pagedPickItemsGet,allDepartmentsGet } from './Acceptance'
import { message } from 'ant-design-vue';
import { format } from 'path';


    const open = () => {
        }
    const emit = defineEmits(['event'])
    let pickType = ref("")
    let findtype = ref("materialNameTip")
    let materialCode = ref("")
    let batchNo = ref("")
    let fliter = ref("")
    let goodsCode = ref("")
    let goodsName = ref("")
    let pickManName = ref("")
    let orderNo = ref("")
    let orderBy = ref("1")
    let dataSource = ref();
    let date = ref([moment().subtract(1, 'days'),moment().subtract(1, 'days')]);
    let count = ref();
    let dapdata = ref([]);
      const { t } = useI18n();
      const [registerModal, { changeOkLoading, closeModal }] = useModalInner();


      const visibleChange = async (visible: boolean) => {
        if (visible) {
          date.value = [moment().subtract(1, 'days'),moment().subtract(1, 'days')];
        } else {

        }
      };

      // 保存用户
      const submit = async () => {
        try {
          //console.log(selectdate.value)
          emit('event', selectdate.value)
          closeModal()

        } catch (error) {
          changeOkLoading(false);
        }
      };
      const cancel = () => {
        closeModal();
      };
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
    var pageparam = new PickItemForOutboundPagedQueryDto()
    const data = async()=>{
        var params =  new PickItemForOutboundPagedQueryDto()
        console.log(date.value[0].format('YYYY-MM-DD'))
        params.department = pickType.value
        params.materialCode = materialCode.value
        params.materialName = fliter.value
        params.batchNo = batchNo.value
        params.goodsCode = goodsCode.value
        params.goodsName = goodsName.value
        params.noticeOrderNo = orderNo.value
        params.pickManName = pickManName.value
        params.pageSize = 10
        params.pageIndex =  pagination.value.current
        params.recordTimeStart = date.value[0];
        params.recordTimeEnd = date.value[1] 
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
      emit('event', record)
      closeModal()
    }
    onMounted(async() => {
    
    //console.log(tableRef.value.$el.querySelector('.ant-table-thead').clientHeight);
    
    await allDepartmentsGet().then((res)=>{
        res.forEach((r: any) => {
            dapdata.value.push({
              value: r.departmentName,
              label: r.departmentName,
            });
        })
        pickType.value = res[0].departmentName
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
  let selectdate = ref()
  const rowSelection: TableProps['rowSelection'] = {
  onChange: (selectedRowKeys: string[], selectedRows: DataType[]) => {
    console.log(`selectedRowKeys: ${selectedRowKeys}`, 'selectedRows: ', selectedRows);
    selectdate.value = selectedRows;
  },
  getCheckboxProps: (record: DataType) => ({
    name: record.name,
  }),
};
</script>
<style lang="less" scoped>

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
