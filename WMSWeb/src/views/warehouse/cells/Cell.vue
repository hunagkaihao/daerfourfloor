<template>
    <div>
        <BasicTable @register="registerTable" size="small" >
            <template #toolbar>
                <a-button preIcon="ant-design:plus-circle-outlined" type="primary" @click="openCreateCellModal" 
                
                >
                    {{ t('common.createText') }}
                </a-button>
                <!-- 绑定库区和解绑库区按钮已移除 -->
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
                <TableAction :actions="[]" :dropDownActions="[
            {
                auth: 'Wms.Delete',
                label: t('common.delText'),
                onClick: handleDelete.bind(null, record),
            },
        ]" />
            </template>
        </BasicTable>
        <CreateCell @register="registerCreateCellModal" @reload="reload" :bodyStyle="{ 'padding-top': '0' }" />
        <ImportCells @register="registerImportGoodssModal" @reload="reload" :bodyStyle="{ 'padding-top': '0' }" />
        <!-- BindArea组件已删除 -->
        <ExpExcelModal @register="register" @success="defaultHeader" />
    </div>
</template>

<script lang="ts" setup>
import { defineComponent } from 'vue';
import { useMessage } from '/@/hooks/web/useMessage';
import { BasicTable, useTable, TableAction } from '/@/components/Table';
import { tableColumns, searchFormSchema, getTableListAsync, deleteCellAsync,allCellsAndMaterialAreaGet, disableCellAsync, enableCellAsync} from './Cell';
import { useModal } from '/@/components/Modal';
import CreateCell from './CreateCell.vue';
import { jsonToSheetXlsx, ExpExcelModal, ExportModalResult } from '/@/components/Excel';
// BindArea组件已删除
import { message } from 'ant-design-vue';
import { useI18n } from '/@/hooks/web/useI18n';
import { Tag } from 'ant-design-vue';
import ImportCells from './ExcelImport.vue';
import {CellDto} from '/@/services/ServiceProxies';
const [register, { openModal }] = useModal();
defineComponent({
    name: 'Cell'
})

const { createConfirm } = useMessage();
const { t } = useI18n();
const [registerCreateCellModal, { openModal: openCreateCellModal }] = useModal();

const [registerImportGoodssModal, { openModal: openImportGoodssModal }] = useModal();
// registerBindAreaModal已删除
// table配置
const [registerTable, { reload , getRowSelection,getSelectRows ,clearSelectedRowKeys}] = useTable({
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
    showIndexColumn: true,
    rowSelection: { type: 'checkbox' },
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

// 删除用户
const handleDelete = async (record: Recordable) => {

        let msg = t('common.askDelete');
        createConfirm({
            iconType: 'warning',
            title: t('common.tip'),
            content: msg,
            onOk: async () => {
                await deleteCellAsync({ id: record.id, reload });
            },
        });
    
};

// 禁用库位
const handleDisable = async (record: Recordable) => {
    let msg = `确定要禁用库位 ${record.cellCode} 吗？`;
    createConfirm({
        iconType: 'warning',
        title: t('common.tip'),
        content: msg,
        onOk: async () => {
            await disableCellAsync(record.cellCode, reload);
        },
    });
};

// 启用库位
const handleEnable = async (record: Recordable) => {
    let msg = `确定要启用库位 ${record.cellCode} 吗？`;
    createConfirm({
        iconType: 'warning',
        title: t('common.tip'),
        content: msg,
        onOk: async () => {
            await enableCellAsync(record.cellCode, reload);
        },
    });
};
// bindArea和unbindArea函数已删除
async function gettable(params){ 
   excelparam = params
  //console.log(a)
  return await getTableListAsync(params)
}
var excelparam :{};
var data : any[] = [];
var a : CellDto[]
async function defaultHeader({ filename, bookType }: ExportModalResult) {
        // 默认Object.keys(data[0])作为header
        try{
        a = await allCellsAndMaterialAreaGet(excelparam)
        }catch (error) {
          message.error("无法正常获取数据");
        }
        data.length = 0;
        for (let index = 0; index < a.length; index++) {
          data.push({
            库位编码:a[index].cellCode,
            库位名称:a[index].cellName,
            库位类型:a[index].cellType,
            所属仓库:a[index].warehouseName,
            仓库区域:a[index].warehouseAreaName,
            区域编码:a[index].availableBoxSpecsNames,
            物料区域:a[index].materialArea,
            库位状态:a[index].cellStatus,
            运行状态:a[index].runStatus,
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