<template>
    <BasicModal :width="600" :title="t('编辑仓库')" :canFullscreen="false"
        @ok="submit" @cancel="cancel" @register="registerModal" @visible-change="visibleChange" :destroyOnClose="true"
        :maskClosable="false">
        <BasicForm @register="registerCellForm" />
    </BasicModal>
</template>

<script lang="ts" setup>
import { BasicModal, useModalInner } from '/@/components/Modal';
import { BasicForm, useForm } from '/@/components/Form/index';
import { EditWareFormSchema, updateWareAsync } from './Ware';
import { CellAddDto } from '/@/services/ServiceProxies';
import { useI18n } from '/@/hooks/web/useI18n';


const emit = defineEmits(["reload"]);  
const { t } = useI18n();
var id = '1';
const [registerModal, { changeOkLoading, closeModal }] = useModalInner((data) => {
        id = data.record.id,
        setFieldsValue({
            warehouseCodeNew: data.record.warehouseCode,
            warehouseNameNew: data.record.warehouseName,
            warehouseTypeNew: data.record.warehouseType,
            warehouseRemarkNew: data.record.warehouseRemark,
            
        });
      });
const [registerCellForm, { getFieldsValue, validate, setFieldsValue, resetFields }] = useForm({
    labelWidth: 120,
    schemas: EditWareFormSchema,
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
        await updateWareAsync({
            id,
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
.ant-checkbox-wrapper+.ant-checkbox-wrapper {
    margin-left: 0px;
}
</style>