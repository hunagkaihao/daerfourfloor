import type { AppRouteModule } from '/@/router/types';
import { LAYOUT } from '/@/router/constant';
import { t } from '/@/hooks/web/useI18n';

const skip: AppRouteModule = {
  path: '/skip',
  name: 'Skip',
  component: LAYOUT,
  meta: {
    orderNo: 40,
    icon: 'ant-design:car-outlined',
    title: t('routes.skip.skipManagement'),
  },
  children: [
    // 料车数据已移动到仓库管理模块下
  ],
};

export default skip;
