import { FormSchema } from '/@/components/Table';
import { BasicColumn } from '/@/components/Table';
import { SelectItem } from '/@/utils/SelectItem';
import moment from 'moment';
import { h } from 'vue';
import {
  StockServiceProxy
} from '/@/services/ServiceProxies';
import { message } from 'ant-design-vue';
import { useLoading } from '/@/components/Loading';
import { useI18n } from '/@/hooks/web/useI18n';
const { t } = useI18n();
const [openFullLoading, closeFullLoading] = useLoading({
  tip: 'Loading...',
});
const _StockServiceProxy = new StockServiceProxy()

export const tableColumns: BasicColumn[] = [
  {
    title: t('物料编号'),
    dataIndex: 'materialCode',
  },
  {
    title: t('物料名称'),
    dataIndex: 'materialName',
  },
  {
    title: t('规格'),
    dataIndex: 'specs',
  },
  {
    title: t('数量'),
    dataIndex: 'receiveTotalCount',
  },
  {
    title: t('库位'),
    dataIndex: 'cellCode',
  },
  {
    title: t('容器'),
    dataIndex: 'boxCode',
  },
  {
    title: t('入库日期'),
    dataIndex: 'stockInDate',
  },
  {
    title: t('状态'),
    dataIndex: 'status',
  },
];

export const searchFormSchema: FormSchema[] = [
  {
    field: 'materialCode',
    label: t('物料编号'),
    component: 'Input',
    colProps: { span: 4 },
  },
  {
    field: 'materialNameTip',
    label: t('物料名称'),
    component: 'Input',
    colProps: { span: 4 },
  },
  {
    field: 'cellCode',
    label: t('库位编号'),
    component: 'Input',
    colProps: { span: 4 },
  },
  {
    field: 'boxCode',
    label: t('容器编号'),
    component: 'Input',
    colProps: { span: 4 },
  },
];

// 库存查询接口
export async function getStocks(params) {
  openFullLoading();
  try {
    const res = await _StockServiceProxy.stocksQuery(params);
    closeFullLoading();
    return res;
  } catch (error) {
    closeFullLoading();
    message.error('查询失败');
    return [];
  }
}

// 创建出库任务
export async function createOutStockTask(boxCode, fromCellCode, toCellCode) {
  openFullLoading();
  try {
    const res = await _StockServiceProxy.createStockTask(boxCode, fromCellCode, toCellCode);
    closeFullLoading();
    return res;
  } catch (error) {
    closeFullLoading();
    message.error('创建任务失败');
    return { success: false, message: '创建任务失败' };
  }
}
