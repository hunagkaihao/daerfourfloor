<template>
  <BasicModal
    v-bind="$attrs"
    title="创建出库任务"
    width="600px"
    @ok="handleSubmit"
  >
    <BasicForm @register="registerForm">
      <template #boxCode>
        <a-select
          v-model:value="formState.boxCode"
          placeholder="选择容器"
          style="width: 100%"
        >
          <a-select-option
            v-for="item in selectedStocks"
            :key="item.boxCode"
            :value="item.boxCode"
          >
            {{ item.boxCode }} - {{ item.materialName }}
          </a-select-option>
        </a-select>
      </template>
      <template #fromCellCode>
        <a-select
          v-model:value="formState.fromCellCode"
          placeholder="选择源库位"
          style="width: 100%"
        >
          <a-select-option
            v-for="item in selectedStocks"
            :key="item.cellCode"
            :value="item.cellCode"
          >
            {{ item.cellCode }}
          </a-select-option>
        </a-select>
      </template>
    </BasicForm>
  </BasicModal>
</template>

<script lang="ts" setup>
import { ref, computed, watch } from 'vue';
import { BasicModal, useModalInner } from '/@/components/Modal';
import { BasicForm, useForm } from '/@/components/Form';
import { FormSchema } from '/@/components/Table';
import { createOutStockTask } from './Outstock';
import { message } from 'ant-design-vue';
import { useTable } from '/@/components/Table';

const [registerForm, { setFieldsValue, getFieldsValue, validate }] = useForm({
  labelWidth: 80,
  schemas: [
    {
      field: 'boxCode',
      label: '容器',
      component: 'Input',
      required: true,
    },
    {
      field: 'fromCellCode',
      label: '源库位',
      component: 'Input',
      required: true,
    },
    {
      field: 'toCellCode',
      label: '目标库位',
      component: 'Input',
      required: true,
      placeholder: '请输入出库库位',
    },
  ],
});

const formState = ref({
  boxCode: '',
  fromCellCode: '',
  toCellCode: '',
});

const [register, { closeModal }] = useModalInner((data) => {
  // 从父组件获取选中的库存数据
  if (data && data.selectedStocks) {
    selectedStocks.value = data.selectedStocks;
    // 自动设置第一个库存的容器和库位
    if (data.selectedStocks.length > 0) {
      formState.value.boxCode = data.selectedStocks[0].boxCode;
      formState.value.fromCellCode = data.selectedStocks[0].cellCode;
    }
  }
});

const selectedStocks = ref([]);

const emit = defineEmits(['success']);

const handleSubmit = async () => {
  const values = await validate();
  if (!values) return;

  try {
    const res = await createOutStockTask(
      values.boxCode,
      values.fromCellCode,
      values.toCellCode
    );

    if (res.success) {
      message.success('出库任务创建成功');
      emit('success');
      closeModal();
    } else {
      message.error(res.message || '创建任务失败');
    }
  } catch (error) {
    message.error('创建任务失败');
  }
};
</script>

<style scoped lang="less">
</style>
