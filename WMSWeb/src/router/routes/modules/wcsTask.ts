import type { AppRouteModule } from '/@/router/types';
import { LAYOUT } from '/@/router/constant';
import { t } from '/@/hooks/web/useI18n';

const wcsTask: AppRouteModule = {
  path: '/wcsTask',
  name: 'WcsTask',
  component: LAYOUT,
  //redirect: '/admin/abpUser',
  meta: {
    orderNo: 51,
    icon: 'ant-design:robot-outlined', 
    title: t('设备任务管理'),
  },
  children: [
    {
      path: 'agvTask',
      name: 'AgvTask',
      component: () => import('/@/views/task/agv/AgvTaskManagement.vue'),
      meta: {
        title: t('AGV任务管理'),
        policy: 'Wms.Read',
        icon: 'ant-design:robot-outlined',
      },
    },
  ],
};

export default wcsTask;
