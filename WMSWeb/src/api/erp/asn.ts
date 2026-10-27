import { ERP_ASNServiceProxy, ErpAsnDto } from '/@/services/ServiceProxies';
import { defHttp } from '/@/utils/http/axios';

const asnService = new ERP_ASNServiceProxy();

export async function getAsnList(params: {
  page: number;
  pageSize: number;
  asnCode?: string;
  supplierName?: string;
  startDate?: string;
  endDate?: string;
  status?: number;
}) {
  const { page, pageSize, asnCode, supplierName, startDate, endDate, status } = params;
  return defHttp.get<{ items: AsnWorkflowLine[]; total: number }>(
    { url: '/erp/asn/list', params: { page, pageSize, asnCode, supplierName, startDate, endDate, status } },
    { isTransformResponse: false },
  );
}

export async function pushReceipt(asnCode: string) {
  return asnService.pushReceipt(asnCode);
}

/** ASN的U8流程字段；使用原始响应保留生成代理尚未包含的来源关联。 */
export type AsnWorkflowLine = ErpAsnDto & {
  isPushedToErp?: boolean;
  pushTime?: string;
  arrivalVoucherId?: number;
  arrivalVoucherCode?: string;
  arrivalDetailId?: number;
  arrivalPushError?: string;
  arrivalPushAttemptTime?: string;
  isInspectionReportPushed?: boolean;
  inspectionPushError?: string;
  /** 物料人工检验状态，与报检接口推送结果分开。 */
  materialInspectionStatus?: number;
  materialInspectionStatusName?: string;
};

/** U8推送响应，message保留客户接口的具体失败原因。 */
export interface AsnPushResult { success: boolean; message?: string }

/** 按完整ASN生成到货单或重试；报文由后端依据本地来源构造。 */
export function pushAsnArrival(asnCode: string) {
  return defHttp.post<AsnPushResult>(
    { url: '/erp/asn/push-arrival-by-asn?asnCode=' + encodeURIComponent(asnCode) },
    { isTransformResponse: false },
  );
}

/** 到货单成功关联后生成来料报检单。 */
export function pushAsnInspection(asnCode: string) {
  return defHttp.post<AsnPushResult>(
    { url: '/erp/asn/push-inspection-by-asn?asnCode=' + encodeURIComponent(asnCode) },
    { isTransformResponse: false },
  );
}

/** 明细操作判断所需的本地状态，PDA与管理页共用；不依赖生成代理是否包含新字段。 */
export interface AsnOrderPushLine {
  /** 当前明细所属采购订单号。 */
  cordercode?: string;
  /** 取消状态4不参与推送。 */
  status?: number;
  /** ASN到货数量，须大于0。 */
  iquantity?: number;
  /** 到货单是否推送成功。 */
  isPushedToErp?: boolean;
  /** U8来源到货单主表、单号及明细关联，报检前必须齐全。 */
  arrivalVoucherId?: number;
  arrivalVoucherCode?: string;
  arrivalDetailId?: number;
  /** 报检单是否推送成功。 */
  isInspectionReportPushed?: boolean;
}

/** 按所属采购订单判断按钮状态；同一订单的所有有效明细一起生成单据。 */
export function getAsnOrderPushState(lines: AsnOrderPushLine[], orderCode?: string) {
  const orderLines = orderCode ? lines.filter(line => line.status !== 4 && line.cordercode === orderCode) : [];
  const allArrived = orderLines.length > 0 && orderLines.every(line => line.isPushedToErp);
  return {
    canPushArrival: orderLines.length > 0 && !allArrived && orderLines.every(line => Number(line.iquantity) > 0),
    canPushInspection: allArrived && orderLines.some(line => !line.isInspectionReportPushed) && orderLines.every(line =>
      !!line.arrivalVoucherId && !!line.arrivalVoucherCode && !!line.arrivalDetailId),
  };
}

/** 明细入口仅推送当前ASN、指定采购订单的完整到货单。 */
export function pushAsnOrderArrival(asnCode: string, orderCode: string) {
  return defHttp.post<AsnPushResult>(
    { url: '/erp/asn/push-arrival-by-order', params: { asnCode: asnCode.trim(), orderCode } },
    { isTransformResponse: false, joinParamsToUrl: true },
  );
}

/** 明细入口仅报检指定采购订单对应的到货单，重试由服务端保持原标识。 */
export function pushAsnOrderInspection(asnCode: string, orderCode: string) {
  return defHttp.post<AsnPushResult>(
    { url: '/erp/asn/push-inspection-by-order', params: { asnCode: asnCode.trim(), orderCode } },
    { isTransformResponse: false, joinParamsToUrl: true },
  );
}
