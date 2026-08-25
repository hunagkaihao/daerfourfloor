import { FormSchema } from '/@/components/Table';
import { BasicColumn } from '/@/components/Table';
import moment from 'moment';
import {
  MaterialServiceProxy,
  StockServiceProxy,
  ERP_ASNServiceProxy,
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
const _erpAsnServiceProxy = new ERP_ASNServiceProxy();

export interface IncompleteAsnItem {
  rowKey?: string;
  ccode?: string;
  cordercode?: string;
  cinvcode?: string;
  cinvname?: string;
  cinvstd?: string;
  iquantity?: number;
  alreadyStockInQuantity?: number;
}

/**
 * 通过物料编号获取未完成的ASN单据
 */
export async function getIncompleteAsnByMaterialCode(materialCode: string): Promise<IncompleteAsnItem[]> {
  const result = await _erpAsnServiceProxy.incompleteByMaterial(materialCode);
  if (!result.success || !result.data?.length) {
    return [];
  }

  return result.data.map((item, index) => ({
    rowKey: `${item.ccode || ''}-${item.cordercode || ''}-${index}`,
    ccode: item.ccode,
    cordercode: item.cordercode,
    cinvname: item.cinvname,
    iquantity: item.iquantity,
    alreadyStockInQuantity: item.alreadyStockInQuantity ?? 0,
  }));
}

export interface IncompleteAsnGroup {
  ccode: string;
  items: IncompleteAsnItem[];
}

export function groupIncompleteAsnByCode(items: IncompleteAsnItem[] = []): IncompleteAsnGroup[] {
  const groupMap = new Map<string, IncompleteAsnItem[]>();

  items.forEach((item) => {
    const code = item.ccode || '-';
    if (!groupMap.has(code)) {
      groupMap.set(code, []);
    }
    groupMap.get(code)!.push(item);
  });

  return Array.from(groupMap.entries()).map(([ccode, groupItems]) => ({
    ccode,
    items: groupItems,
  }));
}

const incompleteAsnDetailColumns = [
  {
    title: '订单号',
    dataIndex: 'cordercode',
    key: 'cordercode',
    width: 130,
    ellipsis: true,
  },
  {
    title: '入库数',
    dataIndex: 'iquantity',
    key: 'iquantity',
    width: 60,
    align: 'center',
  },
  {
    title: '已入数',
    dataIndex: 'alreadyStockInQuantity',
    key: 'alreadyStockInQuantity',
    width: 60,
    align: 'center',
  },
];

export const incompleteAsnColumns = [
  ...incompleteAsnDetailColumns,
  {
    title: '操作',
    key: 'action',
    width: 56,
    align: 'center',
    slots: { customRender: 'asnSelect' },
  },
];

export const selectedAsnColumns = [
  ...incompleteAsnDetailColumns,
  {
    title: '操作',
    key: 'action',
    width: 56,
    align: 'center',
    slots: { customRender: 'asnReselect' },
  },
];

/**
 * 通过物料码查询物料信息
 */
export async function materialsWithCodeTipGet(params: any): Promise<any> {
  return _materialServiceProxy.materialsWithCodeTipGet(params);
}

/**
 * 容器组盘（关联ASN订单号）
 */
export async function stockCreateAndBindBoxWithAsn(boxcode: string, orderCode: string, params: any): Promise<any> {
  return _stockServiceProxy.stockCreateAndBindBoxWithAsn(boxcode, orderCode, params);
}

/**
 * ASN校验
 */
export async function validateAsn(asnCode: string): Promise<any> {
  openFullLoading();
  try {
    const result = await _erpAsnServiceProxy.get(asnCode);
    closeFullLoading();
    return result;
  } catch (error) {
    closeFullLoading();
    message.error('ASN校验失败');
    return { success: false, message: 'ASN校验失败' };
  }
}

export class DataItem {
  goodsId: number | undefined;
  materialCode: string | undefined;
  materialName: string | undefined;
  goodsSpec: string | undefined;
  goodsBand: string | undefined;
  goodsBatchNo: string | undefined;
  quantity: number | undefined;
  goodsUnits: string | undefined;
  goodsProperty1: string | undefined;
  supplierCode: string | undefined;
  dataCode: string | undefined;
  ProcessNo: string | undefined;
  grade: string | undefined;
  supplierProductionDate: string | undefined;
  boxNumber: string | undefined;
  capacity: number | undefined;
  baoshu: number | undefined;
  sanjianshu: number | undefined;
  incellshu: number | undefined;
  countInOnePkgOrBox: number | undefined;
}

export interface GoodsInBox {
  goodsId: number;
  materialCode: string;
  materialName: string;
  goodsSpec: string;
}

export const diskcolumns: BasicColumn[] = [
  {
    title: '物料编号',
    dataIndex: 'materialCode',
  },
  {
    title: '物料名称',
    dataIndex: 'materialName',
  },
  {
    title: '数量',
    dataIndex: 'totalCount',
  },
  {
    title: '库位',
    dataIndex: 'cellCode',
  },
  {
    title: '容器',
    dataIndex: 'boxCode',
  },
];
