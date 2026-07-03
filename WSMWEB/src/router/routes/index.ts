import type { AppRouteRecordRaw, AppRouteModule } from '/@/router/types';

import { PAGE_NOT_FOUND_ROUTE, REDIRECT_ROUTE } from '/@/router/routes/basic';
import { mainOutRoutes } from './mainOut';
import { PageEnum } from '/@/enums/pageEnum';
import { t } from '/@/hooks/web/useI18n';

const modules = import.meta.globEager('./modules/**/*.ts');

const routeModuleList: AppRouteModule[] = [];

Object.keys(modules).forEach((key) => {
  const mod = modules[key].default || {};
  const modList = Array.isArray(mod) ? [...mod] : [mod];
  routeModuleList.push(...modList);
});

export const asyncRoutes = [PAGE_NOT_FOUND_ROUTE, ...routeModuleList];

export const RootRoute: AppRouteRecordRaw = {
  path: '/',
  name: 'Root',
  redirect: PageEnum.BASE_HOME,
  meta: {
    title: 'Root',
  },
};

export const LoginRoute: AppRouteRecordRaw = {
  path: '/login',
  name: 'Login',
  component: () => import('/@/views/sys/login/Login.vue'),
  meta: {
    title: t('routes.basic.login'),
  },
};
//新增移动端登录
export const MobileLoginRoute: AppRouteRecordRaw = {
  path: '/mobilelogin',
  name: 'MobileLogin',
  component: () => import('/@/views/mobile/login/Login.vue'),
  meta: {
    title: t('routes.basic.login'),
    ignoreAuth: true,
  },
};

//新增移动端主页
export const MobileHomeRoute: AppRouteRecordRaw = {
  path: '/mobilehome',
  name: 'MobileHome',
  component: () => import('/@/views/mobile/home/home.vue'),
  meta: {
    title: t('主页'),
    ignoreAuth: true,
  },
};


//人工入库
export const IncellByPeople: AppRouteRecordRaw = {
  path: '/incellByPeople',
  name: 'IncellByPeople',
  component: () => import('/@/views/mobile/views/IncellByPeople.vue'),
  meta: {
    title: t('人工入库'),
    ignoreAuth: true,
  },
};
//超期复检入库
// IncellByOverdue路由已删除
// IncellByOverdueCall路由已删除
//领用通知
export const AcceptanceCall: AppRouteRecordRaw = {
  path: '/acceptanceCall',
  name: 'AcceptanceCall',
  component: () => import('/@/views/mobile/views/AcceptanceCall.vue'),
  meta: {
    title: t('领用通知'),
    ignoreKeepAlive: true,
    ignoreAuth: true,
  },
};
//检验入库领用通知
export const AcceptanceCall2: AppRouteRecordRaw = {
  path: '/acceptanceCall2',
  name: 'AcceptanceCall2',
  component: () => import('/@/views/mobile/views/AcceptanceCall2.vue'),
  meta: {
    title: t('检验入库领用通知'),
    ignoreKeepAlive: true,
    ignoreAuth: true,
  },
};

//库存查询
export const cellStock: AppRouteRecordRaw = {
  path: '/cellStock',
  name: 'cellStock',
  component: () => import('/@/views/mobile/views/Stock.vue'),
  meta: {
    title: t('库存查询'),
    ignoreAuth: true,
  },
};
//agv托盘组盘入库
export const AGVIncell: AppRouteRecordRaw = {
  path: '/agvIncell',
  name: 'AGVIncell',
  component: () => import('/@/views/mobile/views/AGVIncell.vue'),
  meta: {
    title: t('agv托盘组盘入库'),
    ignoreAuth: true,
  },
};

//容器绑定
export const BoxBind: AppRouteRecordRaw = {
  path: '/boxBind',
  name: 'BoxBind',
  component: () => import('/@/views/mobile/views/BoxBind.vue'),
  meta: {
    title: t('容器绑定'),
    ignoreAuth: true,
  },
};
// GoodAndBoxBind路由已删除
//容器组盘
export const BoxDisk: AppRouteRecordRaw = {
  path: '/boxDisk',
  name: 'BoxDisk',
  component: () => import('/@/views/mobile/views/BoxDisk.vue'),
  meta: {
    title: t('容器组盘'),
    ignoreAuth: true,
  },
};
//容器组盘(ASN校验)
export const BoxDiskWithAsn: AppRouteRecordRaw = {
  path: '/boxDiskWithAsn',
  name: 'BoxDiskWithAsn',
  component: () => import('/@/views/mobile/views/BoxDiskWithAsn.vue'),
  meta: {
    title: t('容器组盘(ASN)'),
    ignoreAuth: true,
  },
};
//容器入库
export const BoxIncell: AppRouteRecordRaw = {
  path: '/boxIncell',
  name: 'BoxIncell',
  component: () => import('/@/views/mobile/views/BoxIncell.vue'),
  meta: {
    title: t('容器入库'),
    ignoreAuth: true,
  },
};
//物料抽检
// GoodSpotCheck路由已删除
// EmptyShelfEdit路由已删除
// GoodsDevan路由已删除
// GoodsAdd路由已删除
// EmptyBoxOut路由已删除
// BindStation路由已删除
// CreateAgvTask路由已删除
// CreateAgvBackTask路由已删除
// ZZOutCell路由已删除
//领用物料绑定
export const GoodsBind: AppRouteRecordRaw = {
  path: '/goodsBind',
  name: 'GoodsBind',
  component: () => import('/@/views/mobile/views/GoodsBind.vue'),
  meta: {
    title: t('领用物料绑定'),
    ignoreAuth: true,
  },
};
// ShelfIncellList路由已删除
//整车入库
// ShelfIncell路由已删除
// Tiaobo路由已删除

// Handincell路由已删除
export const Handoutcell: AppRouteRecordRaw = {
  path: '/handoutcell',
  name: 'HandOutCell',
  component: () => import('/@/views/mobile/views/HandOutCell.vue'),
  meta: {
    title: t('物料调拨'),
    ignoreAuth: true,
  },
};
// Boxoutcell路由已删除
// DPTiaobo路由已删除
// DPCall路由已删除
//检验查询
export const CheckNoGet: AppRouteRecordRaw = {
  path: '/checkNoGet',
  name: 'CheckNoGet',
  component: () => import('/@/views/mobile/views/CheckNoGet.vue'),
  meta: {
    title: t('整车清单'),
    ignoreAuth: true,
  },
};
//领用通知
export const AcceptanceCalltest: AppRouteRecordRaw = {
  path: '/acceptanceCalltest',
  name: 'AcceptanceCalltest',
  component: () => import('/@/views/mobile/views/AcceptanceCalltest.vue'),
  meta: {
    title: t('领用通知测试'),
    ignoreKeepAlive: true,
    ignoreAuth: true,
  },
};
//创建WMS任务
export const CreateStockTask: AppRouteRecordRaw = {
  path: '/createStockTask',
  name: 'CreateStockTask',
  component: () => import('/@/views/mobile/views/CreateStockTask.vue'),
  meta: {
    title: t('创建WMS任务'),
    ignoreKeepAlive: true,
    ignoreAuth: true,
  },
};
//创建WMS任务
export const CreateOutStockTask: AppRouteRecordRaw = {
  path: '/createOutStockTask',
  name: 'CreateOutStockTask',
  component: () => import('/@/views/mobile/views/CreateOutStockTask.vue'),
  meta: {
    title: t('创建WMS任务'),
    ignoreKeepAlive: true,
    ignoreAuth: true,
  },
};



//扫描ASN(移动端)
export const ScanAsn: AppRouteRecordRaw = {
  path: '/scanAsn',
  name: 'ScanAsn',
  component: () => import('/@/views/mobile/views/asn/ScanAsn.vue'),
  meta: {
    title: t('扫描ASN'),
    ignoreAuth: true,
  },
};

//测试扫描ASN(移动端)
export const TestScanAsn: AppRouteRecordRaw = {
  path: '/testScanAsn',
  name: 'TestScanAsn',
  component: () => import('/@/views/mobile/views/asn/TestScanAsn.vue'),
  meta: {
    title: t('测试扫描ASN'),
    ignoreAuth: true,
  },
};

//出库单出库(移动端)
export const OutStockOrder: AppRouteRecordRaw = {
  path: '/outStockOrder',
  name: 'OutStockOrder',
  component: () => import('/@/views/mobile/views/OutStockOrder.vue'),
  meta: {
    title: t('出库单出库'),
    ignoreAuth: true,
  },
};

//发货单扫码(移动端)
export const DeliveryOrderScan: AppRouteRecordRaw = {
  path: '/deliveryOrderScan',
  name: 'DeliveryOrderScan',
  component: () => import('/@/views/mobile/views/DeliveryOrderScan.vue'),
  meta: {
    title: t('发货单扫码'),
    ignoreAuth: true,
  },
};

//出库分拣(移动端)
export const OutStockSort: AppRouteRecordRaw = {
  path: '/outStockSort',
  name: 'OutStockSort',
  component: () => import('/@/views/mobile/views/OutStockSort.vue'),
  meta: {
    title: t('出库分拣'),
    ignoreAuth: true,
  },
};

//整箱出库(移动端)
export const MaterialOut: AppRouteRecordRaw = {
  path: '/materialOut',
  name: 'MaterialOut',
  component: () => import('/@/views/mobile/views/MaterialOut.vue'),
  meta: {
    title: t('整箱出库'),
    ignoreAuth: true,
  },
};

//拆箱出库(移动端)
export const MaterialPartialOut: AppRouteRecordRaw = {
  path: '/materialPartialOut',
  name: 'MaterialPartialOut',
  component: () => import('/@/views/mobile/views/MaterialPartialOut.vue'),
  meta: {
    title: t('拆箱出库'),
    ignoreAuth: true,
  },
};

//库存查询(移动端)
export const StockQuery: AppRouteRecordRaw = {
  path: '/stockQuery',
  name: 'StockQuery',
  component: () => import('/@/views/mobile/views/StockQuery.vue'),
  meta: {
    title: t('库存查询'),
    ignoreAuth: true,
  },
};

//创建出库任务(汇总)(移动端)
export const CreateOutStockTaskSummary: AppRouteRecordRaw = {
  path: '/createOutStockTaskSummary',
  name: 'CreateOutStockTaskSummary',
  component: () => import('/@/views/mobile/views/CreateOutStockTaskSummary.vue'),
  meta: {
    title: t('创建出库任务(汇总)'),
    ignoreAuth: true,
  },
};

// Basic routing without permission
export const basicRoutes = [
  LoginRoute,
  MobileLoginRoute,
  MobileHomeRoute,
  RootRoute,
  ...mainOutRoutes,
  REDIRECT_ROUTE,
  PAGE_NOT_FOUND_ROUTE,
  IncellByPeople,
  AcceptanceCall,
  CheckNoGet,
  cellStock,
  AGVIncell,
  CreateStockTask,
  CreateOutStockTask,
  BoxBind,
  BoxDisk,
  BoxDiskWithAsn,
  BoxIncell,
  GoodsBind,
  AcceptanceCall2,
  Handoutcell,
  AcceptanceCalltest,
  ScanAsn,
  TestScanAsn,
  OutStockOrder,
  DeliveryOrderScan,
  OutStockSort,
  MaterialOut,
  MaterialPartialOut,
  StockQuery,
  CreateOutStockTaskSummary,

];
