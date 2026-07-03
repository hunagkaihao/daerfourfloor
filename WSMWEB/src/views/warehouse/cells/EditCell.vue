<template>
  <BasicModal
    :title="t('routes.warehouse.cellManagement_edit_cell')"
    :width="600"
    :canFullscreen="false"
    @ok="submit"
    @cancel="cancel"
    @register="registerModal"
    @visible-change="visibleChange"
    :destroyOnClose="true"
    :maskClosable="false"
  >
    <BasicForm @register="registerCellForm" />
  </BasicModal>
</template>

<script lang="ts">
  import { defineComponent } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { BasicForm, useForm } from '/@/components/Form/index';
  import { editFormSchema, updateCellAsync } from './Cell';
  import {  CellDto } from '/@/services/ServiceProxies';
  import { useI18n } from '/@/hooks/web/useI18n';
  export default defineComponent({
    name: 'EditCell',
    components: {
      BasicModal,
      BasicForm,
    },
    emits: ['reload'],
    setup(_, { emit }) {
      const [registerCellForm, { getFieldsValue, validate, setFieldsValue, resetFields }] = useForm(
        {
          labelWidth: 120,
          schemas: editFormSchema,
          showActionButtonGroup: false,
        }
      );
      const { t } = useI18n();
      let currentCellInfo = new CellDto();
      const [registerModal, { changeOkLoading, closeModal }] = useModalInner((data) => {
        currentCellInfo = data.record;
        setFieldsValue({
          warehouseName: data.record.warehouseName,
          warehouseAreaName: data.record.warehouseAreaName,
          shelfName: data.record.shelfName,
          cellCode: data.record.cellCode,
          cellName: data.record.cellName,
          availableBoxSpecsNames: data.record.availableBoxSpecsNames,
          cellType: data.record.cellType,
          materialArea:data.record.materialArea
        });
      });

      const visibleChange = async (visible: boolean) => {
        if (visible) {
        } else {
        }
      };

      const submit = async () => {
        try {
          let request = getFieldsValue();
          // let updateCellInput = new UpdateCellInput();
          request.id = currentCellInfo.id;
          // request.publishDate = request.publishDate.format();
          await updateCellAsync({
            request: request,
            changeOkLoading,
            validate,
            closeModal,
            resetFields,
          });
          emit('reload');
        } catch (error) {
          changeOkLoading(false);
        }
      };
      const cancel = () => {
        resetFields();
        closeModal();
      };

      return {
        registerModal,
        registerCellForm,
        submit,
        visibleChange,
        cancel,
        t,
      };
    },
  });
</script>
<style lang="less" scoped>
  .ant-checkbox-wrapper + .ant-checkbox-wrapper {
    margin-left: 0px;
  }
</style>
