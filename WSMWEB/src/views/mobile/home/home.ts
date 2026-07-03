import {
    PickListServiceProxy,
    RecheckListServiceProxy
  } from '/@/services/ServiceProxies';
  const _PickNotifierServiceProxy = new PickListServiceProxy();
  const _RecheckListServiceProxy = new RecheckListServiceProxy();
  export function pickItemsCnt(
  ): Promise<any> {
    return _PickNotifierServiceProxy.pickItemsCnt();
  }
  export function recheckItemsCount(
  ): Promise<any> {
    return _RecheckListServiceProxy.recheckItemsCount();
  }
