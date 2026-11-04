<template>
  <div>
    <BasicTable @register="registerTable" :clickToRowSelect="false" size="small">
<template #toolbar>

        <a-button
          type="primary"
          :loading="exportLoading"
          @click="handleOpenExport"
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
import { BasicTable, useTable } from '/@/components/Table';
import { useModal } from '/@/components/Modal';
import { jsonToSheetXlsx, ExpExcelModal, ExportModalResult } from '/@/components/Excel';
import moment from 'moment';
import { ref } from 'vue';
import type { StockInHistoryDto } from '/@/services/ServiceProxies';
import {
  tableColumns,
  searchFormSchema,
  getTableListAsync,
  allStockInHistoriesGet,
} from './IncellHis';
import { useI18n } from '/@/hooks/web/useI18n';
const [register, { openModal }] = useModal();
const { createMessage } = useMessage();
const { t } = useI18n();
// table配置
const [registerTable, { getForm }] = useTable({
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
});

/** 导出查询期间显示加载状态并阻止重复请求。 */
const exportLoading = ref(false);
/** 保存本次时间范围内的完整导出结果，确认文件名时使用同一批记录。 */
let exportRows: StockInHistoryDto[] = [];

/** 根据当前入库时间和其他筛选加载全部匹配记录，无需勾选表格行。 */
async function handleOpenExport() {
  if (exportLoading.value) return;
  exportLoading.value = true;
  exportRows = [];
  try {
    const params = getForm().getFieldsValue();
    if (!(params.stockInTimeStart && params.stockInTimeEnd) && params.time?.length !== 2) {
      createMessage.warning('请先选择入库时间范围');
      return;
    }
    exportRows = await allStockInHistoriesGet(params);
    if (exportRows.length === 0) {
      createMessage.warning('所选时间范围内没有符合条件的入库记录');
      return;
    }
    openModal(true);
  } catch (error) {
    createMessage.error('获取入库历史导出数据失败，请重试');
  } finally {
    exportLoading.value = false;
  }
}

/** 将本次查询的全部入库记录按中文列名导出，文件类型由导出弹窗指定。 */
function handleExport({ filename, bookType }: ExportModalResult) {
  const exportData = exportRows.map((item) => ({
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
    createMessage.warning('没有可导出的入库记录，请重新查询');
    return;
  }
  jsonToSheetXlsx({
    data: exportData,
    filename: filename || '入库历史记录.xlsx',
    write2excelOpts: { bookType: bookType || 'xlsx' },
  });
}
</script>
