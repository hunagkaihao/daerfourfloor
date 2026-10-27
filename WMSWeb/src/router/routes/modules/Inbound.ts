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
  ],
};

export default Inbound;
