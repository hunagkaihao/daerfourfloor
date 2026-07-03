<template>
  <BasicModal
    :width="800"
    :title="t('库存信息')"
    :canFullscreen="false"
    @ok="submit"
    @cancel="cancel"
    @register="registerModal"
    @visible-change="visibleChange"
    :destroyOnClose="true"
    :maskClosable="false"
  >
  <a-card style="margin:5px"  >

                <a-row>
                    <a-col :span="12">
                        <p>物料编号:{{good.materialCode}}</p>
                        <p>生产批号:{{good.batchNo}}</p>
                        <p>领用类型:{{good.pickType}}</p>
                        <p>领料单号:{{good.pickListCode}}</p>
                    </a-col>
                    <a-col :span="12">
                        <p>物料名称:{{good.materialName}}</p>
                        <p>型号规格:{{good.specs}}</p>
                        <p>领用单位:{{good.department}}</p>
                        <p>领用数量:{{good.countToPick}}</p>
                        <p>未领数量:{{good.unpickedCount}}</p>
                    </a-col>
                </a-row>
     </a-card>
      <a-button type="primary" @click="auto" >自动分配</a-button>
    <a-button type="primary" @click="people" >手动分配</a-button>
    <a-table ref="tableRef"  :dataSource="dataSource" :row-key="record => record.pickItemId" :columns="columns2" :pagination="pagination" @change="handleTableChange"  :scroll="{x:400}">

    </a-table>
   
  </BasicModal>
  <Auto  @event="eventFnboxselect"  @register="registerAutoModal"></Auto>
  <People  @event="eventFnboxselect"  @register="registerPeopleModal"></People>
</template>

<script lang="ts" setup>
  import { defineComponent ,ref,onMounted} from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { BasicForm, useForm } from '/@/components/Form/index';
  import { CellAddDto } from '/@/services/ServiceProxies';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { useModal } from '/@/components/Modal';
  import {
    
  } from '/@/services/ServiceProxies';
  import {  columns2,stockDetailByCheckNoAreaPaged } from './Acceptance'
  import { message } from 'ant-design-vue';
  import Auto from './Auto.vue';
  import People from './People.vue';
    const emit = defineEmits(['event'])
    let materialCode = ref("")
    let dataSource = ref();
    let count = ref();
    let good = ref("");
    let dapdata = ref([]);
      const { t } = useI18n();
      const [registerModal, { changeOkLoading, closeModal }] = useModalInner((data) => {
        console.log(data); // 这里可以获取传递过来的数据
        good.value = data.record;
        materialCode.value = data.record.materialCode
        getdata()
      });
      const [registerAutoModal, { openModal: openAutoModal }] = useModal();
      const [registerPeopleModal, { openModal: openPeopleModal }] = useModal();
      const [registerCellForm, { getFieldsValue, validate, resetFields }] = useForm({
        labelWidth: 120,

        showActionButtonGroup: false,
      });

      const visibleChange = async (visible: boolean) => {
        if (visible) {
          
        } else {
          resetFields();
        }
      };

      // 保存用户
      const submit = async () => {
        try {
          let request = getFieldsValue() as CellAddDto;

        } catch (error) {
          changeOkLoading(false);
        }
      };
      const cancel = () => {
        resetFields();
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
            getdata();
        };
    var pageparam = new StockDetailByCheckNoPagedQueryDto()
    const getdata = async()=>{
        var params =  new StockDetailByCheckNoPagedQueryDto()
        params.materialCode = materialCode.value
        params.pageSize = 10
        params.pageIndex =  pagination.value.current
       
      
        pageparam = params
        await stockDetailByCheckNoAreaPaged(params).then((res)=>{
            dataSource.value =res.items
            pagination.value.total = res.totalCount
            count.value = dataSource.value.length
        })
    }
      const reflash = async()=>{
            pagination.value.current = 1
            await getdata().then((re)=>{
                message.success('查询成功')
            })
        }

    function openOut(record){
      emit('event', record)
      closeModal()
    }
    const eventFnboxselect = async(val) => {
      console.log(val)
      emit('event', val)
      closeModal()
    }
    function auto(){
      openAutoModal(true,{record: good.value}) 
    }
    function people(){
      openPeopleModal(true,{record: good.value}) 
    }
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
