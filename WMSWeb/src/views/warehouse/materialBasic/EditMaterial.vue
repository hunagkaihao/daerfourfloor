<template>
  <BasicModal
    :width="520"
    :title="t('routes.material.basicData_edit')"
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
import { editFormSchema, updateMaterialAsync } from './MaterialBasic';
import { MaterialDto } from '/@/services/ServiceProxies';
import { useI18n } from '/@/hooks/web/useI18n';

const emit = defineEmits(['reload']);
const { t } = useI18n();
let materialId = '';
let materialRecord: MaterialDto | null = null;

const [registerModal, { changeOkLoading, closeModal }] = useModalInner((data) => {
  materialId = data.record.id;
  materialRecord = data.record;
  setFieldsValue({
    materialCodeNew: data.record.materialCode,
    materialNameNew: data.record.materialName,
    specsNew: data.record.specs,
    unitNew: data.record.unit,
  });
});

const [registerForm, { getFieldsValue, validate, setFieldsValue, resetFields }] = useForm({
  labelWidth: 90,
  schemas: editFormSchema,
  showActionButtonGroup: false,
});

const visibleChange = (visible: boolean) => {
  if (!visible) resetFields();
};

const submit = async () => {
  if (!materialRecord) return;
  try {
    await updateMaterialAsync({
      id: materialId,
      record: materialRecord,
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
