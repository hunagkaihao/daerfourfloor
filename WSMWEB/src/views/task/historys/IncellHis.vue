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


<ExpExcelModal @register="register" @success="handleExport" />

  </div>
</template>

<script lang="ts" setup>
import { useMessage } from '/@/hooks/web/useMessage';
import { BasicTable, useTable, TableAction } from '/@/components/Table';
import { useModal } from '/@/components/Modal';
import { jsonToSheetXlsx, ExpExcelModal, ExportModalResult } from '/@/components/Excel';
import {StockInHistoryDto}from '/@/services/ServiceProxies';
import moment from 'moment';
import {
  tableColumns,
  searchFormSchema,
  getTableListAsync
} from './IncellHis';
import { useI18n } from '/@/hooks/web/useI18n';
const [register, { openModal }] = useModal();
const { createConfirm, message } = useMessage();
const { t } = useI18n();
// table配置
const [registerTable, { reload, getSelectRows }] = useTable({
  columns: tableColumns,
  formConfig: {
    labelWidth: 70,
    schemas: searchFormSchema,
    fieldMapToTime: [['time', ['stockInTimeStart', 'stockInTimeEnd'], 'YYYY-MM-DD HH:mm:ss']],
  },
  api: getTableListAsync,
  showTableSetting: true,
  useSearchForm: true,
  bordered: true,
  canResize: true,
  showIndexColumn: false,
  rowSelection: { type: 'checkbox' },
});

function handleExport({ filename, bookType }: ExportModalResult) {
  const selectRows = getSelectRows();
  const exportData = selectRows.map((item) => ({
    物料编号: item.materialCode,
    物料名称: item.materialName,
    规格: item.materialSpecs,
    数量: item.inCount,
    单位: item.materialUnit,
    生产批号: item.batchNo,
    入库时间: moment(item.inTime).format('YYYY-MM-DD HH:mm:ss'),
    收料码: item.barcode,
    容器编号: item.boxCode,
    容器名称: item.boxName,
    库位编号: item.cellCode,
    库位名称: item.cellName,
    区域编号: item.areaCode,
    区域名称: item.areaName,
    仓库编号: item.warehouseCode,
    仓库名称: item.warehouseName,
    入库类型: item.stockInType,
    环保要求: item.isHB ? '是' : '否',
    保质期: item.expiryDate ? moment(item.expiryDate).format('YYYY-MM-DD') : '',
    备料单: item.blCode,
    备货单号: item.bhCode,
    操作者: item.operatorName,
  }));
  if (exportData.length === 0) {
    message.warning('请先选择要导出的数据');
    return;
  }
  jsonToSheetXlsx({
    data: exportData,
    filename: filename || '入库历史记录',
    bookType: bookType || 'xlsx',
  });
}
</script>