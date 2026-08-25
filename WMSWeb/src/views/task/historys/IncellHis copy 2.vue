<template>
  <div>
    <BasicTable @register="registerTable" :clickToRowSelect="false" size="small">
<template #toolbar>

        <a-button
          type="primary"
          @click="openModal"
        >
          {{ t('Excel导出') }}
        </a-button>
      </template>
      <template #isActive="{ record }">
        <Tag :color="record.isActive ? 'green' : 'red'">
          {{ record.isActive ? t('common.enabled') : t('common.disEnabled') }}
        </Tag>
      </template>

    </BasicTable>


<ExpExcelModal @register="register" @success="defaultHeader" />

  </div>
</template>

<script lang="ts" setup>
import { useMessage } from '/@/hooks/web/useMessage';
import { BasicTable, useTable, TableAction } from '/@/components/Table';
import { useModal } from '/@/components/Modal';
import { jsonToSheetXlsx,aoaToSheetXlsx, ExpExcelModal, ExportModalResult } from '/@/components/Excel';
import {StockInHistoryDto}from '/@/services/ServiceProxies';
import {
  tableColumns,
  searchFormSchema,
  getTableListAsync,
  allStockInHistoriesGet
} from './IncellHis';
import { useI18n } from '/@/hooks/web/useI18n';
const [register, { openModal }] = useModal();
const { createConfirm } = useMessage();
const { t } = useI18n();
// table配置
const [registerTable, { reload }] = useTable({
  columns: tableColumns,
  formConfig: {
    labelWidth: 70,
    schemas: searchFormSchema,
    fieldMapToTime: [['time', ['stockInTimeStart', 'stockInTimeEnd'], 'YYYY-MM-DD HH:mm:ss']],
  },
  api: gettable,
  showTableSetting: true,
  useSearchForm: true,
  bordered: true,
  canResize: true,
  showIndexColumn: false,
  rowSelection: { type: 'checkbox' },
});

async function gettable(params) {
  if (params.boxCode == "") {
    params.boxCode = undefined
  }
  if (params.barcode == "") {
    params.barcode = undefined
  }
  if (params.materialCode == "") {
    params.materialCode = undefined
  }
  if (params.materialNameTip == "") {
    params.materialNameTip = undefined
  }
  if (params.materialSpecsTip == "") {
    params.materialSpecsTip = undefined
  }
  if (params.stockInType == "") {
    params.stockInType = undefined
  }
  if (params.checkNoTip == "") {
    params.checkNoTip = undefined
  }
  a = await allStockInHistoriesGet(params)
  return await getTableListAsync(params)
}
var data : any[] = [];
var a : StockInHistoryDto[]

function defaultHeader({ filename, bookType }: ExportModalResult) {
        // 默认Object.keys(data[0])作为header
        data.length = 0;
        const arrHeader = tableColumns.map((column) => column.title);
        const arrData = a.map((item) => {
          return Object.keys(item).map((key) => item[key]);
        });
      console.log(arrHeader) 
        console.log(arrData) 
        aoaToSheetXlsx({
          data: arrData,
          header: arrHeader,
          filename: '二维数组方式导出excel.xlsx',
        });
        
        console.log(data) 
        // jsonToSheetXlsx({
        //   data,
        //   filename,
        //   write2excelOpts: {
        //     bookType,
        //   },
        // });
      }
</script>
