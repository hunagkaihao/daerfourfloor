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
import { StockOutHistoryDto } from '/@/services/ServiceProxies';
import {
  tableColumns,
  searchFormSchema,
  getTableListAsync,
  checkDataCreateByOutHistory,
} from './OutcellHis';
import moment from 'moment';
import { useI18n } from '/@/hooks/web/useI18n';

const [register, { openModal }] = useModal();
const { createConfirm, message } = useMessage();
const { t } = useI18n();

const [registerTable, { reload, getDataSource }] = useTable({
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

function exportExcel({ filename, bookType }: ExportModalResult) {
  const data = getDataSource();
  if (!data || data.length === 0) {
    message.warning('暂无数据可导出');
    return;
  }
  
  const exportData = data.map((item: any) => ({
    ...item,
    stockOutTime: moment(item.stockOutTime).format('YYYY-MM-DD HH:mm:ss'),
    checkDate: item.checkDate ? moment(item.checkDate).format('YYYY-MM-DD HH:mm:ss') : '',
  }));
  
  jsonToSheetXlsx({
    data: exportData,
    filename: `${filename || '出库历史记录'}_${moment().format('YYYYMMDDHHmmss')}`,
    bookType: bookType,
  });
}
</script>