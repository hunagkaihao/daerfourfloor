import type { AppRouteModule } from '/@/router/types';
import { LAYOUT } from '/@/router/constant';
import { t } from '/@/hooks/web/useI18n';

const stockTask: AppRouteModule = {
  path: '/stockTask',
  name: 'StockTask',
  component: LAYOUT,
  //redirect: '/admin/abpUser',
  meta: {
    orderNo: 35,
    icon: 'ant-design:swap-outlined',
    title: t('routes.stockTask.stockTaskManagement'),
  },
  children: [
    {
      path: 'createtask',
      name: 'Createtask',
      component: () => import('/@/views/task/tasks/task.vue'),
      meta: {
        title: t('无计划领用'),
        policy: 'Wms.Read',
        icon: 'ant-design:file-search-outlined',
        //ignoreKeepAlive: true, //忽略页面缓存，每次强制刷新
      },
    },
    {
      path: 'incellHis',
      name: 'IncellHis',
      component: () => import('/@/views/task/historys/IncellHis.vue'),
      meta: {
        title: t('入库历史记录'),
        policy: 'Wms.Read',
        icon: 'ant-design:select-outlined',
        //ignoreKeepAlive: true, //忽略页面缓存，每次强制刷新
      },
    },
    {
      path: 'outcellHis',
      name: 'OutcellHis',
      component: () => import('/@/views/task/historys/OutcellHis.vue'),
      meta: {
        title: t('出库历史记录'),
        policy: 'Wms.Read',
        icon: 'ant-design:select-outlined',
        //ignoreKeepAlive: true, //忽略页面缓存，每次强制刷新
      },
    },
    {
      path: 'barcodeList',
      name: 'barcodeList',
      component: () => import('/@/views/warehouse/goods/BarcodeList.vue'),
      meta: {
        title: t('到货单管理'),
        policy: 'Wms.Read',
        icon: 'ant-design:file-text-outlined',
        //ignoreKeepAlive: true, //忽略页面缓存，每次强制刷新
      },
    },
    {
      path: 'linliaoOrder',
      name: 'linliaoorder',
      component: () => import('/@/views/warehouse/goods/linliaoOrder.vue'),
      meta: {
        title: t('领料单管理'),
        policy: 'Wms.Read',
        icon: 'ant-design:select-outlined',
        //ignoreKeepAlive: true, //忽略页面缓存，每次强制刷新
      },
    },
    // {
    //   path: 'taskHis',
    //   name: 'TaskHis',
    //   component: () => import('/@/views/warehouse/taskHiss/TaskHis.vue'),
    //   meta: {
    //     title: t('routes.stockTask.taskHisManagement'),
    //     policy: 'WarehouseManagement.StockTaskManagement',
    //     icon: 'ant-design:file-search-outlined',
    //   },
    // },
   
  ],
};

export default stockTask;
