<template>
  <BasicModal
    width="98%"
    :title="t('新增出库单')"
    :canFullscreen="true"
    @ok="submit"
    @cancel="cancel"
    @register="registerModal"
    @visible-change="visibleChange"
    :destroyOnClose="true"
    :maskClosable="false"
  >
    <a-button @click="openCreateOrderModal">选择通知单</a-button>
    <!-- <a-card v-for="(good,index) in goodlist" style="margin:5px" v-show="showtable" >

                <a-row>
                    <a-col :span="12">
                        <p>物料编号:{{good.materialCode}}生产批号:{{good.batchNo}}领用类型:{{good.pickType}}领料单号:{{good.pickListCode}}</p>

                    </a-col>
                    <a-col :span="12">
                        <p>物料名称:{{good.materialName}}型号规格:{{good.specs}}领用单位:{{good.department}}领用数量:{{good.countToPick}}未领数量:{{good.unpickedCount}}</p>
                    </a-col>
                </a-row>
                <a-button @click="adddetail">添加</a-button>
     </a-card> -->
         <a-table ref="tableRef" pagination="false"  :dataSource="goodlist" :row-key="record => record.key" :columns="ordercolumns"   :scroll="{x:400}">
                <!-- <template #bodyCell="{ column,record }">
                    <template v-if="column.key === 'operation'">
                        <span style="color:coral;font-weight: bold;" @click="deletedetail(record.key)">删除</span>
                    </template>
                </template> -->
    </a-table>
    <a-button @click="autodetails">自动添加明细</a-button>
    <a-button @click="adddetails">手动添加明细</a-button>
     <h1>出库单明细:</h1>
    <a-table ref="tableRef"  :dataSource="dataSource" :row-key="record => record.key" :columns="detailcolumns"   :scroll="{x:400}">
                <template #bodyCell="{ column,record }">
                    <template v-if="column.key === 'operation'">
                        <span style="color:coral;font-weight: bold;" @click="deletedetail(record.key)">删除</span>
                    </template>
                </template>
    </a-table>
  </BasicModal>
  <SelectCall @event="eventFnboxselect"  @register="registerCreateOrderModal"></SelectCall>
  <Peoples @event="eventFnboxselect2"  @register="registerPeoplesModal"></Peoples>
  <Autos @event="eventFnboxselect2"  @register="registerAutoModal"></Autos>
  <SelectOrderDetail @event="eventFnboxselect2"  @register="registerOrderDetailModal"></SelectOrderDetail>
</template>

<script lang="ts" setup>
  import { ref } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import {  } from '/@/services/ServiceProxies';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { useModal } from '/@/components/Modal';
  import {detailcolumns,ordercolumns,add} from './Acceptance'
  import SelectCall from './SelectCall.vue';
  import SelectOrderDetail from './SelectOrderDetail.vue';
  import Peoples  from './Peoples.vue';
  import Autos from './Autos.vue';
  import { message } from 'ant-design-vue';
  const emit = defineEmits(['reload'])
      const { t } = useI18n();
      const [registerCreateOrderModal, { openModal: openCreateOrderModal }] = useModal();
      const [registerOrderDetailModal, { openModal: openOrderDetailModal }] = useModal();
      const [registerPeoplesModal, { openModal: openPeoplesModal }] = useModal();
      const [registerAutoModal, { openModal: openAutoModal }] = useModal();
      const [registerModal, { changeOkLoading, closeModal }] = useModalInner();

      let good = ref("");
      let goodlist = ref([])
      let dataSource = ref<OutboundItemCreateDto[]>([]);
      let showtable = ref(true)
      const visibleChange = async (visible: boolean) => {
        if (visible) {
        } else {
          reset();
        }
      };

      // 保存用户
      const submit = async () => {
        try {
          let param = new OutboundListCreateDto()
          param.receivingUnit=goodlist.value[0].department;
          param.type=goodlist.value[0].pickType;
          param.items = dataSource.value
          await add(param).then(res=>{
            message.success('新增成功');
            closeModal() 
          })
          emit('reload');
       
        } catch (error) {
          changeOkLoading(false);
        }
      };
      const cancel = () => {
        reset();
        closeModal();
      };
  function reset(){
    good.value = "";
    dataSource.value = [];
  }
  const eventFnboxselect = async(val) => {
    //判断车间是否一致，否则不需切换
    if(goodlist.value.length == 0){
      goodlist.value = val
    }else if(goodlist.value[0].department != val[0].department && dataSource.value.length>0){
      message.error('请选择相同车间的通知单');
      return;  
    }else{
      goodlist.value = val
    }
  }
  let index = 1;
  const eventFnboxselect2 = async(val) => {
    console.log(val)
    
    for(let i=0;i<val.length;i++){
      let item = new OutboundItemCreateDto()
      item.materialCode=val[i].materialCode;
      item.materialName=val[i].materialName;
      item.specs=val[i].specs;
      item.unit=val[i].unit;
      item.checkNo=val[i].checkNo;
      for(let j=0;j<goodlist.value.length;j++){
        if(val[i].materialCode == goodlist.value[j].materialCode){
          item.batchNo=goodlist.value[j].batchNo;
          break;
        }
      }
      
      item.quantity=val[i].allocatedCount;
      item.key = index++; // 假设 index 是唯一的标识符;

      dataSource.value.push(item)
    }
    
  }
  const adddetail = async() => {
    openOrderDetailModal(true, {
        record: goodlist.value,
    }) 
  }
  const adddetails = async() => {
    openPeoplesModal(true, {
        record: goodlist.value,
    }) 
  }
   const autodetails = async() => {
    openAutoModal(true, {
        record: goodlist.value,
    }) 
  }
  const deletedetail= (key: string) => {
    console.log(key)
    dataSource.value = dataSource.value.filter(item => item.key !== key);
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
