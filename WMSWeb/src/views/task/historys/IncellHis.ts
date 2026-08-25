import { FormSchema } from '/@/components/Table';
import { BasicColumn } from '/@/components/Table';
import { SelectItem } from '/@/utils/SelectItem';
import moment from 'moment';
import {
    StockInHistoryServiceProxy,
} from '/@/services/ServiceProxies';
import { message } from 'ant-design-vue';
import { useLoading } from '/@/components/Loading';
import { useI18n } from '/@/hooks/web/useI18n';
const { t } = useI18n();
const [openFullLoading, closeFullLoading] = useLoading({
  tip: 'Loading...',
});
const _StockInHistoryServiceProxy = new StockInHistoryServiceProxy()
export const cellStatusSelectItem: SelectItem[] = [
    {
      label: '可用',
      value: 'Available',
      key: 0,
    },
    {
      label: '锁定',
      value: 'Locked',
      key: 1,
    },
    {
        label: '冻结',
        value: 'Freezing',
        key: 1,
      },
  ];
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
    dataIndex: 'materialSpecs',
  },
  {
    title: t('数量'),
    dataIndex: 'inCount',
  },
  {
    title: t('单位'),
    dataIndex: 'materialUnit',
  },
  {
    title: t('生产批号'),
    dataIndex: 'batchNo',
  },
  {
    title: t('入库时间'),
    dataIndex: 'inTime',
    customRender: ({ text }) => {
         return moment(text).format('YYYY-MM-DD HH:mm:ss');
     },
  },
   
  {
    title: t('收料码'),
    dataIndex: 'barcode',
  },
  {
    title: t('容器编号'),
    dataIndex: 'boxCode',
  },
  {
    title: t('容器名称'),
    dataIndex: 'boxName',
  },
  {
    title: t('库位编号'),
    dataIndex: 'cellCode',
  },
  {
    title: t('库位名称'),
    dataIndex: 'cellName',
  },
  {
    title: t('区域编号'),
    dataIndex: 'areaCode',
  },
  {
    title: t('区域名称'),
    dataIndex: 'areaName',
  },
  {
    title: t('仓库编号'),
    dataIndex: 'warehouseCode',
  },
  {
    title: t('仓库名称'),
    dataIndex: 'warehouseName',
  },
  {
    title: t('入库类型'),
    dataIndex: 'stockInType',
  },
  {
    title: t('环保要求'),
    dataIndex: 'isHB',
    customRender: ({ text }) => {
      return text ? '是' : '否';
    },
  },
  {
    title: t('保质期'),
    dataIndex: 'expiryDate',
    customRender: ({ text }) => {
      return text ? moment(text).format('YYYY-MM-DD') : '';
    },
  },
  {
    title: t('备料单'),
    dataIndex: 'blCode',
  },
  {
    title: t('备货单号'),
    dataIndex: 'bhCode',
  },
  {
    title: t('操作者'),
    dataIndex: 'operatorName',
  },

];

export const searchFormSchema: FormSchema[] = [
  {
    field: 'materialNameTip',
    label: t('物料名称'),
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'materialSpecsTip',
    label: t('物料规格'),
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'materialCode',
    label: t('物料编号'),
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'batchNo',
    label: t('生产批号'),
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'barcode',
    label: t('收料码'),
    component: 'Input',
    colProps: { span: 6 },
  },

  {
    field: 'stockInType',
    label: t('入库类型'),
    component: 'Select',
    colProps: { span: 6 },
    componentProps:{
      options:[
        {
          label: '正常采购',
          value: '正常采购',

        },
        {
          label: '生产入库',
          value: '生产入库',
        },
        {
            label: '委托加工',
            value: '委托加工',
          },
          {
            label: '盘点入库',
            value: '盘点入库',
          },
          {
            label: '超期复检',
            value: '超期复检',
          },        
        ]
    }
  },
  
  {
    field: 'time',
    component: 'RangePicker',
    label: '入库时间',
    labelWidth: 80,
    colProps: { span: 6 },
    defaultValue: [moment().subtract(7, 'days'), moment().add(1, 'days')],
  },
  
];

/**
 * 分页列表
 * @param params
 * @returns
 */
export async function getTableListAsync(
  params
): Promise<any> {

  return _StockInHistoryServiceProxy.pagedStockInHistoriesGet(params);
}


export async function allStockInHistoriesGet(
  params
): Promise<any> {

  return _StockInHistoryServiceProxy.allStockInHistoriesGet(params);
}