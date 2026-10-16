import { defHttp } from '/@/utils/http/axios';

/** U8检验结果，仅CanPurchase=true时允许采购入库；业务错误保留原始响应。 */
export interface U8InspectionLine {
  asnLineId: string;
  materialCode: string;
  batchCode?: string;
  checkCode?: string;
  qualifiedQuantity: number;
  canPurchase: boolean;
  isPurchasePushed: boolean;
  message?: string;
}

/** 以ASN查询关联检验单，来源匹配和数量检查由后端完成。 */
export function queryU8Inspection(asnCode: string) {
  return defHttp.get<{ success: boolean; message?: string; items: U8InspectionLine[] }>(
    { url: '/erp/asn/inspection-results?asnCode=' + encodeURIComponent(asnCode.trim()) },
    { isTransformResponse: false },
  );
}

/** 到货单在U8审核后主动报检，未审核时返回可重试的业务提示。 */
export function reportU8Inspection(asnCode: string) {
  return defHttp.post<{ success: boolean; message?: string }>(
    { url: '/erp/asn/push-inspection-by-asn?asnCode=' + encodeURIComponent(asnCode.trim()) },
    { isTransformResponse: false },
  );
}

/** 仅重试指定ASN行的采购入库，不重复确认合格或回补样品数量。 */
export function retryU8Purchase(asnLineId: string) {
  return defHttp.post<{ success: boolean; message?: string }>(
    { url: '/erp/asn/push-purchase-by-inspection?asnLineId=' + encodeURIComponent(asnLineId) },
    { isTransformResponse: false },
  );
}

