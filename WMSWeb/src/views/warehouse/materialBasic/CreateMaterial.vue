<template>
  <BasicModal
    :width="520"
    :title="t('routes.material.basicData_create')"
    :canFullscreen="false"
    @ok="submit"
    @cancel="cancel"
    @register="registerModal"
    @visible-change="visibleChange"
    :destroyOnClose="true"
    :maskClosable="false"
  >
    <BasicForm @register="registerForm" />
  </BasicModal>
</template>

<script lang="ts" setup>
import { BasicModal, useModalInner } from '/@/components/Modal';
import { BasicForm, useForm } from '/@/components/Form/index';
import { createFormSchema, createMaterialAsync } from './MaterialBasic';
import { useI18n } from '/@/hooks/web/useI18n';

const emit = defineEmits(['reload']);
const { t } = useI18n();
const [registerModal, { changeOkLoading, closeModal }] = useModalInner();
const [registerForm, { getFieldsValue, validate, resetFields }] = useForm({
  labelWidth: 90,
  schemas: createFormSchema,
  showActionButtonGroup: false,
});

const visibleChange = (visible: boolean) => {
  if (!visible) resetFields();
};

const submit = async () => {
  try {
    await createMaterialAsync({
      request: getFieldsValue(),
      changeOkLoading,
      validate,
      closeModal,
      resetFields,
    });
    emit('reload');
  } catch {
    changeOkLoading(false);
  }
};

const cancel = () => {
  resetFields();
  closeModal();
};
</script>
