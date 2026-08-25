import type { AppRouteModule } from '/@/router/types';
import { LAYOUT } from '/@/router/constant';
import { t } from '/@/hooks/web/useI18n';

const Inbound: AppRouteModule = {
  path: '/Inbound',
  name: 'Inbound',
  component: LAYOUT,
  //redirect: '/admin/abpUser',
  meta: {
    orderNo: 29,
    icon: 'ant-design:swap-outlined',
    title: t('入库管理'),
  },
  children: [
    // {
    //   path: 'createtask',
    //   name: 'Createtask',
    //   component: () => import('/@/views/task/tasks/task.vue'),
    //   meta: {
    //     title: t('无计划领用'),
    //     policy: 'Wms.Read',
    //     icon: 'ant-design:file-search-outlined',
    //   },
    // },
    // {
    //   path: 'incellHis',
    //   name: 'IncellHis',
    //   component: () => import('/@/views/task/historys/IncellHis.vue'),
    //   meta: {
    //     title: t('入库历史记录'),
    //     policy: 'Wms.Read',
    //     icon: 'ant-design:select-outlined',
    //   },
    // },
    // {
    //   path: 'outcellHis',
    //   name: 'OutcellHis',
    //   component: () => import('/@/views/task/historys/OutcellHis.vue'),
    //   meta: {
    //     title: t('出库历史记录'),
    //     policy: 'Wms.Read',
    //     icon: 'ant-design:select-outlined',
    //   },
    // },
    // {
    //   path: 'barcodeList',
    //   name: 'barcodeList',
    //   component: () => import('/@/views/warehouse/goods/BarcodeList.vue'),
    //   meta: {
    //     title: t('到货单管理'),
    //     policy: 'Wms.Read',
    //     icon: 'ant-design:file-text-outlined',
    //   },
    // },
    // {
    //   path: 'linliaoOrder',
    //   name: 'linliaoorder',
    //   component: () => import('/@/views/warehouse/goods/linliaoOrder.vue'),
    //   meta: {
    //     title: t('领料单管理'),
    //     policy: 'Wms.Read',
    //     icon: 'ant-design:select-outlined',
    //     //ignoreKeepAlive: true, //忽略页面缓存，每次强制刷新
    //   },
    // },
  ],
};

export default Inbound;
