import { FormSchema } from '/@/components/Table';
import { BasicColumn } from '/@/components/Table';
import moment from 'moment';
import {
  ErpOutboundOrderServiceProxy,
  ErpOutboundOrderDto
} from '/@/services/ServiceProxies';
import { message } from 'ant-design-vue';
import { useLoading } from '/@/components/Loading';
import { useI18n } from '/@/hooks/web/useI18n';
const { t } = useI18n();
const [openFullLoading, closeFullLoading] = useLoading({
  tip: 'Loading...',
});
const _ErpOutboundOrderServiceProxy = new ErpOutboundOrderServiceProxy()

export const tableColumns: BasicColumn[] = [
  {
    title: '出库单号',
    dataIndex: 'orderNo',
  },
  {
    title: '仓库代码',
    dataIndex: 'warehouseCode',
  },
  {
    title: '计划出库日期',
    dataIndex: 'planOutboundDate',
    customRender: ({ record }) => {
      return record.planOutboundDate ? moment(record.planOutboundDate).format('YYYY-MM-DD HH:mm:ss') : '';
    },
  },
  {
    title: '实际出库日期',
    dataIndex: 'actualOutboundDate',
    customRender: ({ record }) => {
      return record.actualOutboundDate ? moment(record.actualOutboundDate).format('YYYY-MM-DD HH:mm:ss') : '';
    },
  },
  {
    title: '状态',
    dataIndex: 'status',
    customRender: ({ record }) => {
      const statusMap: Record<number, string> = {
        0: '待处理',
        1: '已完成',
        2: '已取消'
      };
      return statusMap[record.status] || '未知';
    },
  },
  {
    title: '备注',
    dataIndex: 'remark',
  },
];

export const searchFormSchema: FormSchema[] = [
  {
    field: 'orderNo',
    label: '出库单号',
    component: 'Input',
    colProps: { span: 4 },
  },
  {
    field: 'warehouseCode',
    label: '仓库代码',
    component: 'Input',
    colProps: { span: 4 },
  },
  {
    field: 'status',
    label: '状态',
    component: 'Select',
    colProps: { span: 4 },
    componentProps: {
      options: [
        { label: '待处理', value: 0 },
        { label: '已完成', value: 1 },
        { label: '已取消', value: 2 },
      ],
    },
  },
  {
    field: 'startDate',
    label: '开始日期',
    component: 'DatePicker',
    colProps: { span: 4 },
    componentProps: {
      format: 'YYYY-MM-DD',
      placeholder: '选择开始日期',
    },
  },
  {
    field: 'endDate',
    label: '结束日期',
    component: 'DatePicker',
    colProps: { span: 4 },
    componentProps: {
      format: 'YYYY-MM-DD',
      placeholder: '选择结束日期',
    },
  },
];

// 出库单查询接口
export async function getOutboundOrders(params) {
  openFullLoading();
  try {
    const { warehouseCode, status, startDate, endDate } = params;
    const res = await _ErpOutboundOrderServiceProxy.list(warehouseCode, status, startDate, endDate);
    closeFullLoading();
    return res;
  } catch (error) {
    closeFullLoading();
    message.error('查询失败');
    return [];
  }
}

// 出库单详情接口
export async function getOutboundOrderDetail(id: string) {
  openFullLoading();
  try {
    const res = await _ErpOutboundOrderServiceProxy.outboundOrderGet(id);
    closeFullLoading();
    return res;
  } catch (error) {
    closeFullLoading();
    message.error('查询详情失败');
    return null;
  }
}

// 出库单删除接口
export async function deleteOutboundOrder(id: string) {
  openFullLoading();
  try {
    const res = await _ErpOutboundOrderServiceProxy.outboundOrderDelete(id);
    closeFullLoading();
    return res;
  } catch (error) {
    closeFullLoading();
    message.error('删除失败');
    return false;
  }
}
