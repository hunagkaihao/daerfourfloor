<template>
    <BasicModal :width="600" :title="t('创建库区')" :canFullscreen="false"
        @ok="submit" @cancel="cancel" @register="registerModal" @visible-change="visibleChange" :destroyOnClose="true"
        :maskClosable="false">
        <BasicForm @register="registerCellForm" />
    </BasicModal>
</template>

<script lang="ts" setup>
import { BasicModal, useModalInner } from '/@/components/Modal';
import { BasicForm, useForm } from '/@/components/Form/index';
import { createAreaFormSchema, createAreaAsync } from './Ware';
import { CellAddDto } from '/@/services/ServiceProxies';
import { useI18n } from '/@/hooks/web/useI18n';


const emit = defineEmits(["reloadarea"]);  
var warehouseCode = ''
const { t } = useI18n();
const [registerModal, { changeOkLoading, closeModal }] = useModalInner((data)=>{
    console.log(data.record.warehouseCode)
    warehouseCode = data.record.id
});
const [registerCellForm, { getFieldsValue, validate, resetFields, }] = useForm({
    labelWidth: 120,
    schemas: createAreaFormSchema,
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
        let request = getFieldsValue();
        await createAreaAsync({
            warehouseCode,
            request,
            changeOkLoading,
            validate,
            closeModal,
            resetFields,
        });
        emit('reloadarea');
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