import { defHttp } from '/@/utils/http/axios';
import { getToken } from '/@/utils/auth';

/**
 * 库存整理接口使用ASP.NET Core JwtBearer认证，必须显式添加Bearer前缀。
 * 同时关闭全局拦截器的原始Token注入，避免自定义请求头被覆盖。
 */
function getStockConsolidationRequestOptions() {
  const token = getToken();
  return {
    request: {
      headers: token ? { Authorization: `Bearer ${token}` } : {},
    },
    options: {
      isTransformResponse: false,
      withToken: false,
    },
  };
}

/**
 * 启动四楼库存整理后台线程。
 */
export function startStockConsolidation(): Promise<any> {
  const auth = getStockConsolidationRequestOptions();
  return defHttp.post(
    { url: '/wms/stock-consolidation/start', ...auth.request },
    auth.options,
  );
}

/**
 * 请求安全停止四楼库存整理后台线程。
 * 已经下发的AGV任务不会被取消。
 */
export function stopStockConsolidation(): Promise<any> {
  const auth = getStockConsolidationRequestOptions();
  return defHttp.post(
    { url: '/wms/stock-consolidation/stop', ...auth.request },
    auth.options,
  );
}

/**
 * 查询四楼库存整理线程当前状态。
 */
export function getStockConsolidationStatus(): Promise<any> {
  const auth = getStockConsolidationRequestOptions();
  return defHttp.get(
    { url: '/wms/stock-consolidation/status', ...auth.request },
    auth.options,
  );
}
