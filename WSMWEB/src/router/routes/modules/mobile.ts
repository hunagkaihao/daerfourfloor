import type { AppRouteModule } from '/@/router/types';
import { LAYOUT } from '/@/router/constant';
import { t } from '/@/hooks/web/useI18n';

const mobile: AppRouteModule = {
  path: '/mobile',
  name: 'Mobile',
  component: LAYOUT,
  meta: {
    orderNo: 45,
    icon: 'ant-design:mobile-outlined',
    title: t('移动端'),
    ignoreAuth: true,
  },
  children: [
    {
      path: 'pda',
      name: 'PDA',
      component: () => import('/@/views/warehouse/pda/MobileBrowser.vue'),
      meta: {
        title: t('routes.mobile.pda'),
        policy: 'Wms.Read',
        icon: 'ant-design:mobile-outlined',
        ignoreAuth: true,
      },
    },
    {
      path: 'stockQuery',
      name: 'StockQuery',
      component: () => import('/@/views/mobile/views/StockQuery.vue'),
      meta: {
        title: '库存查询',
        ignoreAuth: true,
      },
    },
  ],
};

export default mobile;
