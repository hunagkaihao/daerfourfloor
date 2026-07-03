import type { AppRouteModule } from '/@/router/types';
import { LAYOUT } from '/@/router/constant';
import { t } from '/@/hooks/web/useI18n';

const reportStatistics: AppRouteModule = {
  path: '/reportStatistics',
  name: 'ReportStatistics',
  component: LAYOUT,
  meta: {
    orderNo: 40,
    icon: 'ant-design:bar-chart-outlined',
    title: t('报表统计管理'),
  },
  children: [
    {
      path: 'materialStockStatistics',
      name: 'MaterialStockStatistics',
      component: () => import('/@/views/task/statistics/MaterialStockStatistics.vue'),
      meta: {
        title: t('仓库存料统计报表'),
        policy: 'Wms.Read',
        icon: 'ant-design:bar-chart-outlined',
        //ignoreKeepAlive: true, //忽略页面缓存，每次强制刷新
      },
    },
    {
      path: 'workshopReceiptStatistics',
      name: 'WorkshopReceiptStatistics',
      component: () => import('/@/views/task/statistics/WorkshopReceiptStatistics.vue'),
      meta: {
        title: t('车间收料统计报表'),
        policy: 'Wms.Read',
        icon: 'ant-design:shop-outlined',
        //ignoreKeepAlive: true, //忽略页面缓存，每次强制刷新
      },
    },
  ],
};

export default reportStatistics;
