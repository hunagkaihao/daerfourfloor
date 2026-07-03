import type { AppRouteModule } from '/@/router/types';
import { LAYOUT } from '/@/router/constant';
import { t } from '/@/hooks/web/useI18n';

const warehouse: AppRouteModule = {
  path: '/warehouse',
  name: 'Warehouse',
  component: LAYOUT,
  //redirect: '/admin/abpUser',
  meta: {
    orderNo: 30,
    icon: 'ant-design:home-outlined',
    title: t('仓库管理'),
    ignoreAuth: true,
  },
  children: [
    {
      path: 'ware',
      name: 'Ware',
      component: () => import('/@/views/warehouse/wares/Ware.vue'),
      meta: {
        title: t('仓库数据'),
        policy: 'Wms.Read',
        icon: 'ant-design:home-outlined',
        ignoreAuth: true,
      },
    },
    {
      path: 'cell',
      name: 'Cell',
      component: () => import('/@/views/warehouse/cells/Cell.vue'),
      meta: {
        title: t('routes.warehouse.cellManagement'),
        policy: 'Wms.Read',
        icon: 'material-symbols:approval-outline',
      },
    },
    {
      path: 'storageBox',
      name: 'StorageBox',
      component: () => import('/@/views/warehouse/boxs/StorageBox.vue'),
      meta: {
        title: t('routes.warehouse.storageBoxManagement'),
        policy: 'Wms.Read',
        icon: 'ant-design:inbox-outlined',
      },
    },
    {
      path: 'outstock',
      name: 'Outstock',
      component: () => import('/@/views/warehouse/outstock/Outstock.vue'),
      meta: {
        title: '出库管理',
        policy: 'Wms.Read',
        icon: 'ant-design:export-outlined',
        ignoreAuth: true,
      },
    },
    {
      path: 'outbound-order',
      name: 'OutboundOrder',
      component: () => import('/@/views/warehouse/outbound-order/OutboundOrder.vue'),
      meta: {
        title: '出库单管理',
        policy: 'Wms.Read',
        icon: 'ant-design:file-text-outlined',
        ignoreAuth: true,
      },
    },
  ],
};

export default warehouse;
