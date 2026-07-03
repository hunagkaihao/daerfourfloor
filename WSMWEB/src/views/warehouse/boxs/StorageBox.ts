import { FormSchema } from '/@/components/Table';
import { BasicColumn } from '/@/components/Table';
import moment from 'moment';
import {
  BoxServiceProxy
} from '/@/services/ServiceProxies';
import { message } from 'ant-design-vue';
import { useLoading } from '/@/components/Loading';

import { useI18n } from '/@/hooks/web/useI18n';
const { t } = useI18n();
const [openFullLoading, closeFullLoading] = useLoading({
  tip: 'Loading...',
});
const _storageBoxsServiceProxy = new BoxServiceProxy();

// 容器类型选项
export const boxTypeSelectItem = [
  {
    label: '托盘',
    value: '1',
    key: 0,
  },
  {
    label: '料箱',
    value: '2',
    key: 1,
  },
];
export const tableColumns: BasicColumn[] = [
  {
    title: t('容器编号'),
    dataIndex: 'boxCode',
  },
  {
    title: t('容器类型'),
    dataIndex: 'boxTypeName',
    customRender: ({ text }) => {
      const typeMap = {
        '1': '托盘',
        '2': '料箱',
        '12': '托盘',
      };
      return typeMap[text] || text;
    },
  },
  {
    title: t('库位名称'),
    dataIndex: 'cellName',
  },
  {
    title: t('状态'),
    dataIndex: 'status',
    customRender: ({ text }) => {
      const statusMap = {
        'NoHave': '无货',
        'Have': '有货',
      };
      return statusMap[text] || text;
    },
  },
  {
    title: t('库区'),
    dataIndex: 'warehouseAreaName',
  },
  {
    title: t('所在仓库'),
    dataIndex: 'warehouseName',
  },
  // {
  //   title: t('routes.warehouse.storageBoxManagement_createTime'),
  //   dataIndex: 'creationTime',
  //   customRender: ({ text }) => {
  //     return moment(text).format('YYYY-MM-DD HH:mm:ss');
  //   },
  // },
];

export const tableDetailColumns: BasicColumn[] = [
  {
    title: t('routes.warehouse.storageBoxManagement_storageBoxBarcode'),
    dataIndex: 'storageBoxBarcode',
    width: 100,
  },
  {
    title: t('routes.material.goodsManagement_goodsCode'),
    dataIndex: 'goodsCode',
    width: 100,
  },
  {
    title: t('routes.material.goodsManagement_name'),
    dataIndex: 'goodsName',
    width: 100,
  },
  {
    title: t('routes.material.goodsManagement_goodsSpec'),
    dataIndex: 'goodsSpec',
    width: 100,
  },
  {
    title: t('routes.material.goodsManagement_goodsBand'),
    dataIndex: 'goodsBand',
    width: 100,
  },
  {
    title: t('routes.warehouse.storageBoxDetailManagement_goodsBatchNo'),
    dataIndex: 'goodsBatchNo',
    width: 100,
  },
  {
    title: t('routes.warehouse.storageBoxDetailManagement_quantity'),
    dataIndex: 'quantity',
    width: 50,
  },
  {
    title: t('routes.material.goodsManagement_goodsUnits'),
    dataIndex: 'goodsUnits',
    width: 50,
  },
  {
    title: t('routes.warehouse.storageBoxManagement_createTime'),
    dataIndex: 'creationTime',
    customRender: ({ text }) => {
      return moment(text).format('YYYY-MM-DD HH:mm:ss');
    },
  },
];

export const searchFormSchema: FormSchema[] = [
  {
    field: 'boxCode',
    label: t('容器编号'),
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'boxName',
    label: t('容器名称'),
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'cellName',
    label: t('库位名称'),
    component: 'Input',
    colProps: { span: 6 },
  },
];

export const WallFormSchema: FormSchema[] = [
  {
    field: 'BoxCode',
    component: 'Input',
    label: t('容器编号'),
    labelWidth: 85,
    required: true,
    colProps: {
      span: 12,
    },
    componentProps: {
      autocomplete: 'off',
    },
  },
  {
    field: 'WallCell',
    component: 'Input',
    label: t('分拨墙库位'),
    labelWidth: 85,
    required: true,
    colProps: {
      span: 12,
    },
    componentProps: {
      autocomplete: 'off',
    },
  },
];

export const WallFormSchema2: FormSchema[] = [
  {
    field: 'WallCell',
    component: 'Input',
    label: t('分拨墙库位'),
    labelWidth: 85,
    required: true,
    colProps: {
      span: 12,
    },
    componentProps: {
      autocomplete: 'off',
    },
  },
];
export const createFormSchema: FormSchema[] = [
  {
    field: 'boxCode',
    component: 'Input',
    label: t('容器编号'),
    labelWidth: 85,
    required: true,
    colProps: {
      span: 12,
    },
    componentProps: {
      autocomplete: 'off',
    },
  },
  {
    field: 'boxTypeName',
    component: 'Select',
    label: t('容器类型'),
    labelWidth: 85,
    required: true,
    colProps: {
      span: 12,
    },
    componentProps: {
      options: [
        {
          label: '托盘',
          value: '1',
        },
        {
          label: '料箱',
          value: '2',
        },
      ],
    },
  },
 
 

];

export const editFormSchema: FormSchema[] = [
  {
    field: 'boxCodeNew',
    component: 'Input',
    label: t('容器编号'),
    labelWidth: 85,
    required: true,
    colProps: {
      span: 12,
    },
    componentProps: {
      autocomplete: 'off',
    },
  },
  {
    field: 'boxNameNew',
    component: 'Input',
    label: t('容器名称'),
    labelWidth: 85,
    required: true,
    colProps: {
      span: 12,
    },
    componentProps: {
      autocomplete: 'off',
    },
  },
  {
    field: 'boxTypeNameNew',
    component: 'Select',
    label: t('容器类型'),
    labelWidth: 85,
    required: true,
    colProps: {
      span: 12,
    },
    componentProps: {
      options: [
        {
          label: '托盘',
          value: '1',
        },
        {
          label: '料箱',
          value: '2',
        },
      ],
    },
  },
  
];

/**
 * 分页列表
 * @param params
 * @returns
 */
export async function getTableListAsync(
  params
): Promise<any> {

  return _storageBoxsServiceProxy.pagedBoxesGet(params);
}
/**
 * Excel导出
 * @param params
 * @returns
 */
export async function allBoxesGet(
  params
): Promise<any> {

  return _storageBoxsServiceProxy.allBoxesGet(params);
}

/**
 * 分页明细列表
 * @param params
 * @returns
 */
export async function getDetaiTableListAsync(
  params
): Promise<any> {
  return _storageBoxsServiceProxy.boxAdd(params);
}

/**
 * 创建书籍
 * @param param0
 */
export async function createStorageBoxAsync({
  request,
  changeOkLoading,
  validate,
  closeModal,
  resetFields,
}) {
  changeOkLoading(true);
  await validate();
  await _storageBoxsServiceProxy.boxAdd(request);
  changeOkLoading(false);
  message.success(t('common.operationSuccess'));
  resetFields();
  closeModal();
}
/**
 * 批量创建
 * @param param0
 */
export async function createManyStorageBoxAsync({ request, changeOkLoading, closeModal }) {
  changeOkLoading(true);
  // await validate();

  await _storageBoxsServiceProxy.boxAdd(request);
  changeOkLoading(false);
  message.success(t('common.operationSuccess'));
  // resetFields();
  closeModal();
}
/**
 * 删除用户
 * @param param0
 */
export async function deleteStorageBoxAsync({ id, reload }) {
  try {
    openFullLoading();
    await _storageBoxsServiceProxy.boxDel(id);
    closeFullLoading();
    message.success(t('common.operationSuccess'));
    reload();
  } catch (error) {
    closeFullLoading();
  }
}

export async function deleteStorageBoxDetailAsync({ id, storageBoxId, reloadDetail }) {
  try {

    openFullLoading();

    await _storageBoxsServiceProxy.boxDel(id);
    closeFullLoading();
    message.success(t('common.operationSuccess'));
    reloadDetail();
  } catch (error) {
    closeFullLoading();
  }
}





/**
 * 编辑用户
 * @param param0
 */
export async function updateStorageBoxAsync({
  boxId,
  request,
  changeOkLoading,
  validate,
  closeModal,
  resetFields,
}) {
  changeOkLoading(true);
  await validate();

  await _storageBoxsServiceProxy.boxUpdate(boxId,request);
  changeOkLoading(false);
  resetFields();
  message.success(t('common.operationSuccess'));
  closeModal();
}
