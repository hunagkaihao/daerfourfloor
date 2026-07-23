import { FormSchema } from '/@/components/Table';
import { BasicColumn } from '/@/components/Table';
import moment from 'moment';
import {
  StockServiceProxy,
  ERP_ASNServiceProxy
} from '/@/services/ServiceProxies';
import { h } from 'vue';
import { Tag, message } from 'ant-design-vue';
import { useLoading } from '/@/components/Loading';

import { useI18n } from '/@/hooks/web/useI18n';
const { t } = useI18n();
const [openFullLoading, closeFullLoading] = useLoading({
  tip: 'Loading...',
});
const _stockServiceProxy = new StockServiceProxy();
const _erpAsnServiceProxy = new ERP_ASNServiceProxy();
/**
 * 获取容器里库存物料信息
 * @param params
 * @returns
 */
export async function stocksGetInBox(
  boxcode
): Promise<any> {

  return _stockServiceProxy.stocksGetInBox(boxcode);
}
/**
 * 获取库位里库存物料信息
 * @param cellCode 库位编码
 * @returns
 */
export async function stocksGetInCell(
  cellCode: string
): Promise<any> {

  return _stockServiceProxy.stocksGetInCell(cellCode);
}
/**
 * 容器绑定库位
 * @param params
 * @returns
 */
export function boxBindCell(
  boxCode: string,
  cellCode: string,
):Promise<any> {
  return _stockServiceProxy.boxBindCell(boxCode,cellCode);
}
/**
 * 容器解绑库位
 * @param params
 * @returns
 */
export function boxDisBindCell(
  boxCode: string,
  cellCode: string,
):Promise<any> {
  return _stockServiceProxy.boxDisBindCell(boxCode,cellCode);
}
//241022创建容器CTU入库任务
export function CreateCtuBasicIn(
  boxCode: string,
  cellCode: string,
):Promise<any> {
  return _stockServiceProxy.createCtuBasicIn(boxCode,cellCode);
}
//241022创建容器CTU入库任务
export function createStockTask(
  boxCode: string,
  cellCode: string,
  endcellCode: string,
):Promise<any> {
  return _stockServiceProxy.createStockTask(boxCode,cellCode,endcellCode);
}
// 创建容器搬运出库任务
export function createStockTaskV2(
  boxCode: string,
  cellCode: string,
  endcellCode: string,
):Promise<any> {
  return _stockServiceProxy.createStockTaskV2(boxCode,cellCode,endcellCode);
}
//查询库存
export function stocksQuery(
  PagedStockQueryDto 
):Promise<any> {
  return _stockServiceProxy.stocksQuery(PagedStockQueryDto);
}
//库存清空
export function stocksDisBindBox(
  PagedStockQueryDto 
):Promise<any> {
  return _stockServiceProxy.stocksDisBindBox(PagedStockQueryDto);
}
//库存清空
export function stockRemoveDirect(
  stockId
):Promise<any> {
  return _stockServiceProxy.stockRemoveDirect(stockId);
}
//物料抽检
export function stockInspection(
  stockId: string,
  outBoundCount: number,
  pagOrBoxCount?: number
):Promise<any> {
  return _stockServiceProxy.stockInspection(stockId, outBoundCount, pagOrBoxCount);
}
//抽检完成
export function setInspectionCompleted(
  stockId: string
):Promise<any> {
  return _stockServiceProxy.setInspectionCompleted(stockId);
}
export function pushInspectionReport(
  stockIds: string[]
):Promise<any> {
  return _stockServiceProxy.pushInspectionReport(stockIds);
}
//查找抽检中的库存
export function findStockByCellAndMaterial(
  cellCode: string,
  materialCode: string
): Promise<any> {
  return _stockServiceProxy.findByCellAndMaterial(cellCode, materialCode);
}
//确认抽检合格
export function confirmInspectionQualified(
  stockId: string,
  qualifiedQty: number
): Promise<any> {
  return _stockServiceProxy.confirmInspectionQualified(stockId, qualifiedQty);
}
//设置抽检不合格
export function setInspectionNotQualified(
  stockId: string
): Promise<any> {
  return _stockServiceProxy.setInspectionNotQualified(stockId);
}
//推送采购入库单到U8
export function pushCGRKDAdd(
  params: any
): Promise<any> {
  return _erpAsnServiceProxy.pushCGRKDAdd(params);
}
export const diskcolumns = [
  {
    title: '物料编号',
    dataIndex: 'materialCode',
  },
  {
    title: '物料名称',
    dataIndex: 'materialName',
  },
  {
    title: '箱数',
    dataIndex: 'totalPagOrBoxInTime',
  },
  {
    title: '数量',
    dataIndex: 'totalCountInTime',
  },
  {
    title: '库位',
    dataIndex: 'cellCode',
  },
  {
    title: '生产批号',
    dataIndex: 'batchCode',
  },
];

export const columns = [
  {
    title: '收料条形码',
    dataIndex: 'barcode',
    key: 'barcode',
    align: "center",
  },
  {
    title: '物料名称',
    dataIndex: 'materialName',
    key: 'materialName',
    align: "center",
  },
  {
    title: '物料编码',
    dataIndex: 'materialCode',
    key: 'materialCode',
    align: "center",
  },
  {
    title: '箱数',
    dataIndex: 'totalPagOrBoxInTime',
    key: 'totalPagOrBoxInTime',
    align: "center",
  },
  {
    title: '入库数量',
    key: 'totalCountInTime',
    dataIndex: 'totalCountInTime',
    align: "center",
  },
  {
    title: '被抽检数量',
    key: 'inspectionCount',
    dataIndex: 'inspectionCount',
    align: "center",
  },
  {
    title: '抽检状态',
    dataIndex: 'inspectionStatus',
    key: 'inspectionStatus',
    align: "center",
    width: 100,
    customRender: ({ text }) => {
      const statusMap: Record<number, { label: string; color: string }> = {
        0: { label: '待检', color: 'default' },
        1: { label: '抽检中', color: 'processing' },
        2: { label: '合格', color: 'success' },
        3: { label: '不合格', color: 'error' },
        4: { label: '抽检完成', color: 'default' },
      };
      const status = statusMap[text];
      if (!status) return text ?? '-';
      return h(Tag, { color: status.color }, () => status.label);
    },
  },
  {
    title: '操作',
    key: 'operation',
    align: "center",
    width: 50,
    slots: {
			customRender: 'bodyCell'
		}
  },
];