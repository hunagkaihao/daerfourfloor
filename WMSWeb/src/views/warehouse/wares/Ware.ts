import { FormSchema } from '/@/components/Table';
import { BasicColumn } from '/@/components/Table';
import moment from 'moment';
import { WarehouseServiceProxy,
  UserServiceProxy,PagedWarehouseQueryDto,PagingUserListInput,PagedResultDto_1OfOfIdentityUserDtoAndContractsAnd_0AndCulture_neutralAndPublicKeyToken_null } from '/@/services/ServiceProxies';
import { message } from 'ant-design-vue';
import { useLoading } from '/@/components/Loading';
import { useI18n } from '/@/hooks/web/useI18n';
import { SelectItem } from '/@/utils/SelectItem';
import{ useUserStore } from '/@/store/modules/user'
import { reactive } from 'vue';
const _WarehouseServiceProxy = new WarehouseServiceProxy()
const _UserContollerServiceProxy = new UserServiceProxy();
const { t } = useI18n();
const cellStore = useUserStore()
/**
 * 分页列表
 * @param params
 * @returns
 */
export async function getWareListAsync(
  param:PagedWarehouseQueryDto
):Promise<any>{
  const result = await _WarehouseServiceProxy.pagedWarehouseGet(param);
  
  // 对返回的数据按warehouseCode从小到大排序
  if (result && result.items && Array.isArray(result.items)) {
    result.items.sort((a, b) => {
      const codeA = a.warehouseCode || '';
      const codeB = b.warehouseCode || '';
      return codeA.localeCompare(codeB, undefined, { numeric: true, sensitivity: 'base' });
    });
  }
  
  return result;
}
export async function getAreaListAsync(
  param
):Promise<any>{

  if(param == "1"){
    return []
  }
  return _WarehouseServiceProxy.warehouseAreasGet(param);
}

export async function getTableListAsync(
  params
): Promise<any> {
  return _UserContollerServiceProxy.page(params);
}
const option = reactive([
  {
    value: 1,
    label: '自动化叉车库',
  },
  // {
  //   value: 2,
  //   label: '仓库二',
  // },
  // {
  //   value: 3,
  //   label: '仓库三',
  // },
]);





const [openFullLoading, closeFullLoading] = useLoading({
  tip: 'Loading...',
});

export const cellTypeSelectItem: SelectItem[] = [
  {
    label: '库位',
    value: 'Cell',
    key: 0,
  },
  {
    label: 'CTU库位',
    value: 'CTUCell',
    key: 1,
  },
  {
    label: '分拨墙',
    value: 'WallCell',
    key: 2,
  },
  {
    label: '站台',
    value: 'Station',
    key: 3,
  },
  {
  label: '异常站台',
  value: 'ErrorStation',
  key: 4,
  }
];

export const cellStatusSelectItem: SelectItem[] = [
  {
    label: '满货',
    value: 'Full',
    key: 0,
  },
  {
    label: '有货',
    value: 'Have',
    key: 1,
  },
  {
    label: '无货',
    value: 'Nohave',
    key: 2,
  },
  {
    label: '空容器',
    value: 'Pallet',
    key: 3,
  },
];

export const runStatusSelectItem: SelectItem[] = [
  {
    label: '禁用',
    value: 'Disable',
    key: 0,
  },
  {
    label: '可用',
    value: 'Enable',
    key: 1,
  },
  {
    label: '运行',
    value: 'Run',
    key: 2,
  },
  {
    label: '选定',
    value: 'Selected',
    key: 3,
  },
];
export const WareColumns: BasicColumn[] = [
  {
    title: t('仓库编号'),
    dataIndex: 'warehouseCode',
  },
  {
    title: t('仓库名称'),
    dataIndex: 'warehouseName',
  },
  {
    title: t('仓库类型'),
    dataIndex: 'warehouseType',
  },
  {
    title: t('仓库备注'),
    dataIndex: 'warehouseRemark',
  },

]
export const AreaColumns: BasicColumn[] = [
  {
    title: t('库区编号'),
    dataIndex: 'warehouseAreaCode',
  },
  {
    title: t('库区名称'),
    dataIndex: 'warehouseAreaName',
  },
  {
    title: t('库区备注'),
    dataIndex: 'warehouseAreaRemark',
  },


]
export const WaresearchFormSchema: FormSchema[] = [
  {
    field: 'nameFilter',
    label: t('仓库名称'),
    component: 'Input',
    colProps: { span: 8 },
  },
];
export const searchFormSchema: FormSchema[] = reactive([
  {
    field: 'filter',
    label: t('routes.warehouse.cellManagement_cellCode'),
    component: 'Input',
    colProps: { span: 8 },
  },
  {
    field: 'Warehouseld',
    label: t('所属仓库'),
    component: 'Select',
    colProps: { span: 8 },
    componentProps:{
      options:option
    }
  },
]);
export const WareFormSchema: FormSchema[] = [
    {
        field: 'warehouseCode',
        component: 'Input',
        label: t('仓库编号'),
        labelWidth: 85,
        required: true,
        colProps: {
          span: 12,
        },
    },    
    {
      field: 'warehouseName',
      component: 'Input',
      label: t('仓库名称'),
      labelWidth: 85,
      required: true,
      //defaultValue: 'Cell',
      colProps: {
        span: 12,
      },  
    },
    {
      field: 'warehouseType',
      component: 'Input',
      label: t('仓库类型'),
      labelWidth: 85,
      required: true,
      defaultValue: 'CTU',
      colProps: {
        span: 12,
      },
      componentProps: {
        //autocomplete: 'off',
        disabled: true,
      },
  }, 
  
]

export const createAreaFormSchema: FormSchema[] = reactive([
  {
    field: 'warehouseAreaCode',
    component: 'Input',
    label: t('库区编号'),
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
    field: 'warehouseAreaName',
    component: 'Input',
    label: t('库区名称'),
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
    field: 'warehouseAreaRemark',
    component: 'Input',
    label: t('库区备注'),
    labelWidth: 85,
    required: true,
    colProps: {
      span: 12,
    },
    
  },
 
]);
export const createFormSchema: FormSchema[] = reactive([
  {
    field: 'warehouseCode',
    component: 'Input',
    label: t('仓库编号'),
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
    field: 'warehouseName',
    component: 'Input',
    label: t('仓库名称'),
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
    field: 'warehouseType',
    component: 'Input',
    label: t('仓库类型'),
    labelWidth: 85,
    required: true,
    colProps: {
      span: 12,
    },
    
  },
  {
    field: 'warehouseRemark',
    component: 'Input',
    label: t('仓库备注'),
    labelWidth: 85,

    colProps: {
      span: 12,
    },

  },
]);

export const EditWareFormSchema: FormSchema[] = [
  {
    field: 'warehouseCodeNew',
    component: 'Input',
    label: t('仓库编码'),
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
    field: 'warehouseNameNew',
    component: 'Input',
    label: t('仓库名称'),
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
    field: 'warehouseTypeNew',
    component: 'Input',
    label: t('仓库类型'),
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
    field: 'warehouseRemarkNew',
    component: 'Input',
    label: t('仓库备注'),
    labelWidth: 85,

    colProps: {
      span: 12,
    },

  },
];
export const EditAreaFormSchema: FormSchema[] = [
 
  {
    field: 'warehouseAreaCodeNew',
    component: 'Input',
    label: t('库区编号'),
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
    field: 'warehouseAreaNameNew',
    component: 'Input',
    label: t('库区名称'),
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
    field: 'warehouseAreaRemarkNew',
    component: 'Input',
    label: t('库区备注'),
    labelWidth: 85,
    required: true,
    colProps: {
      span: 12,
    },
    
  },
];
export const editFormSchema: FormSchema[] = [
  {
    field: 'warehouseCode',
    component: 'Input',
    label: t('仓库编码'),
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
    field: 'warehouseName',
    component: 'Input',
    label: t('仓库名称'),
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
    field: 'warehouseType',
    component: 'Input',
    label: t('仓库类型'),
    labelWidth: 85,
    required: true,
    colProps: {
      span: 12,
    },
    componentProps: {
      autocomplete: 'off',
      disabled: true,
    },
  },
];







export async function getTableListByZAsync(){}




/**
 * 创建仓库
 * @param param0
 */
export async function createWareAsync({
  request,
  changeOkLoading,
  validate,
  closeModal,
  resetFields,
}) {
  changeOkLoading(true);
  await validate();
  await _WarehouseServiceProxy.warehouseAdd(request)
  changeOkLoading(false);
  message.success(t('common.operationSuccess'));
  resetFields();
  closeModal();
}

/**
 * 创建库区
 * @param param0
 */
export async function createAreaAsync({
  warehouseCode,
  request,
  changeOkLoading,
  validate,
  closeModal,
  resetFields,
}) {
  changeOkLoading(true);
  await validate();
  await _WarehouseServiceProxy.warehouseAreaAdd(warehouseCode,request)
  changeOkLoading(false);
  message.success(t('common.operationSuccess'));
  resetFields();
  closeModal();
}

/**
 * 删除仓库
 * @param param0
 */
export async function deleteWareAsync({ id, reload }) {
  try {
    await _WarehouseServiceProxy.warehouseDel(id)
    openFullLoading();
    closeFullLoading();
    message.success(t('common.operationSuccess'));
    reload();
  } catch (error) {
    closeFullLoading();
  }
}
/**
 * 删除仓库
 * @param param0
 */
export async function deleteAreaAsync({ areaIdToDel, reloadarea }) {
  try {
    await _WarehouseServiceProxy.warehouseAreaDel(areaIdToDel)
    openFullLoading();
    closeFullLoading();
    message.success(t('common.operationSuccess'));
    reloadarea();
  } catch (error) {
    closeFullLoading();
  }
}
/**
 * 编辑用户
 * @param param0
 */
export async function updateWareAsync({
  id,
  request,
  changeOkLoading,
  validate,
  closeModal,
  resetFields,
}) {
  changeOkLoading(true);
  await validate();
  await _WarehouseServiceProxy.warehouseUpdate(id,request)

  changeOkLoading(false);
  resetFields();
  message.success(t('common.operationSuccess'));
  closeModal();
}
/**
 * 编辑库区
 * @param param0
 */
export async function updateAreaAsync({
  id,
  request,
  changeOkLoading,
  validate,
  closeModal,
  resetFields,
}) {
  changeOkLoading(true);
  await validate();
  await _WarehouseServiceProxy.warehouseAreaUpdate(id,request)

  changeOkLoading(false);
  resetFields();
  message.success(t('common.operationSuccess'));
  closeModal();
}
