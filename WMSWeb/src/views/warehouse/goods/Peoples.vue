<template>
  <BasicModal
    :width="800"
    :title="t('手动分配')"
    :canFullscreen="false"
    @ok="submit"
    @cancel="cancel"
    @register="registerModal"
    @visible-change="visibleChange"
    :destroyOnClose="true"
    :maskClosable="false"
  >

    <a-table  :dataSource="dataSource"  :columns="columns4" :pagination="pagination" @change="handleTableChange" >
        <template #bodyCell="{ column, text, record }">
              <template v-if="column.key === 'allocatedCount'">
                <!-- <div class="editable-cell">
                  <div v-if="editableData[record.key]" class="editable-cell-input-wrapper">
                    <a-input v-model:value="editableData[record.key].allocatedCount" @pressEnter="save(record.key)" />
                    <check-outlined class="editable-cell-icon-check" @click="save(record.key)" />
                  </div>
                  <div v-else class="editable-cell-text-wrapper">
                    {{ text || ' ' }}
                    <edit-outlined class="editable-cell-icon" @click="edit(record.key)" />
                  </div>
                </div> -->
                <a-input v-model:value="record.allocatedCount" />
        
              </template>
      </template>
    </a-table>

  </BasicModal>
</template>

<script lang="ts" setup>
  import { defineComponent ,ref,reactive} from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { useI18n } from '/@/hooks/web/useI18n';
  import {
    
    
  } from '/@/services/ServiceProxies';
  import {  columns4,batchAutoAllocateStockDetailWithTotal } from './Acceptance';
  import { CheckOutlined, EditOutlined } from '@ant-design/icons-vue';
  import type { Ref, UnwrapRef } from 'vue';
  import { cloneDeep } from 'lodash-es';
    const emit = defineEmits(['event'])
    let materialCode = ref("")
    let dataSource = ref();
    let quantity = ref()
    let good = ref([]);
      const { t } = useI18n();
      const [registerModal, { changeOkLoading, closeModal }] = useModalInner((data) => {
        console.log(data); // 这里可以获取传递过来的数据
        good.value = data.record;
        getdata()
      });


      const visibleChange = async (visible: boolean) => {
        if (visible) {
          
        } else {

        }
      };
      const state = reactive<{
      selectedRowKeys: Key[];
      loading: boolean;
    }>({
      selectedRowKeys: [], // Check here to configure the default column
      loading: false,
    });
  // const onSelectChange = (selectedRowKeys: Key[]) => {
  //   console.log('selectedRowKeys changed: ', selectedRowKeys);
  //   state.selectedRowKeys = selectedRowKeys;
  // };
      // 保存用户
      const submit = async () => {
        console.log(dataSource.value)
        let res =[]
        for(let i = 0;i<dataSource.value.length;i++){
            if(dataSource.value[i].allocatedCount != "" && dataSource.value[i].allocatedCount!= undefined && dataSource.value[i].allocatedCount!= 0 && dataSource.value[i].allocatedCount!= "0"){
                res.push(dataSource.value[i])
            }
        }
        emit('event', res)
        closeModal()
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
            getdata();
        };
    
    const getdata = async()=>{
        var params =  new BatchAutoAllocateStockDetailInputDto()
        params.items = []
        for(let i = 0;i<good.value.length;i++){
            let item = new AutoAllocateStockDetailInputDto()
            item.materialCode = good.value[i].materialCode;
            item.quantity = good.value[i].unpickedCount;
            params.items.push(item)
        }

       

        await batchAutoAllocateStockDetailWithTotal(params).then((res)=>{
            for(let i = 0;i<res.length;i++){
                res[i].key = i+1
            }
            dataSource.value =res
            pagination.value.total = res.totalCount
        })
    }
interface DataItem {
  key: string;
  allocatedCount: string;
  checkNo: string;
}
const editableData: UnwrapRef<Record<string, DataItem>> = reactive({});

const edit = (key: string) => {
  editableData[key] = cloneDeep(dataSource.value.filter(item => key === item.key)[0]);
};
const save = (key: string) => {
  Object.assign(dataSource.value.filter(item => key === item.key)[0], editableData[key]);
  delete editableData[key];
};
</script>
<style lang="less" scoped>
.editable-cell {
  position: relative;
  .editable-cell-input-wrapper,
  .editable-cell-text-wrapper {
    padding-right: 24px;
  }

  .editable-cell-text-wrapper {
    padding: 5px 24px 5px 5px;
  }

  .editable-cell-icon,
  .editable-cell-icon-check {
    position: absolute;
    right: 0;
    width: 20px;
    cursor: pointer;
  }

  .editable-cell-icon {
    margin-top: 4px;
    display: none;
  }

  .editable-cell-icon-check {
    line-height: 28px;
  }

  .editable-cell-icon:hover,
  .editable-cell-icon-check:hover {
    color: #108ee9;
  }

  .editable-add-btn {
    margin-bottom: 8px;
  }
}
.editable-cell:hover .editable-cell-icon {
  display: inline-block;
}
</style>
