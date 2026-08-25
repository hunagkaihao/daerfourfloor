<template>
  <div>
    <BasicTable @register="registerTable" :clickToRowSelect="false" size="small">
      <template #isCheckOut="{ record }">
        <Tag :color="record.isCheckOut === '1' ? 'green' : 'red'">
          {{ record.isCheckOut === '1' ? '已抽检' : '未抽检' }}
        </Tag>
      </template>
    </BasicTable>
    
    <!-- 修改已绑定数量弹窗 - 完全按照料车管理的方式 -->
    <BasicModal
      @register="registerEditModal"
      :title="'修改已绑定数量'"
      @ok="handleConfirmModify"
      :okButtonProps="{ loading: modifyLoading }"
      :width="800"
    >
      <BasicForm @register="registerEditForm" />
    </BasicModal>
  </div>
</template>

<script lang="ts" setup>
import { onMounted, ref, nextTick } from 'vue';
import { BasicTable, useTable } from '/@/components/Table';
import {
  tableColumns,
  searchFormSchema
} from './BarcodeList';
import { useI18n } from '/@/hooks/web/useI18n';
import { useModal, BasicModal } from '/@/components/Modal';
import { BasicForm, useForm } from '/@/components/Form';
import { Tag } from 'ant-design-vue';
import { useMessage } from '/@/hooks/web/useMessage';
import { BarcodeListServiceProxy } from '/@/services/ServiceProxies';
import moment from 'moment';

// 移除Excel导出相关代码
const { t } = useI18n();
const { createMessage } = useMessage();
const barcodeListService = new BarcodeListServiceProxy();

// 修改已绑定数量相关状态
const modifyLoading = ref(false);

// 弹窗配置 - 优化布局，字段分三行显示
const [registerEditModal, { openModal: openEditModal, closeModal: closeEditModal }] = useModal();
const [registerEditForm, { getFieldsValue, setFieldsValue, validate }] = useForm({
  labelWidth: 120,
  showActionButtonGroup: false,
  schemas: [
    // 第一行：收料码 + 物料编号
    {
      field: 'barcode',
      label: '收料码',
      component: 'Input',
      componentProps: {
        disabled: true,
      },
      colProps: { span: 12 },
    },
    {
      field: 'materialCode',
      label: '物料编号',
      component: 'Input',
      componentProps: {
        disabled: true,
      },
      colProps: { span: 12 },
    },
    // 第二行：物料名称 + 总收料数量
    {
      field: 'materialName',
      label: '物料名称',
      component: 'Input',
      componentProps: {
        disabled: true,
      },
      colProps: { span: 12 },
    },
    {
      field: 'receiveTotalCount',
      label: '总收料数量',
      component: 'InputNumber',
      componentProps: {
        disabled: true,
      },
      colProps: { span: 12 },
    },
    // 第三行：当前已绑定数量 + 新的已绑定数量
    {
      field: 'currentInBindCount',
      label: '当前已绑定数量',
      component: 'InputNumber',
      componentProps: {
        disabled: true,
      },
      colProps: { span: 12 },
    },
    {
      field: 'newInBindCount',
      label: '新的已绑定数量',
      component: 'InputNumber',
      componentProps: {
        min: 0,
        precision: 6,
        placeholder: '请输入新的已绑定数量',
      },
      colProps: { span: 12 },
    },
  ],
});

// table配置
const [registerTable, {getDataSource, reload,getSelectRows,clearSelectedRowKeys, getForm}] = useTable({
  columns: tableColumns,
  formConfig: {
    labelWidth: 70,
    schemas: searchFormSchema,
    // 自定义重置函数，确保重置后收料日期恢复为最近7天，并自动查询
    resetFunc: () => {
      const form = getForm();
      form?.setFieldsValue({
        slDateRange: [moment().subtract(7, 'days'), moment()],
      });
      // 重置后自动触发查询
      nextTick(() => {
        reload();
      });
    },
  },
  // api: gettable, // 移除API调用
  dataSource: [], // 使用静态数据
  showTableSetting: true,
  useSearchForm: true,
  bordered: true,
  canResize: true,
  rowKey: 'id',
  showIndexColumn: false,
  rowSelection: { type: 'checkbox' },
  // 禁用自动查询，避免页面加载时调用两次接口
  immediate: false,
});

// 移除不再使用的data变量

// 移除gettable函数，不再需要接口调用


// 移除Excel导出功能


// 修改已绑定数量 - 移除接口调用
function handleEdit(record: any) {
  console.log('修改已绑定数量功能已禁用，选中记录:', record);
  // 移除接口调用，只显示提示信息
  createMessage.info('修改已绑定数量功能已禁用，请等待后续更新');
}

// 确认修改 - 移除接口调用
async function handleConfirmModify() {
  // 移除接口调用，只显示提示信息
  createMessage.info('修改已绑定数量功能已禁用，请等待后续更新');
  console.log('修改已绑定数量功能已禁用');
}

// 声明全局函数类型
declare global {
  interface Window {
    handleBarcodeEdit?: (record: any) => void;
  }
}

onMounted(() => {
  // 先设置表单的默认值，然后触发查询
  const form = getForm();
  if (form) {
    form.setFieldsValue({
      slDateRange: [moment().subtract(7, 'days'), moment()],
    });
    
    // 使用 nextTick 确保表单值设置完成后再触发查询
    nextTick(() => {
      reload();
    });
  } else {
    // 如果表单还没有准备好，直接触发查询
    reload();
  }
  // 暴露全局函数供表格调用
  window.handleBarcodeEdit = handleEdit;
});
</script>

<style scoped>
/* 自定义样式 */
</style>
