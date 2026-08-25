<template>
  <BasicModal
    width="98%"
    :title="t('节点流转记录')"
    :canFullscreen="true"
    @ok="submit"
    @cancel="cancel"
    @register="registerModal"
    @visible-change="visibleChange"
    :destroyOnClose="true"
    :maskClosable="false"
  >
    <div>
            <a-table :columns="innerColumns" :data-source="data" :pagination="false">

        </a-table>
        </div>
      
<h1 style="text-align: center;">流转记录</h1>
            <a-table ref="tableRef"  :dataSource="dataSource"  :columns="hiscolumns"   :scroll="{ x: 1500, y: 400 }" >

            </a-table>
  </BasicModal>
</template>

<script lang="ts" setup>
  import { ref,onMounted} from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { useI18n } from '/@/hooks/web/useI18n';
  import moment from 'moment';
  // 移除已删除的Acceptance.ts文件导入，使用内联实现
  const columns = [];
  const pagedList = async () => {
    // 不调用接口，返回空数据
    return [];
  };
  const hiscolumns = [];
import { message } from 'ant-design-vue';
const innerColumns = [
{ title: '领用项号', dataIndex: 'uniqueCode', key: 'uniqueCode' },
  { title: '物料编码', dataIndex: 'materialCode', key: 'materialCode' },
  { title: '物料名称', dataIndex: 'materialName', key: 'materialName' },
  { title: '物料规格',dataIndex: 'specs', key: 'specs' },
  { title: '单位', dataIndex: 'unit', key: 'unit' },
  { title: '领用数量', dataIndex: 'countToPick', key: 'countToPick' },
  { title: '已领数量', dataIndex: 'pickedCount', key: 'pickedCount' },
  { title: '领用状态', dataIndex: 'pickItemStatus', key: 'pickItemStatus' },
  { title: '未领数量', dataIndex: 'countInRemaining', key: 'countInRemaining' },
];
    const emit = defineEmits(['event'])
    let dataSource = ref();
    let data = ref([]);
    let date = ref([moment().subtract(1, 'days'),moment().subtract(1, 'days')]);
      const { t } = useI18n();
      const [registerModal, { changeOkLoading, closeModal }] = useModalInner(async(record)=>{
        data.value.length = 0;
        data.value.push(record.record)
        getdata(record.record.uniqueCode)
    });

    async function getdata(uniqueCode: string){

        await pagedList(uniqueCode,1,1000,0,1000).then(res=>{
            dataSource.value = res.items
        })
    }

      const visibleChange = async (visible: boolean) => {
        if (visible) {
          
        } else {

        }
      };

      // 保存用户
      const submit = async () => {
        try {

          closeModal()

        } catch (error) {
          changeOkLoading(false);
        }
      };
      const cancel = () => {
        closeModal();
      };
      

    onMounted(async() => {

    })
    
   
    


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
