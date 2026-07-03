import { AxiosRequestConfig, AxiosResponse } from 'axios';
import { message } from 'ant-design-vue';
import { useUserStoreWithOut } from '/@/store/modules/user';
import { router } from '/@/router';
import { PageEnum } from '/@/enums/pageEnum';
import { useI18n } from '/@/hooks/web/useI18n';
import { Modal } from 'ant-design-vue';
import { useLocale } from '/@/locales/useLocale';
import { useGlobSetting } from '/@/hooks/setting';
export class ServiceProxyBase {
  protected transformOptions(options: AxiosRequestConfig) {
    const { apiUrl } = useGlobSetting();
    options.baseURL = apiUrl;
    const guard: boolean = this.urlGuard(options.url as string);
    const userStore = useUserStoreWithOut();

    if (!guard) {
      if (userStore.checkUserLoginExpire) {
        router.replace(PageEnum.BASE_LOGIN);
        return;
      }
      const { token, language } = this.buildRequestMessage();
      // 添加header
      options.headers = {
        'accept-language': language,
        'Content-Type': 'application/json',
        Authorization: 'Bearer ' + token,
        __tenant: userStore.tenantId,
      };
    } else {
      options.headers = {
        'Content-Type': 'application/json',
        __tenant: userStore.tenantId,
      };
    }

    return Promise.resolve(options);
  }
  protected transformResult(
    _url: string,
    response: AxiosResponse,
    processor: (response: AxiosResponse) => Promise<any>
  ): Promise<any> {
    const { t } = useI18n();

    //if (response.status == 401 || response.status == 403 || response.status == 302) {
    //屏蔽403错误
    if (response.status == 401  || response.status == 302) {
      message.error(t('common.authorityText'));
      router.replace(PageEnum.BASE_LOGIN);
    } else if (response.status == 400) {
      Modal.error({
        title: '验证失败',
        content: response.data.error.validationErrors[0].message,
      });
    } else if (response.status >= 500) {
      Modal.error({
        title: '请求异常',
        content: response.data.error.message,
      });
    }

    return processor(response);
  }

  //判决接口是否需要拦截
  private urlGuard(url: string): boolean {

    // if (url.startsWith('/api/abp/application')) {
    //   return true;
    // }
    if (url == '/Tenants/find') {
      return true;
    }
    if (url == '/wms/account/newlogin') {
      return true;
    }
    if (url.startsWith('/api/app/account/login/Sts')) {
      return true;
    }

    //看板页面接口不需要拦截
    if (url == '/GoodsHiss/getDaysSum') {
      return true;
    }
    if (url.startsWith('/SdsCall')) {
      return true;
    }
    if (url == '/GoodsHiss/getTodayCounts') {
      return true;
    }
    if (url == '/AgvTasks/getPLCTaskListByFloor') {
      return true;
    }
    if (url == '/Shelfs/getShelfRoute') {
      return true;
    }
    if (url.startsWith ('/AgvTasks/queryAgvStatus?')) {
      return true;
    }
    if (url.startsWith ('/DataDictionary/getDetailByCode?')) {
      return true;
    }
    if (url.startsWith ('/Shelfs/getShelfInfo')) {
      return true;
    }
    if (url.startsWith ('/AgvTasks/queryAgvStatus')) {
      return true;
    }
    if (url.startsWith('/StockTasks/getDaysSum')) {
      return true;
    }
    if (url.startsWith('/AgvTasks/getTodayAGVTaskSums')) {
      return true;
    }
    if (url.startsWith('/StockTasks/getHoursSum')) {
      return true;
    }
    if (url.startsWith('/AgvTasks/getAgvTaskListByFloor?')) {
      return true;
    }
    if (url.startsWith('/Shelfs/getAreaStationReport')) {
      return true;
    }
    if (url.startsWith('/StockTasks/getAgvTaskList')) {
      return true;
    }

    return false;
  }

  private buildRequestMessage(): any {
    const userStore = useUserStoreWithOut();
    const token = userStore.getToken;
    //console.log("req"+token)
    const { getLocale } = useLocale();
    const language = getLocale.value == 'en' ? getLocale.value : 'zh-Hans';
    return {
      token,
      language,
    };
  }
}
