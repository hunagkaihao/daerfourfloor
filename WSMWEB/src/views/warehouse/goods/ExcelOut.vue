<template>
    <BasicModal
      :width="1000"
      :height="600"
      :title="t('EXCEL批量导出')"
      :canFullscreen="false"
      @ok="submit"
      @cancel="cancel"
      @register="registerModal"
      @visible-change="visibleChange"
      :destroyOnClose="true"
      :maskClosable="false"
    >
      <a-row style="height: 20%">
        <ImpExcel @success="loadDataSuccess" dateFormat="YYYY-MM-DD">
          <a-button class="m-3"> 导入Excel </a-button>
        </ImpExcel>
      </a-row>
      <a-row style="height: 80%">
        <BasicTable
          v-for="(table, index) in tableListRef"
          :key="index"
          :title="table.title"
          :columns="table.columns"
          :dataSource="table.dataSource"
          style="height: 400px"
        />
      </a-row>
      <!-- <a-row>
        <a-button preIcon="ant-design:plus-circle-outlined" type="primary" @click="cancel">
          {{ t('取消') }}
        </a-button>
        <a-button preIcon="ant-design:plus-circle-outlined" type="primary" @click="subit">
          {{ t('导入') }}
        </a-button>
      </a-row> -->
    </BasicModal>
  </template>
  <script lang="ts">
    import { defineComponent, ref } from 'vue';
    import { BasicModal, useModalInner } from '/@/components/Modal';
    import { ImpExcel, ExcelData  } from '/@/components/Excel';
    import { BasicTable, BasicColumn } from '/@/components/Table';
    import { useI18n } from '/@/hooks/web/useI18n';
    import { message } from 'ant-design-vue';
    //   import { createManyGoodsAsync } from './Goods';
    export default defineComponent({
      name: 'ImportOut',
      components: { BasicTable, ImpExcel, BasicModal },
      emits: ['reload'],
      setup(_, { emit }) {
        const { t } = useI18n();
        const [registerModal, { changeOkLoading, closeModal }] = useModalInner();
        const visibleChange = async (visible: boolean) => {
          if (visible) {
          } else {
            //   resetFields();
          }
        };
  
        // 保存用户
        const submit = async () => {
          try {
            if (tableListRef.value.length == 0) {
              message.warn(t('数据为空'));
              return;
            }
  
            let request = tableListRef.value[0].dataSource as [];
            console.log(request);
            //   let params = {
            //     value: '测试',
            //   };
            //   await createManyGoodsAsync({
            //     request,
            //     changeOkLoading,
            //     closeModal,
            //   });
            emit('reload', request);
            message.success(t('common.operationSuccess'));
            closeModal();
          } catch (error) {
            changeOkLoading(false);
          }
        };
        const cancel = () => {
          // resetFields();
          closeModal();
        };
        const tableListRef = ref<
          {
            title: string;
            columns?: any[];
            dataSource?: any[];
          }[]
        >([]);
        function loadDataSuccess(excelDataList: ExcelData[]) {
          tableListRef.value = [];
          console.log(excelDataList);
          for (const excelData of excelDataList) {
            const {
              header,
              results,
              meta: { sheetName },
            } = excelData;
            const columns: BasicColumn[] = [];
            for (const title of header) {
              columns.push({
                title: t('routes.material.goodsManagement_' + title),
                dataIndex: title,
              });
            }
            tableListRef.value.push({ title: sheetName, dataSource: results, columns });
          }
        }
        return {
          loadDataSuccess,
          tableListRef,
          t,
          cancel,
          registerModal,
          submit,
          visibleChange,
        };
      },
    });
  </script>
  