import { FormSchema } from '/@/components/Table';
import { BasicColumn } from '/@/components/Table';
import { SelectItem } from '/@/utils/SelectItem';
import moment from 'moment';
import {
    StockOutHistoryServiceProxy,
    ChkResultListServiceProxy,
} from '/@/services/ServiceProxies';
import { message } from 'ant-design-vue';
import { useLoading } from '/@/components/Loading';
import { useI18n } from '/@/hooks/web/useI18n';
const { t } = useI18n();
const [openFullLoading, closeFullLoading] = useLoading({
  tip: 'Loading...',
});
const _StockOutHistoryServiceProxy = new StockOutHistoryServiceProxy()
const _ChkResultListServiceProxy = new ChkResultListServiceProxy()
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
    dataIndex: 'specs',
  },
  {
    title: t('数量'),
    dataIndex: 'stockOutCount',
  },
  {
    title: t('生产批号'),
    dataIndex: 'pickBatch',
  },
  {
    title: t('出库时间'),
    dataIndex: 'stockOutTime',
    customRender: ({ text }) => {
         return moment(text).format('YYYY-MM-DD HH:mm:ss');
     },
  },
   
  {
    title: t('收料码'),
    dataIndex: 'barcode',
  },
  {
    title: t('单位'),
    dataIndex: 'unit',
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
    dataIndex: 'houseCode',
  },
  {
    title: t('仓库名称'),
    dataIndex: 'houseName',
  },
  {
    title: t('检验时间'),
    dataIndex: 'checkDate',
    customRender: ({ text }) => {
      return moment(text).format('YYYY-MM-DD HH:mm:ss');
  },
  },
  // {
  //   title: t('检验类型'),
  //   dataIndex: 'checkType',
  // },
  {
    title: t('检验编号'),
    dataIndex: 'checkOrderCode',
  },
  {
    title: t('检验结果'),
    dataIndex: 'checkResult',
  },

  
  {
    title: t('供应商送货号'),
    dataIndex: 'supplierCode',
  },
  {
    title: t('供应商名称'),
    dataIndex: 'supplierName',
  },
  {
    title: t('出库类型'),
    dataIndex: 'stockOutType',
  },
  {
    title: t('产品编号'),
    dataIndex: 'goodsCode',
  },
  {
    title: t('加工产品'),
    dataIndex: 'goodsName',
  },
  {
    title: t('产品规格'),
    dataIndex: 'goodsSpecs',
  },
  {
    title: t('环保要求'),
    dataIndex: 'isHB',
  },
  {
    title: t('保质期'),
    dataIndex: 'expiryDate',
  },
  {
    title: t('领出人'),
    dataIndex: 'pickManName',
  },
  
  // {
  //   title: t('批号'),
  //   dataIndex: 'batchCode',
  // },
  // {
  //   title: t('对应备料单号'),
  //   dataIndex: 'blCode',
  // },
  // {
  //   title: t('对应备货单号'),
  //   dataIndex: 'bhCode',
  // },
  {
    title: t('操作者'),
    dataIndex: 'operator',
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
    field: 'materialCode',
    label: t('物料编号'),
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'pickBatchTip',
    label: t('生产批号'),
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
    field: 'barcode',
    label: t('收料码'),
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'stockOutType',
    label: t('出库类型'),
    component: 'Select',
    colProps: { span: 6 },
    componentProps:{
      options:[
        {
          label: '生产领用',
          value: '1',

        },
        {
          label: '超计划l领用',
          value: '14',
        },
        {
            label: '无计划领用',
            value: '15',
          },
          {
            label: '外协领用',
            value: '2',
          },        
        ]
    }
  },
  {
    field: 'time',
    component: 'RangePicker',
    label: '出库时间',
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

  return _StockOutHistoryServiceProxy.pagedStockOutHistoriesGet(params);
}
export async function allStockOutHistoriesGet(
  params
): Promise<any> {

  return _StockOutHistoryServiceProxy.allStockOutHistoriesGet(params);
}
//重新入库

export async function checkDataCreateByOutHistory({ id}) {
  try {
    openFullLoading();
    await  _ChkResultListServiceProxy.checkDataCreateByOutHistory(id).then((res)=>{
      message.success(res.message);
    })
    closeFullLoading();
    
  } catch (error) {
    message.error(error.error.message);
    closeFullLoading();
  }
}