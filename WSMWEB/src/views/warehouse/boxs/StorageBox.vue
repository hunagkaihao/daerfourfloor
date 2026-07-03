<template>
  <div>

     
        <BasicTable
          @register="registerTable"
          @selection-change="onSelectChange"
          :clickToRowSelect="false"
          size="small"
        >
          <template #toolbar>
            <a-button
              preIcon="ant-design:plus-circle-outlined"
              type="primary"
              @click="openCreateStorageBoxModal"
               v-auth="'Wms.Add'"
            >
              {{ t('common.createText') }}
            </a-button>
   
            <a-button
              preIcon="ant-design:plus-circle-outlined"
              type="primary"
              @click="openImportStorageBoxModal"
              v-auth="'WarehouseManagement.StorageBoxManagement.Create'"
            >
              {{ t('EXCEL导入') }}
            </a-button>
      <a-button  type="primary" @click="openModal"
                   >
                    {{ t('Excel导出') }}
                </a-button>
          </template>
          <template #isActive="{ record }">
            <Tag :color="record.isActive ? 'green' : 'red'">
              {{ record.isActive ? t('common.enabled') : t('common.disEnabled') }}
            </Tag>
          </template>
          <template #action="{ record }">
            <TableAction
              :actions="[
                {
                  icon: 'ic:outline-delete-outline',
                  auth: 'Wms.Delete',
                  label: t('common.delText'),
                  onClick: handleDelete.bind(null, record),
                },
              ]"
            />
          </template>
        </BasicTable>

       
        <CreateStorageBox
          @register="registerCreateStorageBoxModal"
          @reload="reload"
          :bodyStyle="{ 'padding-top': '0' }"
        /> 

        <ExpExcelModal @register="register" @success="defaultHeader" />
  </div>
</template>

<script lang="ts" setup>
  import { defineComponent, ref } from 'vue';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { BasicTable, useTable, TableAction } from '/@/components/Table';
  import {
    tableColumns,
    tableDetailColumns,
    searchFormSchema,
    getTableListAsync,
    allBoxesGet,
    getDetaiTableListAsync,
    deleteStorageBoxAsync,
    deleteStorageBoxDetailAsync,
  } from './StorageBox';
  import { useModal } from '/@/components/Modal';
  import { message } from 'ant-design-vue';
  import { jsonToSheetXlsx, ExpExcelModal, ExportModalResult } from '/@/components/Excel';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { Tag } from 'ant-design-vue';
  import CreateStorageBox from './CreateStorageBox.vue'
  import {BoxDto} from '/@/services/ServiceProxies';
  const [register, { openModal }] = useModal();
      const { createConfirm } = useMessage();
      const { t } = useI18n();
      const [registerCreateStorageBoxModal, { openModal: openCreateStorageBoxModal }] = useModal();
      const [registerCTUTaskModal, { openModal: openCTUTaskModal }] = useModal();
      const [registerCTUoutTaskModal, { openModal: openCTUoutTaskModal }] = useModal();
      const [registerImportStorageBoxModal, { openModal: openImportStorageBoxModal }] = useModal();
      const selectedBoxIdRef = ref('');
      const selectedBoxCodeRef = ref('');
      // table配置
      const [registerTable, { reload }] = useTable({
        columns: tableColumns,
        formConfig: {
          labelWidth: 70,
          schemas: searchFormSchema,
        },
        api: gettable,
        showTableSetting: true,
        useSearchForm: true,
        bordered: true,
        canResize: true,
        showIndexColumn: false,
        rowSelection: { type: 'checkbox' },
        actionColumn: {
          width: 140,
          title: t('common.action'),
          dataIndex: 'action',
          slots: {
            customRender: 'action',
          },
          fixed: 'right',
        },
      });
      async function gettable(params) {
  if (params.cellName == "") {
    params.cellName = undefined
  }
  if (params.boxCode == "") {
    params.boxCode = undefined
  }
  if (params.boxName == "") {
    params.boxName = undefined
  }
excelparam = params
  return await getTableListAsync(params)
}
      //勾选事件
      const onSelectChange = async ({ rows }) => {
        console.log(rows);
        if (rows.length > 0) {
          selectedBoxIdRef.value = rows[0].id;
          selectedBoxCodeRef.value = rows[0].storageBoxBarcode;
          // console.log(rows[0].id);
          console.log(rows[0].storageBoxBarcode);
        } else {
          selectedBoxIdRef.value = '';
          selectedBoxCodeRef.value = '';
        }

        reloadDetail();
      };

      const [registerDetailTable, { reload: reloadDetail }] = useTable({
        columns: tableDetailColumns,
        // formConfig: {
        //   labelWidth: 120,
        //   schemas: searchFormSchema,
        // },
        api: getPageDetaiTableListAsync,
        // useSearchForm: true,
        showTableSetting: true,
        showIndexColumn: true,
        indexColumnProps: {
          width: 50,
        },
        bordered: true,
        canResize: true,
        actionColumn: {
          width: 150,
          title: t('common.action'),
          dataIndex: 'action',
          slots: { customRender: 'action' },
        },
      });
      async function getPageDetaiTableListAsync(params) {
        if (selectedBoxIdRef.value == '') {
          return [];
        }
        params.storageBoxId = selectedBoxIdRef.value;
        return await getDetaiTableListAsync(params);
      }

      // 删除用户
      const handleDelete = async (record: Recordable) => {
        let msg = t('common.askDelete');
        createConfirm({
          iconType: 'warning',
          title: t('common.tip'),
          content: msg,
          onOk: async () => {
            await deleteStorageBoxAsync({ id: record.boxCode, reload });
          },
        });
      };

      //删除明细
      const handleDeleteDetail = async (record: Recordable) => {
        let msg = t('common.askDelete');
        createConfirm({
          iconType: 'warning',
          title: t('common.tip'),
          content: msg,
          onOk: async () => {
            await deleteStorageBoxDetailAsync({
              id: record.id,
              storageBoxId: selectedBoxIdRef.value,
              reloadDetail,
            });
          },
        });
      };


    
 var excelparam :{};
var data : any[] = [];
var a : BoxDto[]
async function defaultHeader({ filename, bookType }: ExportModalResult) {
        // 默认Object.keys(data[0])作为header
        try {
        a = await allBoxesGet(excelparam)
      } catch (error) {
        message.error("无法正常获取数据");
        }
        data.length = 0;
        for (let index = 0; index < a.length; index++) {
          data.push({
            容器编号:a[index].boxCode,
            容器名称:a[index].boxName,
            容器类型:a[index].boxTypeName,
            规格:a[index].specsName,
            库位名称:a[index].cellName,
            状态:a[index].status,
            库区:a[index].warehouseAreaName,
            所在仓库:a[index].warehouseName,
            宽:a[index].width,
            长:a[index].length,
            高:a[index].height,
          });
        }
        console.log(data) 
        jsonToSheetXlsx({
          data,
          filename,
          write2excelOpts: {
            bookType,
          },
        });
      }

</script>
