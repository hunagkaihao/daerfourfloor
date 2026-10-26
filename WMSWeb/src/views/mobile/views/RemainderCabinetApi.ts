import { defHttp } from '/@/utils/http/axios';

/** 单次零头入柜的不可变请求，网络重试必须沿用同一requestId和原始数据。 */
export interface RemainderCabinetEntry {
  /** 本次登记标识，同时用作Stocks主键。 */
  requestId: string;
  /** 送货单对应ASN号。 */
  asnCode: string;
  /** 明确选定的本地ASN来源明细，订单和批次由此确定。 */
  asnLineId: string;
  /** A01-01至L15-08，例如A15-08。 */
  cellCode: string;
  /** 原标识卡，第1段物料、第4段等级，其余空字段不推断批次或数量。 */
  cardBarcode: string;
  /** 实际入柜数量，单位同ASN。 */
  quantity: number;
}

/** 统一库存表中按批次及来源分别保存的零头库存。 */
export interface RemainderCabinetStock {
  stockId: string;
  asnCode?: string;
  orderCode?: string;
  materialCode: string;
  materialName?: string;
  batchCode?: string;
  grade?: string;
  quantity: number;
  status: string;
  /** 实际柜位。 */
  cellCode?: string;
  /** 计量单位。 */
  unit?: string;
  /** 当前记录是否允许领用。 */
  canWithdraw?: boolean;
}

/** 零头格位固定物料与实时库存，库存清空不解除格位绑定。 */
export interface RemainderCabinet {
  cellCode: string;
  warehouseName: string;
  canStore: boolean;
  materialCode?: string;
  grade?: string;
  stocks: RemainderCabinetStock[];
}

/** 扫描柜位后读取固定物料及当前库存，不创建库位或库存。 */
export function getRemainderCabinet(cellCode: string) {
  return defHttp.get<RemainderCabinet>(
    { url: '/wms/stock/remainder-cabinet', params: { cellCode: cellCode.trim() } },
    { isTransformResponse: false },
  );
}

/** 将合格ASN剩余数量直接入柜，后端在一个事务内更新统一库存、ASN及入库流水。 */
export function storeAsnRemainder(input: RemainderCabinetEntry) {
  return defHttp.post<{ success: boolean; message?: string }>(
    { url: '/wms/stock/asn-remainder-entry', data: input },
    { isTransformResponse: false },
  );
}

/** 独立零头柜操作，仅保存存货、等级、柜位和数量；requestId用于网络重试去重。 */
export interface RemainderProductOperation {
  /** 同一笔操作的稳定标识。 */
  requestId: string;
  /** 存货编码。 */
  materialCode: string;
  /** 物料等级。 */
  grade: string;
  /** 实际柜位。 */
  cellCode: string;
  /** 实际操作数量，单位同物料。 */
  quantity: number;
  /** 箱码批次，仅寄存时写入流水；新领用请求不传此项，服务端忽略旧客户端的兼容字段。 */
  batchCode?: string;
}

/** 同存货、同等级、同柜位的当前库存合计。 */
export interface RemainderBalance {
  /** 稳定的汇总行标识。 */
  id: string;
  /** 存货编码。 */
  materialCode: string;
  /** 主数据中的存货名称。 */
  materialName?: string;
  /** 实际柜位。 */
  cellCode: string;
  /** 等级。 */
  grade: string;
  /** 当前在柜总数量。 */
  quantity: number;
  /** 排除冻结及占用后的可领用数量。 */
  availableQuantity: number;
  /** 计量单位。 */
  unit?: string;
}

/** 寄存或领用的一次实际操作流水。 */
export interface RemainderHistory {
  /** 流水唯一行标识。 */
  id: string;
  /** 寄存或领用。 */
  operation: string;
  /** 实际操作时间。 */
  operationTime: string;
  /** 存货编码。 */
  materialCode: string;
  /** 存货名称。 */
  materialName?: string;
  /** 实际货位。 */
  cellCode: string;
  /** 等级。 */
  grade?: string;
  /** 寄存箱码中的批次号；领用和历史未采集批次的寄存记录为空。 */
  batchCode?: string;
  /** 本次操作的正数量，方向由operation区分。 */
  quantity: number;
  /** 计量单位。 */
  unit?: string;
  /** 操作人员。 */
  operatorName?: string;
}

/** 库存和流水共用的可选查询条件。 */
export interface RemainderQuery {
  /** 扫描纯存货码、物料卡或柜位。 */
  scanCode?: string;
  /** 存货编码片段。 */
  materialCode?: string;
  /** 货位编码片段。 */
  cellCode?: string;
  /** 等级片段。 */
  grade?: string;
  /** yyyy、yyyy-MM或yyyy-MM-dd。 */
  dateKeyword?: string;
  /** 仅过滤寄存流水的批次号片段，填写后不返回领用流水。 */
  batchCode?: string;
  /** 分页偏移。 */
  skipCount?: number;
  /** 单页条数，最多200。 */
  maxResultCount?: number;
}

/** 独立寄存，不传递或绑定ASN。 */
export function depositRemainder(input: RemainderProductOperation) {
  return defHttp.post<{ success: boolean; message?: string }>(
    { url: '/wms/stock/remainder-deposit', data: input }, { isTransformResponse: false },
  );
}

/** 按存货、等级和柜位领用可用库存。 */
export function withdrawRemainder(input: RemainderProductOperation) {
  return defHttp.post<{ success: boolean; message?: string }>(
    { url: '/wms/stock/remainder-withdraw', data: input }, { isTransformResponse: false },
  );
}

/** 查询零头柜汇总库存，不包含该仓库的其他库位。 */
export function queryRemainderStocks(input: RemainderQuery) {
  return defHttp.get<{ items: RemainderBalance[]; totalCount: number }>(
    { url: '/wms/stock/remainder-stocks', params: input }, { isTransformResponse: false },
  );
}

/** 按可选条件模糊查询寄存及领用流水。 */
export function queryRemainderHistories(input: RemainderQuery) {
  return defHttp.get<{ items: RemainderHistory[]; totalCount: number }>(
    { url: '/wms/stock/remainder-histories', params: input }, { isTransformResponse: false },
  );
}

/** 按固定绑定查柜位，尚无库存的位置同样返回；不按批次筛选。 */
export function getRemainderLocations(materialCode: string) {
  return defHttp.get<RemainderCabinet[]>(
    { url: '/wms/stock/remainder-locations', params: { materialCode } }, { isTransformResponse: false },
  );
}

/** 固定物料与库位的对应关系，解绑时提交页面读取的原物料编码。 */
export interface RemainderLocationBinding {
  /** A至L柜的大写标准格位。 */
  cellCode: string;
  /** 绑定的新编码，或解绑前的原编码。 */
  materialCode: string;
}

/** 绑定物料；已存在其他物料时须先解绑，标准格位缺失时在零头柜区补建。 */
export function bindRemainderLocation(input: RemainderLocationBinding) {
  return defHttp.post<{ success: boolean; message?: string }>(
    { url: '/wms/stock/remainder-location-bind', data: input }, { isTransformResponse: false },
  );
}

/** 只解绑空柜位，服务端校验现存库存与占用。 */
export function unbindRemainderLocation(input: RemainderLocationBinding) {
  return defHttp.post<{ success: boolean; message?: string }>(
    { url: '/wms/stock/remainder-location-unbind', data: input }, { isTransformResponse: false },
  );
}
