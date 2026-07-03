<template>
    <BasicModal
      :width="600"
      :title="t('编辑无计划领用')"
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
    import { editFormSchema } from './task';
    import { NoPlanPickOutCreateDto } from '/@/services/ServiceProxies';
    import { useI18n } from '/@/hooks/web/useI18n';
import { message } from 'ant-design-vue';
    export default defineComponent({
      name: 'Createnoplan',
      components: {
        BasicModal,
        BasicForm,
      },
      emits: ['reload'],
      setup(_, { emit }) {
        var uniqueCodeToEdit = '' 
        var noPlanPickListIdToEdit= ''
        const { t } = useI18n();
        const [registerModal, { changeOkLoading, closeModal }] = useModalInner((data)=>{
            uniqueCodeToEdit = data.record.uniqueCode
            noPlanPickListIdToEdit = data.record.pickListId
            setFieldsValue({
                materialCode:data.record.materialCode,
                departmentId:data.record.deptName,
                newPickCount:data.record.countToPick,
                newPickType:data.record.pickTypeNo,
                newPickerName:data.record.pickManName,
            })
        });
        const [registerCellForm, { getFieldsValue, validate,setFieldsValue, resetFields }] = useForm({
          labelWidth: 120,
          schemas: editFormSchema,
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
            request.uniqueCodeToEdit = uniqueCodeToEdit
            request.noPlanPickListIdToEdit = noPlanPickListIdToEdit
            
            // 移除接口调用，只显示提示信息
            message.info('编辑无计划领用功能已禁用，请等待后续更新');
            console.log('编辑无计划领用功能已禁用，表单数据:', request);
            
            // 关闭弹窗但不刷新数据
            closeModal();
          } catch (err){
            changeOkLoading(false);
          }
        };
        const cancel = () => {
          resetFields();
          closeModal();
        };
        return {
          t,
          cancel,
          registerModal,
          registerCellForm,
          submit,
          visibleChange,
        };
      },
    });
  </script>
  <style lang="less" scoped>
    .ant-checkbox-wrapper + .ant-checkbox-wrapper {
      margin-left: 0px;
    }
  </style>
  