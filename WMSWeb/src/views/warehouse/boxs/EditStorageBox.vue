<template>
  <BasicModal
    :title="t('编辑料箱')"
    :width="600"
    :canFullscreen="false"
    @ok="submit"
    @cancel="cancel"
    @register="registerModal"
    @visible-change="visibleChange"
    :destroyOnClose="true"
    :maskClosable="false"
  >
    <BasicForm @register="registerStorageBoxForm" />
  </BasicModal>
</template>

<script lang="ts">
  import { defineComponent } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { BasicForm, useForm } from '/@/components/Form/index';
  import { editFormSchema, updateStorageBoxAsync } from './StorageBox';
  import {  BoxDto } from '/@/services/ServiceProxies';
  import { useI18n } from '/@/hooks/web/useI18n';
  export default defineComponent({
    name: 'EditStorageBox',
    components: {
      BasicModal,
      BasicForm,
    },
    emits: ['reload'],
    setup(_, { emit }) {
      const [registerStorageBoxForm, { getFieldsValue, validate, setFieldsValue, resetFields }] =
        useForm({
          labelWidth: 120,
          schemas: editFormSchema,
          showActionButtonGroup: false,
        });
      const { t } = useI18n();
      let boxId = '';
      const [registerModal, { changeOkLoading, closeModal }] = useModalInner((data) => {
        boxId = data.record.id;
        setFieldsValue({
          boxCodeNew: data.record.boxCode,
          boxNameNew: data.record.boxName,
          boxTypeNameNew: data.record.boxTypeName,
          boxSpecsNameNew: data.record.specsName,
          boxLengthNew: data.record.length,
          boxWidthNew: data.record.width,
          boxHeightNew: data.record.height,
        });
      });

      const visibleChange = async (visible: boolean) => {
        if (visible) {
        } else {
        }
      };

      const submit = async () => {
        try {
          let request = getFieldsValue()

          await updateStorageBoxAsync({
            boxId,
            request,
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
        registerStorageBoxForm,
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
