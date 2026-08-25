import type { AppRouteModule } from '/@/router/types';
import { LAYOUT } from '/@/router/constant';
import { t } from '/@/hooks/web/useI18n';

const material: AppRouteModule = {
  path: '/material',
  name: 'Material',
  component: LAYOUT,
  //redirect: '/admin/abpUser',
  meta: {
    orderNo: 20,
    icon: 'ant-design:profile-outlined',
    title: t('routes.material.materialManagement'),
  },
  children: [
    {
      path: 'basic-data',
      name: 'MaterialBasicData',
      component: () => import('/@/views/warehouse/materialBasic/MaterialBasic.vue'),
      meta: {
        orderNo: 1,
        title: t('routes.material.basicDataManagement'),
        policy: 'Wms.Read',
        icon: 'ant-design:database-outlined',
      },
    },
    {
      path: 'goods',
      name: 'MaterialGoods',
      component: () => import('/@/views/warehouse/goods/Good.vue'),
      meta: {
        orderNo: 2,
        title: t('routes.material.goodsManagement'),
        policy: 'Wms.Read',
        icon: 'ant-design:profile-outlined',
      },
    },
  ],
};

export default material;
