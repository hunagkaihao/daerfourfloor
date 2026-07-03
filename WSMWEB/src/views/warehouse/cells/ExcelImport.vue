<template>
  <BasicModal
    :width="800"
    :height="600"
    :title="t('EXCEL批量导入')"
    :canFullscreen="false"
    @ok="submit"
    @cancel="cancel"
    @register="registerModal"
    @visible-change="visibleChange"
    :destroyOnClose="true"
    :maskClosable="false"
  >
    <ImpExcel @success="loadDataSuccess" dateFormat="YYYY-MM-DD">
      <a-button class="m-3"> 导入Excel </a-button>
    </ImpExcel>
    <BasicTable
      v-for="(table, index) in tableListRef"
      :key="index"
      :title="table.title"
      :columns="table.columns"
      :dataSource="table.dataSource"
    />
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
  import { ImpExcel, ExcelData } from '/@/components/Excel';
  import { BasicTable, BasicColumn } from '/@/components/Table';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { createManyCellsAsync } from './Cell';
  export default defineComponent({
    name: 'ImportCells',
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
          let request = tableListRef.value[0].dataSource;
          console.log(request);
          await createManyCellsAsync({
            request,
            changeOkLoading,
            closeModal,
          });
          emit('reload');
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
            columns.push({ title, dataIndex: title });
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
