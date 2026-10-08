import type { StockDto } from '/@/services/ServiceProxies';
import { defHttp } from '/@/utils/http/axios';

/** 扫码命中的库存及所在库位的全部物料，保留原库存ID供回库任务再次校验。 */
export interface InboundReturnStock extends StockDto {
  /** 当前库位的全部有效库存明细，包含其他物料及批次。 */
  cellStocks?: StockDto[];
  /** 按物料编号去重的种类数。 */
  cellMaterialCount?: number;
  /** 当前库位所有库存的实时数量之和，计量单位见明细。 */
  cellTotalCount?: number;
  /** 实时总箱数；任一库存箱数缺失时为null，不按条码数量推算。 */
  cellTotalBoxCount?: number | null;
}

/** 回库申请沿用查询时的来源信息，由后端再次核对库存是否已经变更。 */
export interface InboundReturnTaskInput {
  /** 完整收料条码，按第一段物料编号匹配已有库存。 */
  receivingMaterialBarcode: string;
  /** 人工选择的库存记录ID。 */
  stockId: string;
  /** 查询时的来源库位，实际库存移走后拒绝下发。 */
  sourceCellCode: string;
  /** 所选库存的容器，AGV搬运该容器的全部物料。 */
  boxCode: string;
  /** 人工输入的同仓库目标入库库位。 */
  endCellCode: string;
}

/** 回库任务下发结果；成功仅表示已下发，库存位置在RCS完成回调后更新。 */
export interface InboundReturnTaskResult {
  /** RCS下发及WMS任务创建是否成功。 */
  success: boolean;
  /** 后端返回的下发结果或业务校验提示。 */
  message: string;
}

/** 解析新6段和旧8段收料码；无效扫码返回null，物料编号只从第一段读取。 */
export function parseInboundReturnBarcode(value: string) {
  const barcode = value.trim();
  const parts = barcode.split(',');
  if ((parts.length !== 6 && parts.length !== 8) || !parts[0].trim()) return null;
  return { barcode, materialCode: parts[0].trim() };
}

/** 扫描收料码后查询命中库存及所在库位的全部物料和汇总，保留不可搬运记录供查看。 */
export function getInboundReturnStocks(receivingMaterialBarcode: string): Promise<InboundReturnStock[]> {
  return defHttp.get<InboundReturnStock[]>(
    { url: '/wms/stock/inboundReturnStocks', params: { receivingMaterialBarcode } },
    { isTransformResponse: false },
  );
}

/** 将选中库存整盘搬运到人工指定的入库库位，任务模板由后端AGV配置指定。 */
export function createInboundReturnTask(input: InboundReturnTaskInput): Promise<InboundReturnTaskResult> {
  return defHttp.post<InboundReturnTaskResult>(
    { url: '/wms/stock/createInboundReturnTask', data: input },
    { isTransformResponse: false },
  );
}
