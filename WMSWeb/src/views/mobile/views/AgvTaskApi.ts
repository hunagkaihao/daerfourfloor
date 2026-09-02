import { defHttp } from '/@/utils/http/axios';

/**
 * 获取AGV任务分页列表
 * @param params 查询参数
 * @returns
 */
export async function getAgvTaskList(params: {
  pageIndex: number;
  pageSize: number;
  agvTaskStatus?: number;
}): Promise<any> {
  return defHttp.post(
    {
      url: '/wms/agvtask/paged-list',
      data: params,
    },
    { isTransformResponse: false }
  );
}

/**
 * 取消AGV任务
 * @param taskId 任务ID
 * @returns
 */
export async function cancelAgvTask(taskId: number): Promise<any> {
  return defHttp.post(
    {
      url: `/wms/agvtask/cancel`,
      params: { taskId },
    },
    { isTransformResponse: false }
  );
}
