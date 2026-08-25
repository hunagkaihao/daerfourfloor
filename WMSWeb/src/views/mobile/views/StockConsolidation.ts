import { defHttp } from '/@/utils/http/axios';

/**
 * 启动四楼库存整理后台线程。
 */
export function startStockConsolidation(): Promise<any> {
  return defHttp.post(
    { url: '/wms/stock-consolidation/start' },
    { isTransformResponse: false },
  );
}

/**
 * 请求安全停止四楼库存整理后台线程。
 * 已经下发的AGV任务不会被取消。
 */
export function stopStockConsolidation(): Promise<any> {
  return defHttp.post(
    { url: '/wms/stock-consolidation/stop' },
    { isTransformResponse: false },
  );
}

/**
 * 查询四楼库存整理线程当前状态。
 */
export function getStockConsolidationStatus(): Promise<any> {
  return defHttp.get(
    { url: '/wms/stock-consolidation/status' },
    { isTransformResponse: false },
  );
}
