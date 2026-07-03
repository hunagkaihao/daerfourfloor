<template>
  <BasicModal
    :width="600"
    :title="t('新增容器')"
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

<script lang="ts" setup>
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { BasicForm, useForm } from '/@/components/Form/index';
  import { createFormSchema, createStorageBoxAsync } from './StorageBox';
  import {  } from '/@/services/ServiceProxies';
  import { useI18n } from '/@/hooks/web/useI18n';


      const emit = defineEmits(["reload"]);  
      const { t } = useI18n();
      const [registerModal, { changeOkLoading, closeModal }] = useModalInner();
      const [registerStorageBoxForm, { getFieldsValue, validate, resetFields }] = useForm({
        labelWidth: 120,
        schemas: createFormSchema,
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
          let request = getFieldsValue() ;
          // 设置容器名称等于容器编号
          request.boxName = request.boxCode;
          await createStorageBoxAsync({
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
     
</script>
<style lang="less" scoped>
  .ant-checkbox-wrapper + .ant-checkbox-wrapper {
    margin-left: 0px;
  }
</style>
