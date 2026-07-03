<template>
  <div>
    <BasicTable @register="registerTable" :clickToRowSelect="false" size="small"
    :expandable="{
      
      rowExpandable: record => record.pickItems && record.pickItems.length > 0
    }"
    row-key="key">
      <template #toolbar> 
                <a-button  type="primary" @click="openModal">
                    {{ t('Excel导出') }}
                </a-button>
            </template>
      <template #isActive="{ record }">
        <Tag :color="record.isActive ? 'green' : 'red'">
          {{ record.isActive ? t('common.enabled') : t('common.disEnabled') }}
        </Tag>
      </template>
      <template #expandedRowRender="{ record }" >
        <a-table :columns="innerColumns" :data-source="record.pickItems" :pagination="false">
          <template #bodyCell="{ column,record }">
                    <template v-if="column.key === 'operation'">
                        <span style="color:coral;font-weight: bold;" @click="handleEdit(record)">查询</span>
                    </template>
                </template>
        </a-table>
      </template>

    </BasicTable>

<linliaoNodeModal   @register="registerCreateOrderModal"></linliaoNodeModal>
 <ExpExcelModal @register="register" @success="defaultHeader" />
  </div>
</template>

<script lang="ts" setup>
import { useMessage } from '/@/hooks/web/useMessage';
import { BasicTable, useTable, TableAction } from '/@/components/Table';
import {
  tableColumns,
  searchFormSchema
} from './linliaoOrder';
import linliaoNodeModal from './linliaoNodeModal.vue';
import { useI18n } from '/@/hooks/web/useI18n';
import { useModal } from '/@/components/Modal';
import { PickListItemFlatDto } from '/@/services/ServiceProxies';
import { message } from 'ant-design-vue';
import { jsonToSheetXlsx, ExpExcelModal, ExportModalResult } from '/@/components/Excel';

const [register, { openModal }] = useModal();
const [registerCreateOrderModal, { openModal: openCreateOrderModal }] = useModal();
const { createConfirm } = useMessage();
const { t } = useI18n();
const innerColumns = [
{ title: '领用项号', dataIndex: 'uniqueCode', key: 'uniqueCode' },
  { title: '物料编码', dataIndex: 'materialCode', key: 'materialCode' },
  { title: '物料名称', dataIndex: 'materialName', key: 'materialName' },
  { title: '物料规格',dataIndex: 'specs', key: 'specs' },
  { title: '单位', dataIndex: 'unit', key: 'unit' },
  { title: '领用数量', dataIndex: 'countToPick', key: 'countToPick' },
  { title: '已领数量', dataIndex: 'pickedCount', key: 'pickedCount' },
  { title: '领用状态', dataIndex: 'pickItemStatus', key: 'pickItemStatus' },
  { title: '未领数量', dataIndex: 'countInRemaining', key: 'countInRemaining' },
  { title: '节点报表', dataIndex: 'operation', key: 'operation',slots: {
              customRender: 'bodyCell'
          } },
];
const handleEdit = (record: Recordable) => {
  console.log('查询功能已禁用，选中记录:', record);
  // 移除接口调用，只显示提示信息
  message.info('查询功能已禁用，请等待后续更新');
};
// table配置
const [registerTable, {getDataSource, reload,getSelectRows,clearSelectedRowKeys}] = useTable({
  columns: tableColumns,
  formConfig: {
    labelWidth: 70,
    schemas: searchFormSchema,
    fieldMapToTime: [['time', ['datetart', 'dateEnd'], 'YYYY-MM-DD HH:mm:ss']],
  },
  // api: gettable, // 移除API调用
  dataSource: [], // 使用静态数据
  showTableSetting: true,
  useSearchForm: true,
  bordered: true,
  canResize: true,
  rowKey: 'boxCode', //设置选择项的key
  showIndexColumn: false,
  rowSelection: { type: 'checkbox' },
});
// 移除gettable函数，不再需要接口调用

async function defaultHeader({ filename, bookType }: ExportModalResult) {
        // 移除接口调用，显示提示信息
        message.info('Excel导出功能已禁用，请等待后续更新');
        console.log('Excel导出功能已禁用');
      }


</script>