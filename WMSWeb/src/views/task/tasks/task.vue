<template>
  <div>
    <BasicTable @register="registerTable" :clickToRowSelect="false" size="small">
      <template #toolbar>
        <a-button preIcon="ant-design:plus-circle-outlined" type="primary" @click="openCreateCellModal">
          {{ t('common.createText') }}
        </a-button>
        <a-button  type="primary" @click="openModal">
                    {{ t('Excel导出') }}
                </a-button>
      </template>
      <template #isActive="{ record }">
        <Tag :color="record.isActive ? 'green' : 'red'">
          {{ record.isActive ? t('common.enabled') : t('common.disEnabled') }}
        </Tag>
      </template>
      <template #action="{ record }">
                <TableAction :actions="[
            {
                icon: 'ant-design:edit-outlined',
                auth: 'Wms.Edit',
                label: t('common.editText'),
                onClick: handleEdit.bind(null, record),
            },
            {
              icon: 'ic:outline-delete-outline',
                auth: 'Wms.Delete',
                label: t('common.delText'),
                onClick: handleDelete.bind(null, record),
            },
        ]" />
            </template>
    </BasicTable>

    <Createnoplan @register="registerCreateCellModal" @reload="reload" :bodyStyle="{ 'padding-top': '0' }" />
 <ExpExcelModal @register="register" @success="defaultHeader" />

    <Editnoplan @register="registerEditnoplanModal" @reload="reload" :bodyStyle="{ 'padding-top': '0' }" />
  </div>
</template>

<script lang="ts" setup>
import { useMessage } from '/@/hooks/web/useMessage';
import { BasicTable, useTable, TableAction } from '/@/components/Table';
import {
  tableColumns,searchFormSchema
} from './task';
import { jsonToSheetXlsx, ExpExcelModal, ExportModalResult } from '/@/components/Excel';

import { useModal } from '/@/components/Modal';
import { useI18n } from '/@/hooks/web/useI18n';
import Createnoplan from './Createnoplan.vue';
import Editnoplan from './Editnoplan.vue'
import { message } from 'ant-design-vue';
import {PickItemDto} from '/@/services/ServiceProxies';
const [register, { openModal }] = useModal();
const [registerCreateCellModal, { openModal: openCreateCellModal }] = useModal();
const [registerEditnoplanModal, { openModal: openEditnoplanModal }] = useModal();
const { createConfirm } = useMessage();
const { t } = useI18n();
// table配置
const [registerTable, { reload }] = useTable({
  columns: tableColumns,
  formConfig: {
    labelWidth: 70,
    schemas: searchFormSchema,
  },
  // api: getTable, // 移除API调用
  dataSource: [], // 使用静态数据
  showTableSetting: true,
  useSearchForm: true,
  bordered: true,
  canResize: true,
  showIndexColumn: false,
  rowSelection: { type: 'checkbox' },
  actionColumn: {
          width: 150,
          title: t('common.action'),
          dataIndex: 'action',
          slots: { customRender: 'action' },
        },
});
// 移除getTable函数，不再需要接口调用
// 编辑用户
const handleEdit = (record: Recordable) => {
    // 移除接口调用，只显示提示信息
    message.info('编辑功能已禁用，请等待后续更新');
    console.log('编辑功能已禁用，选中记录:', record);
      };

      // 删除用户
      const handleDelete = async (record: Recordable) => {
        // 移除接口调用，只显示提示信息
        message.info('删除功能已禁用，请等待后续更新');
        console.log('删除功能已禁用，选中记录:', record);
      };
var excelparam :{};
var data : any[] = [];
var a : PickItemDto[]
async function defaultHeader({ filename, bookType }: ExportModalResult) {
        // 移除接口调用，显示提示信息
        message.info('Excel导出功能已禁用，请等待后续更新');
        console.log('Excel导出功能已禁用');
      }
</script>
./IncellHis