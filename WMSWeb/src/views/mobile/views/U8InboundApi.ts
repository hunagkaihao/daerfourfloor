import { defHttp } from '/@/utils/http/axios';

/** ASN本地物料检验状态，查询与确认均按送货单及采购订单隔离。 */
export interface AsnMaterialInspectionLine {
  autoid?: string;
  ccode?: string;
  cordercode?: string;
  cinvcode?: string;
  cinvname?: string;
  cbatch?: string;
  iquantity?: number;
  alreadyStockInQuantity?: number;
  status?: number;
  materialInspectionStatus?: number;
  materialInspectionStatusName?: string;
  isInspectionReportPushed?: boolean;
}

/** 查询ASN检验状态；fromErp用于首次扫码获取ERP明细，保留代理尚未包含的状态字段。 */
export function queryAsnMaterialInspection(asnCode: string, fromErp = false) {
  return defHttp.get<{ success: boolean; message?: string; data?: AsnMaterialInspectionLine[] }>(
    { url: fromErp ? '/erp/asn/get-from-erp' : '/erp/asn/get', params: { asnCode: asnCode.trim() } },
    { isTransformResponse: false },
  );
}

/** 按采购订单人工确认检验结果；失败时不更改状态，不修改物料数量。 */
export function setAsnOrderInspectionStatus(asnCode: string, orderCode: string, inspectionStatus: 2 | 3) {
  return defHttp.post<{ success: boolean; message?: string; data?: AsnMaterialInspectionLine[] }>(
    { url: '/erp/asn/material-inspection-status', params: { asnCode: asnCode.trim(), orderCode, inspectionStatus } },
    { isTransformResponse: false, joinParamsToUrl: true },
  );
}

/** 组盘只依据本地人工检验结论；采购入库另行查询U8真实检验单来源。 */
export async function queryAsnGroupingInspection(asnCode: string) {
  const result = await queryAsnMaterialInspection(asnCode);
  const items: U8InspectionLine[] = (result.data || []).filter(line => line.status !== 4).map(line => {
    const qualified = !!line.isInspectionReportPushed && line.materialInspectionStatus === 2;
    return {
      asnLineId: line.autoid || '', materialCode: line.cinvcode || '', batchCode: line.cbatch,
      canGroup: qualified, canPurchase: false, isPurchasePushed: false, qualifiedQuantity: 0,
      groupedQuantity: Number(line.alreadyStockInQuantity || 0),
      pendingGroupingQuantity: Math.max(0, Number(line.iquantity || 0) - Number(line.alreadyStockInQuantity || 0)),
      message: qualified ? '人工检验已确认合格，可逐箱组盘；采购入库需取得U8检验单来源'
        : `当前${line.materialInspectionStatusName || '待检'}，请在物料状态变更页确认合格后组盘`,
    };
  });
  return { success: result.success, message: result.message, items };
}

/** U8检验结果：CanGroup表示可逐箱组盘，CanPurchase表示整单组盘后可采购入库。 */
export interface U8InspectionLine {
  asnLineId: string;
  materialCode: string;
  batchCode?: string;
  checkCode?: string;
  qualifiedQuantity: number;
  canPurchase: boolean;
  canGroup: boolean;
  groupedQuantity?: number;
  pendingGroupingQuantity?: number;
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


/** 保存ASN后生成到货单，未组盘也可推送；失败时独立重试。 */
export function createU8Arrival(asnCode: string) {
  return defHttp.post<{ success: boolean; message?: string }>(
    { url: '/erp/asn/push-arrival-by-asn?asnCode=' + encodeURIComponent(asnCode.trim()) },
    { isTransformResponse: false },
  );
}
