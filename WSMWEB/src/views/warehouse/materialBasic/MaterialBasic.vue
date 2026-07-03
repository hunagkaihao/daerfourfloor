<template>
  <div>
    <BasicTable @register="registerTable" size="small">
      <template #toolbar>
        <a-button preIcon="ant-design:plus-circle-outlined" type="primary" @click="openCreateModal">
          {{ t('common.createText') }}
        </a-button>
      </template>
      <template #action="{ record }">
        <TableAction
          :actions="[
            {
              icon: 'ant-design:edit-outlined',
              label: t('common.editText'),
              onClick: handleEdit.bind(null, record),
            },
            {
              icon: 'ant-design:delete-outlined',
              label: t('common.delText'),
              color: 'error',
              onClick: handleDelete.bind(null, record),
            },
          ]"
        />
      </template>
    </BasicTable>

    <CreateMaterial @register="registerCreateModal" @reload="reload" />
    <EditMaterial @register="registerEditModal" @reload="reload" />
  </div>
</template>

<script lang="ts" setup>
import { BasicTable, useTable, TableAction } from '/@/components/Table';
import { useModal } from '/@/components/Modal';
import { useMessage } from '/@/hooks/web/useMessage';
import { useI18n } from '/@/hooks/web/useI18n';
import { tableColumns, searchFormSchema, getMaterialListAsync, deleteMaterialAsync } from './MaterialBasic';
import CreateMaterial from './CreateMaterial.vue';
import EditMaterial from './EditMaterial.vue';

const { t } = useI18n();
const { createConfirm } = useMessage();
const [registerCreateModal, { openModal: openCreateModal }] = useModal();
const [registerEditModal, { openModal: openEditModal }] = useModal();

const [registerTable, { reload }] = useTable({
  columns: tableColumns,
  formConfig: {
    labelWidth: 80,
    schemas: searchFormSchema,
    autoSubmitOnEnter: true,
  },
  api: getMaterialListAsync,
  showTableSetting: true,
  useSearchForm: true,
  bordered: true,
  canResize: true,
  showIndexColumn: true,
  rowKey: 'id',
  actionColumn: {
    width: 160,
    title: t('common.action'),
    dataIndex: 'action',
    slots: { customRender: 'action' },
    fixed: 'right',
  },
});

function handleEdit(record) {
  openEditModal(true, { record });
}

function handleDelete(record) {
  createConfirm({
    iconType: 'warning',
    title: t('common.tip'),
    content: `确定删除物料「${record.materialCode}」吗？`,
    onOk: async () => {
      await deleteMaterialAsync({ materialCode: record.materialCode, reload });
    },
  });
}
</script>
