import { FormSchema } from '/@/components/Table';
import { BasicColumn } from '/@/components/Table';
import moment from 'moment';
import {
  MaterialServiceProxy,
  StockServiceProxy
} from '/@/services/ServiceProxies';
import { message } from 'ant-design-vue';
import { useLoading } from '/@/components/Loading';

import { useI18n } from '/@/hooks/web/useI18n';
const { t } = useI18n();
const [openFullLoading, closeFullLoading] = useLoading({
  tip: 'Loading...',
});
const _materialServiceProxy = new MaterialServiceProxy();
const _stockServiceProxy = new StockServiceProxy();
/**
 * 通过物料码查询物料信息
 * @param params
 * @returns
 */
export async function materialsWithCodeTipGet(
  params
): Promise<any> {

  return _materialServiceProxy.materialsWithCodeTipGet(params);
}/**
 * 容器组盘
 * @param params
 * @returns
 */
export async function stockCreateAndBindBox(
  boxcode,params
): Promise<any> {

  return _stockServiceProxy.stockCreateAndBindBox(boxcode,params);
}

/**
 * ASN校验
 * @param asnCode ASN码
 * @returns ASN校验结果
 */
export async function validateAsn(asnCode: string): Promise<any> {
  openFullLoading();
  try {
    const response = await fetch(`/api/erp/asn/get?asnCode=${encodeURIComponent(asnCode)}`);
    const result = await response.json();
    closeFullLoading();
    return result;
  } catch (error) {
    closeFullLoading();
    message.error('ASN校验失败');
    return { success: false, message: 'ASN校验失败' };
  }
}
// 容器类型选项
export const boxTypeSelectItem = [
  {
    label: '托盘',
    value: '1',
    key: 0,
  },
  {
    label: '料箱',
    value: '2',
    key: 1,
  },
];
export const tableColumns: BasicColumn[] = [
  {
    title: t('容器编号'),
    dataIndex: 'boxCode',
  },
  {
    title: t('容器类型'),
    dataIndex: 'boxTypeName',
    customRender: ({ text }) => {
      const typeMap = {
        '1': '托盘',
        '2': '料箱',
        '12': '托盘',
      };
      return typeMap[text] || text;
    },
  },
  {
    title: t('库位名称'),
    dataIndex: 'cellName',
  },
  {
    title: t('状态'),
    dataIndex: 'status',
    customRender: ({ text }) => {
      const statusMap = {
        'NoHave': '无货',
        'Have': '有货',
      };
      return statusMap[text] || text;
    },
  },
  {
    title: t('库区'),
    dataIndex: 'warehouseAreaName',
  },
  {
    title: t('所在仓库'),
    dataIndex: 'warehouseName',
  },
  // {
  //   title: t('routes.warehouse.storageBoxManagement_createTime'),
  //   dataIndex: 'creationTime',
  //   customRender: ({ text }) => {
  //     return moment(text).format('YYYY-MM-DD HH:mm:ss');
  //   },
  // },
];

export const tableDetailColumns: BasicColumn[] = [
  {
    title: t('routes.warehouse.storageBoxManagement_storageBoxBarcode'),
    dataIndex: 'storageBoxBarcode',
    width: 100,
  },
  {
    title: t('routes.material.goodsManagement_goodsCode'),
    dataIndex: 'goodsCode',
    width: 100,
  },
  {
    title: t('routes.material.goodsManagement_name'),
    dataIndex: 'goodsName',
    width: 100,
  },
  {
    title: t('routes.material.goodsManagement_goodsSpec'),
    dataIndex: 'goodsSpec',
    width: 100,
  },
  {
    title: t('routes.material.goodsManagement_goodsBand'),
    dataIndex: 'goodsBand',
    width: 100,
  },
  {
    title: t('routes.warehouse.storageBoxDetailManagement_goodsBatchNo'),
    dataIndex: 'goodsBatchNo',
    width: 100,
  },
  {
    title: t('routes.warehouse.storageBoxDetailManagement_quantity'),
    dataIndex: 'quantity',
    width: 50,
  },
  {
    title: t('routes.material.goodsManagement_goodsUnits'),
    dataIndex: 'goodsUnits',
    width: 50,
  },
  {
    title: t('routes.warehouse.storageBoxManagement_createTime'),
    dataIndex: 'creationTime',
    customRender: ({ text }) => {
      return moment(text).format('YYYY-MM-DD HH:mm:ss');
    },
  },
];

export const searchFormSchema: FormSchema[] = [
  {
    field: 'boxCode',
    label: t('容器编号'),
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'boxName',
    label: t('容器名称'),
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'cellName',
    label: t('库位名称'),
    component: 'Input',
    colProps: { span: 6 },
  },
];

export const WallFormSchema: FormSchema[] = [
  {
    field: 'BoxCode',
    component: 'Input',
    label: t('容器编号'),
    labelWidth: 85,
    required: true,
    colProps: {
      span: 12,
    },
    componentProps: {
      autocomplete: 'off',
    },
  },
  {
    field: 'WallCell',
    component: 'Input',
    label: t('分拨墙库位'),
    labelWidth: 85,
    required: true,
    colProps: {
      span: 12,
    },
    componentProps: {
      autocomplete: 'off',
    },
  },
];

export const WallFormSchema2: FormSchema[] = [
  {
    field: 'WallCell',
    component: 'Input',
    label: t('分拨墙库位'),
    labelWidth: 85,
    required: true,
    colProps: {
      span: 12,
    },
    componentProps: {
      autocomplete: 'off',
    },
  },
];
export const createFormSchema: FormSchema[] = [
  {
    field: 'boxCode',
    component: 'Input',
    label: t('容器编号'),
    labelWidth: 85,
    required: true,
    colProps: {
      span: 12,
    },
    componentProps: {
      autocomplete: 'off',
    },
  },
  {
    field: 'boxTypeName',
    component: 'Select',
    label: t('容器类型'),
    labelWidth: 85,
    required: true,
    colProps: {
      span: 12,
    },
    componentProps: {
      options: [
        {
          label: '托盘',
          value: '1',
        },
        {
          label: '料箱',
          value: '2',
        },
      ],
    },
  },
 
 

];

export const editFormSchema: FormSchema[] = [
  {
    field: 'boxCodeNew',
    component: 'Input',
    label: t('容器编号'),
    labelWidth: 85,
    required: true,
    colProps: {
      span: 12,
    },
    componentProps: {
      autocomplete: 'off',
    },
  },
  {
    field: 'boxNameNew',
    component: 'Input',
    label: t('容器名称'),
    labelWidth: 85,
    required: true,
    colProps: {
      span: 12,
    },
    componentProps: {
      autocomplete: 'off',
    },
  },
  {
    field: 'boxTypeNameNew',
    component: 'Select',
    label: t('容器类型'),
    labelWidth: 85,
    required: true,
    colProps: {
      span: 12,
    },
    componentProps: {
      options: [
        {
          label: '托盘',
          value: '1',
        },
        {
          label: '料箱',
          value: '2',
        },
      ],
    },
  },
  
];


export class DataItem {
  goodsId: number | undefined;
  materialCode: string | undefined;
  materialName: string | undefined;
  goodsSpec: string | undefined;
  goodsBand: string | undefined;
  goodsBatchNo: string | undefined;
  quantity: number | undefined;
  goodsUnits: string | undefined;
  goodsProperty1:string | undefined;
  supplierCode:string | undefined;
  dataCode:string | undefined;
  ProcessNo:string | undefined;
  grade:string | undefined;
  supplierProductionDate:string | undefined;
  boxNumber:string | undefined;
  capacity:number | undefined;
  baoshu:number | undefined;
  sanjianshu:number | undefined;
  incellshu:number | undefined;
  countInOnePkgOrBox:number | undefined;
}