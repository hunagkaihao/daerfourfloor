import { defHttp } from '/@/utils/http/axios';

/** 一笔实物盘点的原始输入；网络重试沿用原始数据及requestId。 */
export interface InventoryCountInput {
  /** 提交标识，用于服务端去重。 */
  requestId: string;
  /** 原始物料码，服务端重新解析物料、产商和等级。 */
  barcode: string;
  /** 整箱只数，可为空；填写时为正整数。 */
  quantityPerBox: number | null;
  /** 实际箱数，可为空；填写时为非负整数。 */
  actualBoxes: number | null;
  /** 实际零头只数，可为空；填写时为非负整数。 */
  remainderQuantity: number | null;
}

/** 保存一笔盘点明细，不向服务端传递客户端计算的总数。 */
export function saveInventoryCount(input: InventoryCountInput) {
  return defHttp.post<{ success: boolean; message?: string }>(
    { url: '/wms/inventory-count', data: input }, { isTransformResponse: false },
  );
}

/** 获取指定保存日期的完整xlsx文件；无数据或日期错误由服务端提示。 */
export function exportInventoryCounts(date: string) {
  return defHttp.get<Blob>(
    { url: '/wms/inventory-count/export', params: { date }, responseType: 'blob' },
    { isTransformResponse: false },
  );
}
