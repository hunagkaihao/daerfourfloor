import { FormSchema } from '/@/components/Table';
import { BasicColumn } from '/@/components/Table';
import { SelectItem } from '/@/utils/SelectItem';
import moment from 'moment';
import {
    AgvTaskServiceProxy,
    AgvTaskStatus,
    ManageType,
} from '/@/services/ServiceProxies';
import { message } from 'ant-design-vue';
import { useLoading } from '/@/components/Loading';
import { useI18n } from '/@/hooks/web/useI18n';
import { createVNode } from 'vue';
import { Modal } from 'ant-design-vue';
import { ExclamationCircleOutlined } from '@ant-design/icons-vue';

const { t } = useI18n();
const [openFullLoading, closeFullLoading] = useLoading({
  tip: 'Loading...',
});

const _AgvTaskServiceProxy = new AgvTaskServiceProxy();

// AGV任务状态选项
export const agvTaskStatusSelectItem: SelectItem[] = [
  {
    label: '被创建',
    value: 0,
    key: 0,
  },
  {
    label: '等待执行',
    value: 1,
    key: 1,
  },
  {
    label: '执行中',
    value: 2,
    key: 2,
  },
  {
    label: '任务开始',
    value: 3,
    key: 3,
  },
  {
    label: '任务完成',
    value: 9,
    key: 9,
  },
  {
    label: '调度删除任务',
    value: 10,
    key: 10,
  },
];

// 管理类型选项
export const manageTypeSelectItem: SelectItem[] = [
  {
    label: '入库',
    value: ManageType._0,
    key: 0,
  },
  {
    label: '出库',
    value: ManageType._1,
    key: 1,
  },
  {
    label: '移库',
    value: ManageType._2,
    key: 2,
  },
];

// 表格列配置
export const tableColumns: BasicColumn[] = [
  {
    title: t('routes.stockTask.agvTaskManagement_creationTime'),
    dataIndex: 'creationTime',
    width: 180,
    customRender: ({ text }) => {
      return text ? moment(text).format('YYYY-MM-DD HH:mm:ss') : '';
    },
  },
  {
    title: '容器编号',
    dataIndex: 'boxCode',
    width: 120,
  },
  {
    title: t('routes.stockTask.agvTaskManagement_podCode'),
    dataIndex: 'podCode',
    width: 120,
  },
  {
    title: '起始位置',
    dataIndex: 'startPositionCode',
    width: 180,
  },
  {
    title: '目标位置',
    dataIndex: 'endPositionCode',
    width: 180,
  },
  {
    title: t('routes.stockTask.agvTaskManagement_agvTaskStatus'),
    dataIndex: 'agvTaskStatus',
    width: 120,
    customRender: ({ text }) => {
      const statusMap = {
        0: '被创建',
        1: '等待执行',
        2: '执行中',
        3: '任务开始',
        4: '出库',
        5: '等待任务继续',
        6: '等待继续任务响应',
        7: '继续执行',
        8: '等待取消响应',
        9: '任务完成',
        10: '调度删除任务',
        11: '设备错误',
        12: '异常完成',
      };
      return statusMap[text] || '未知';
    },
  },
  {
    title: '任务类型',
    dataIndex: 'taskTyp',
    width: 100,
  },
  {
    title: '搬运开始时间',
    dataIndex: 'taskStartTime',
    width: 180,
    customRender: ({ text }) => {
      return text ? moment(text).format('YYYY-MM-DD HH:mm:ss') : '';
    },
  },
  {
    title: '搬运结束时间',
    dataIndex: 'lastModificationTime',
    width: 180,
    customRender: ({ text }) => {
      return text ? moment(text).format('YYYY-MM-DD HH:mm:ss') : '';
    },
  },
  {
    title: '操作',
    dataIndex: 'operation',
    width: 100,
    fixed: 'right',
    slots: { customRender: 'operation' },
  },
];

// 搜索表单配置
export const searchFormSchema: FormSchema[] = [
  {
    field: 'agvTaskStatus',
    label: t('routes.stockTask.agvTaskManagement_agvTaskStatus'),
    component: 'Select',
    colProps: { span: 6 },
    componentProps: {
      options: agvTaskStatusSelectItem,
      placeholder: '请选择任务状态',
    },
  },
  {
    field: 'podCode',
    label: t('routes.stockTask.agvTaskManagement_podCode'),
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'boxCode',
    label: '容器编号',
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'startPositionCode',
    label: '起点位置',
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'endPositionCode',
    label: '终点位置',
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'creationTime',
    component: 'RangePicker',
    label: '创建时间',
    labelWidth: 80,
    colProps: { span: 6 },
    defaultValue: [moment().subtract(7, 'days'), moment().add(1, 'days')],
  },
];

/**
 * 分页获取AGV任务列表
 * @param params
 * @returns
 */
export async function getAgvTaskListAsync(params: any): Promise<any> {
  try {
    // 处理时间范围参数
    if (params.creationTime && params.creationTime.length === 2) {
      // 开始时间设置为当天的 00:00:00
      params.creationTimeStart = moment(params.creationTime[0]).startOf('day').format('YYYY-MM-DD HH:mm:ss');
      // 结束时间设置为当天的 23:59:59
      params.creationTimeEnd = moment(params.creationTime[1]).endOf('day').format('YYYY-MM-DD HH:mm:ss');
      delete params.creationTime;
    }
    
    // 如果直接有creationTimeStart和creationTimeEnd参数，也要确保格式正确
    if (params.creationTimeStart) {
      params.creationTimeStart = moment(params.creationTimeStart).startOf('day').format('YYYY-MM-DD HH:mm:ss');
    }
    if (params.creationTimeEnd) {
      params.creationTimeEnd = moment(params.creationTimeEnd).endOf('day').format('YYYY-MM-DD HH:mm:ss');
    }

    // 清理空值
    Object.keys(params).forEach(key => {
      if (params[key] === '' || params[key] === null || params[key] === undefined) {
        delete params[key];
      }
    });

    console.log('发送给后端的参数:', params);
    return await _AgvTaskServiceProxy.pagedList(params);
  } catch (error) {
    console.error('获取AGV任务列表失败:', error);
    throw error;
  }
}

/**
 * 获取所有AGV任务（用于导出）
 * @param params
 * @returns
 */
export async function getAllAgvTasksAsync(params: any): Promise<any> {
  try {
    // 设置大页面大小以获取所有数据
    const exportParams = {
      ...params,
      pageIndex: 1,
      pageSize: 10000,
    };

    return await _AgvTaskServiceProxy.pagedList(exportParams);
  } catch (error) {
    console.error('获取所有AGV任务失败:', error);
    throw error;
  }
}

/**
 * 取消AGV任务
 * @param taskId 任务ID
 * @returns
 */
export async function cancelAgvTask(taskId: number): Promise<any> {
  try {
    return await _AgvTaskServiceProxy.cancel(taskId);
  } catch (error) {
    console.error('取消AGV任务失败:', error);
    throw error;
  }
}
