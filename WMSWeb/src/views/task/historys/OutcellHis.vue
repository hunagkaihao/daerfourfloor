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
      <template #action="{ record }">
        <TableAction
          :actions="[
            {
              auth: 'Wms.Edit',
              label: t('重新入库'),
              onClick: handleDelete.bind(null, record),
            },
          ]"
        />
      </template>
    </BasicTable>

    <ExpExcelModal @register="register" @success="exportExcel" />
  </div>
</template>

<script lang="ts" setup>
import { useMessage } from '/@/hooks/web/useMessage';
import { BasicTable, useTable, TableAction } from '/@/components/Table';
import { useModal } from '/@/components/Modal';
import { jsonToSheetXlsx, ExpExcelModal, ExportModalResult } from '/@/components/Excel';
import type { StockOutHistoryDto } from '/@/services/ServiceProxies';
import { ref } from 'vue';
import {
  tableColumns,
  searchFormSchema,
  getTableListAsync,
  checkDataCreateByOutHistory,
  allStockOutHistoriesGet,
  getRecordStageText,
} from './OutcellHis';
import moment from 'moment';
import { useI18n } from '/@/hooks/web/useI18n';

const [register, { openModal }] = useModal();
const { createConfirm, createMessage } = useMessage();
const { t } = useI18n();

const [registerTable, { reload, getForm }] = useTable({
  columns: tableColumns,
  formConfig: {
    labelWidth: 70,
    schemas: searchFormSchema,
    fieldMapToTime: [['time', ['stockOutTimeMin', 'stockOutTimeMax'], 'YYYY-MM-DD HH:mm:ss']],
  },
  api: getTableListAsync,
  showTableSetting: true,
  useSearchForm: true,
  bordered: true,
  canResize: true,
  showIndexColumn: false,
  actionColumn: {
    width: 150,
    title: t('common.action'),
    dataIndex: 'action',
    slots: { customRender: 'action' },
  },
});

const handleDelete = async (record: Recordable) => {
  createConfirm({
    title: t('common.deleteConfirmTitle'),
    content: t('common.deleteConfirmMessage'),
    onOk: async () => {
      await checkDataCreateByOutHistory({ id: record.id });
      reload();
    },
  });
};

/** 查询期间显示加载状态，避免重复获取多份全量数据。 */
const exportLoading = ref(false);
/** 本次导出查询的完整结果，确认文件名时保持相同范围。 */
let exportRows: StockOutHistoryDto[] = [];

/** 按当前出库时间和其他筛选获取全部分页记录，无需选择数据行。 */
async function handleOpenExport() {
  if (exportLoading.value) return;
  exportLoading.value = true;
  exportRows = [];
  try {
    const params = getForm().getFieldsValue();
    if (!(params.stockOutTimeMin && params.stockOutTimeMax) && params.time?.length !== 2) {
      createMessage.warning('请先选择出库时间范围');
      return;
    }
    exportRows = await allStockOutHistoriesGet(params);
    if (exportRows.length === 0) {
      createMessage.warning('所选时间范围内没有符合条件的出库记录');
      return;
    }
    openModal(true);
  } catch (error) {
    createMessage.error('获取出库历史导出数据失败，请重试');
  } finally {
    exportLoading.value = false;
  }
}

/** 保留原导出字段，追加可读的阶段与起终点信息；旧记录的新信息保持空值。 */
function exportExcel({ filename, bookType }: ExportModalResult) {
  const data = exportRows;
  if (!data || data.length === 0) {
    createMessage.warning('暂无数据可导出');
    return;
  }
  
  const exportData = data.map((item: any) => ({
    ...item,
    记录阶段: getRecordStageText(item.recordStage),
    起始库位编号: item.startCellCode || '',
    起始库位名称: item.startCellName || '',
    起始容器编号: item.startBoxCode || '',
    起始容器名称: item.startBoxName || '',
    目标库位编号: item.endCellCode || '',
    目标库位名称: item.endCellName || '',
    目标容器编号: item.endBoxCode || '',
    目标容器名称: item.endBoxName || '',
    关联AGV任务: item.agvTaskReqCode || '',
    stockOutTime: moment(item.stockOutTime).format('YYYY-MM-DD HH:mm:ss'),
    checkDate: item.checkDate ? moment(item.checkDate).format('YYYY-MM-DD HH:mm:ss') : '',
  }));
  
  jsonToSheetXlsx({
    data: exportData,
    filename: filename || '出库历史记录.xlsx',
    write2excelOpts: { bookType: bookType || 'xlsx' },
  });
}
</script>
