<template>
  <div>

    <BasicTable @register="registerTable"
    @selection-change="onSelectChange" size="small">
      <template #toolbar>
        <a-button preIcon="ant-design:plus-circle-outlined" type="primary" @click="openCreateWareModal"
        v-auth="'Wms.Add'"
          >
          {{ t('common.createText') }}
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
        label: t('common.editText'),
        auth: 'Wms.Edit',
        onClick: handleEdit.bind(null, record),
      },
    ]" :dropDownActions="[
      {
        label: t('创建库区'),
        auth: 'Wms.Add',
        onClick: handleAddArea.bind(null, record),
      },
      {
        label: t('common.delText'),
        auth: 'Wms.Delete',
        onClick: handleDelete.bind(null, record),
      },
    ]" />
      </template>
    </BasicTable>

  <div style="margin:0px 16px;">
    <BasicTable @register="registerAreaTable"  size="small">
     
      <template #isActive="{ record }">
        <Tag :color="record.isActive ? 'green' : 'red'">
          {{ record.isActive ? t('common.enabled') : t('common.disEnabled') }}
        </Tag>
      </template>
      <template #action="{ record }">
        <TableAction :actions="[
      {
        icon: 'ant-design:edit-outlined',
        label: t('common.editText'),
        auth: 'Wms.Edit',
        onClick: handleEditArea.bind(null, record),
      },
    ]" :dropDownActions="[
      {
        label: t('common.delText'),
        auth: 'Wms.Delete',
        onClick: handleDeleteArea.bind(null, record),
      },
      
    ]" />
      </template>
    </BasicTable>
  </div>
    <CreateWare @register="registerCreateWareModal" @reload="reload"></CreateWare>
    <EditWare @register="registerEditWareModal" @reload="reload"></EditWare>
    <CreateArea  @register="registerCreateAreaModal" @reload="reloadarea"></CreateArea>
    <EditArea @register="registerEditAreaModal" @reload="reloadarea"></EditArea>
  </div>
</template>

<script lang="ts" setup>
import { defineComponent } from 'vue';
import { useMessage } from '/@/hooks/web/useMessage';
import { BasicTable, useTable, TableAction } from '/@/components/Table';
import { WareColumns,AreaColumns, WaresearchFormSchema, getWareListAsync,getAreaListAsync,deleteAreaAsync, deleteWareAsync,createAreaAsync } from './Ware';
import { useModal } from '/@/components/Modal';
import CreateWare from './CreateWare.vue';
import EditWare from './EditWare.vue';
import CreateArea from './CreateArea.vue'
import EditArea from './EditArea.vue'
import { message } from 'ant-design-vue';
import { useI18n } from '/@/hooks/web/useI18n';
import { Tag } from 'ant-design-vue';

    const { createConfirm } = useMessage();
    const { t } = useI18n();
    var wareCode = "1";
    const [registerCreateWareModal, { openModal: openCreateWareModal }] = useModal();
    const [registerEditWareModal, { openModal: openEditWareModal }] = useModal();
    const [registerCreateAreaModal, { openModal: openCreateAreaModal }] = useModal();
    const [registerEditAreaModal, { openModal: openEditAreaModal }] = useModal();
    // table配置
    const [registerTable, { reload }] = useTable({
      columns: WareColumns,
      formConfig: {
        labelWidth: 70,
        schemas: WaresearchFormSchema,
      },
      api: getWareListAsync,
      showTableSetting: true,
      useSearchForm: true,
      bordered: true,
      showIndexColumn: true,
      canResize: true,
      rowSelection: { type: 'radio' },
      maxHeight: 300,
      actionColumn: {
        width: 120,
        title: t('common.action'),
        dataIndex: 'action',
        slots: {
          customRender: 'action',
        },
        fixed: 'right',
      },
    });

    const [registerAreaTable, { reload:reloadarea }] = useTable({
      columns: AreaColumns,
      api: getAreaListAsync,
      beforeFetch: (data) => {
        data = wareCode
        return data;
      },
      showTableSetting: true,
      useSearchForm: false,
      bordered: true,
      canResize: true,
      showIndexColumn: true,
      actionColumn: {
        width: 120,
        title: t('common.action'),
        dataIndex: 'action',
        slots: {
          customRender: 'action',
        },
        fixed: 'right',
      },
    });

    //勾选事件
    const onSelectChange = async ({ rows }) => {
        console.log(rows);
        if (rows.length > 0) {
          console.log( rows[0]);
          wareCode = rows[0].id;

          console.log(wareCode);

        } else {
          wareCode = "1";

        }

        reloadarea();
      };
    // 编辑
    const handleEdit = (record: Recordable) => {
      openEditWareModal(true, {
        record: record,
      });
    };

    // 删除
    const handleDelete = async (record: Recordable) => {
      if (record.name == 'admin') {
        message.error('admin not delete');
        return;
      } else {
        let msg = t('common.askDelete');
        createConfirm({
          iconType: 'warning',
          title: t('common.tip'),
          content: msg,
          onOk: async () => {
            await deleteWareAsync({ id: record.id, reload });
          },
        });
      }
    };
    const handleAddArea = (record: Recordable) => {
      openCreateAreaModal(true, {
        record: record,
      });
    };
 // 删除
 const handleDeleteArea = async (record: Recordable) => {
      
        let msg = t('common.askDelete');
        createConfirm({
          iconType: 'warning',
          title: t('common.tip'),
          content: msg,
          onOk: async () => {
            await deleteAreaAsync({  areaIdToDel : record.id, reloadarea });
          },
        });
      }
   // 编辑
   const handleEditArea = (record: Recordable) => {
      openEditAreaModal(true, {
        record: record,
      });
    };
</script>