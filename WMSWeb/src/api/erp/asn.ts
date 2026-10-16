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
