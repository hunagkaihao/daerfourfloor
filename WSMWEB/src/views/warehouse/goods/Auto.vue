<template>
  <BasicModal
    :width="800"
    :title="t('自动分配')"
    :canFullscreen="false"
    @ok="submit"
    @cancel="cancel"
    @register="registerModal"
    @visible-change="visibleChange"
    :destroyOnClose="true"
    :maskClosable="false"
  >

    <a-table ref="tableRef"  :dataSource="dataSource" :row-key="record => record.pickItemId" :columns="autocolumns" :pagination="pagination" @change="handleTableChange"  :scroll="{x:400}">

    </a-table>
   
  </BasicModal>
</template>

<script lang="ts" setup>
  import { defineComponent ,ref,onMounted} from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { BasicForm, useForm } from '/@/components/Form/index';
  import { CellAddDto } from '/@/services/ServiceProxies';
  import { useI18n } from '/@/hooks/web/useI18n';
  import {
    
  } from '/@/services/ServiceProxies';
  import {  autocolumns,autoAllocateStockDetail } from './Acceptance'
import { message } from 'ant-design-vue';

    const emit = defineEmits(['event'])
    let materialCode = ref("")
    let quantity = ref(0)
    let dataSource = ref();
    let count = ref();
    let good = ref("");
      const { t } = useI18n();
      const [registerModal, { changeOkLoading, closeModal }] = useModalInner((data) => {
        console.log(data); // 这里可以获取传递过来的数据
        good.value = data.record;
        materialCode.value = data.record.materialCode
        quantity.value = data.record.unpickedCount
        getdata()
      });
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
        emit('event', dataSource.value)
        closeModal()
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
    var pageparam = new AutoAllocateStockDetailInputDto()
    const getdata = async()=>{
        var params =  new AutoAllocateStockDetailInputDto()
        params.materialCode = materialCode.value
        params.quantity = quantity.value
      
        pageparam = params
        await autoAllocateStockDetail(params).then((res)=>{
            dataSource.value =res
            
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

     
</script>
<style lang="less" scoped>
  .ant-checkbox-wrapper + .ant-checkbox-wrapper {
    margin-left: 0px;
  }
</style>
