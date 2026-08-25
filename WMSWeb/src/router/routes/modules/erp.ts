import type { AppRouteModule } from '/@/router/types';
import { LAYOUT } from '/@/router/constant';
import { t } from '/@/hooks/web/useI18n';

const erp: AppRouteModule = {
  path: '/erp',
  name: 'Erp',
  component: LAYOUT,
  meta: {
    orderNo: 50,
    icon: 'ant-design:database-outlined',
    title: t('ERP'),
    ignoreAuth: true,
  },
  children: [
    {
      path: 'asnList',
      name: 'AsnList',
      component: () => import('/@/views/erp/AsnList.vue'),
      meta: {
        title: t('ASN列表'),
        icon: 'ant-design:unordered-list-outlined',
        ignoreAuth: true,
      },
    },
  ],
};

export default erp;
