import { defHttp } from '/@/utils/http/axios';

/** ASN本地物料检验状态，查询与确认均按送货单及采购订单隔离。 */
export interface AsnMaterialInspectionLine {
  autoid?: string;
  ccode?: string;
  cordercode?: string;
  cinvcode?: string;
  cinvname?: string;
  cbatch?: string;
  /** ASN等级，与零头柜标识卡第4段匹配。 */
  cfree5?: string;
  /** ASN物料计量单位。 */
  cinfvm_unit?: string;
  iquantity?: number;
  alreadyStockInQuantity?: number;
  status?: number;
  materialInspectionStatus?: number;
  materialInspectionStatusName?: string;
  isInspectionReportPushed?: boolean;
  /** 本地到货推送状态及真实U8来源关联，用于明细入口判断是否可报检。 */
  isPushedToErp?: boolean;
  arrivalVoucherId?: number;
  arrivalVoucherCode?: string;
  arrivalDetailId?: number;
  arrivalPushError?: string;
  inspectionPushError?: string;
}

/** 查询ASN检验状态；fromErp用于首次扫码获取ERP明细，保留代理尚未包含的状态字段。 */
export function queryAsnMaterialInspection(asnCode: string, fromErp = false) {
  return defHttp.get<{ success: boolean; message?: string; data?: AsnMaterialInspectionLine[] }>(
    { url: fromErp ? '/erp/asn/get-from-erp' : '/erp/asn/get', params: { asnCode: asnCode.trim() } },
    { isTransformResponse: false },
  );
}

/** 按物料查询的U8来料检验单，数量缺失时保持未知，不当作0或合格。 */
export interface U8MaterialInspectionItem {
  /** U8检验单主键和单号，接口未提供时为空。 */
  checkId?: number;
  checkCode?: string;
  /** 来源到货单及其明细标识，用于关联正确的本地组盘来源。 */
  sourceCode?: string;
  sourceAutoId?: number;
  materialCode: string;
  batchCode?: string;
  /** 合格、不合格件数；null表示U8未提供数量。 */
  qualifiedQuantity?: number | null;
  disqualifiedQuantity?: number | null;
  verifier?: string;
  isVerified: boolean;
  /** 拒收标志；null表示U8未提供。 */
  isRejected?: boolean | null;
  /** 严格匹配到的本地ASN及明细，未匹配时只展示U8单据。 */
  asnCode?: string;
  asnLines: AsnMaterialInspectionLine[];
}

/** 单页查询结果；还有后续页时不能据当前页未匹配认定物料没有检验单。 */
export interface U8MaterialInspectionResult {
  success: boolean;
  message?: string;
  /** 查询物料码；未按物料筛选时为空。 */
  materialCode?: string | null;
  hasInspection: boolean;
  /** 本次页码、每页条数及U8声明的总记录数。 */
  page: number;
  pageSize: number;
  totalCount: number;
  /** 由页面按需继续加载后续页，避免单次请求自动遍历全部记录。 */
  hasMore: boolean;
  items: U8MaterialInspectionItem[];
}

/**
 * 单次只查一页，空筛选不发送；等待45秒以覆盖后端单页30秒时限及登录开销。
 * @param page U8的pagesize，即从1开始的页码。
 * @param pageSize U8的pagecount，即每页条数，范围1至100，默认100。
 */
export function queryMaterialInspection(materialCode: string, sourceCode?: string, beginMakeTime?: string, page = 1, pageSize = 100) {
  return defHttp.post<U8MaterialInspectionResult>(
    { url: '/erp/asn/material-inspection-results', timeout: 45 * 1000, data: {
      materialCode: materialCode.trim() || undefined, sourceCode: sourceCode?.trim() || undefined,
      beginMakeTime: beginMakeTime || undefined,
      page,
      pageSize,
    } },
    { isTransformResponse: false },
  );
}

/** 所选检验单及原查询分页参数；服务端重查审核状态后仅确认对应来源明细。 */
export interface U8MaterialInspectionConfirmation {
  /** 查询物料、制单开始日期及所选记录所在页，分页含义与查询接口一致。 */
  materialCode: string;
  beginMakeTime?: string;
  page: number;
  pageSize: number;
  /** U8检验单及来源明细身份，不使用前端审核标志作为确认依据。 */
  checkId: number;
  checkCode: string;
  sourceAutoId: number;
  asnCode: string;
  /** 人工检验结论：2合格、3不合格。 */
  inspectionStatus: 2 | 3;
}

/** 按已审核的所选检验单持久化对应ASN明细的检验结论，保持单页查询的45秒超时。 */
export function confirmMaterialInspection(data: U8MaterialInspectionConfirmation) {
  return defHttp.post<{ success: boolean; message?: string; data?: AsnMaterialInspectionLine[] }>(
    { url: '/erp/asn/confirm-material-inspection', timeout: 45 * 1000, data },
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
        : `当前${line.materialInspectionStatusName || '待检'}，请先确认检验合格后再到物料组盘页组盘`,
    };
  });
  return { success: result.success, message: result.message, items };
}

/** U8检验结果：CanGroup表示可逐箱组盘，CanPurchase表示该单据对应明细组盘完成后可独立采购入库。 */
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
