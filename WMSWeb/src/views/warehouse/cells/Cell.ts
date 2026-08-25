import { FormSchema } from '/@/components/Table';
import { BasicColumn } from '/@/components/Table';
import moment from 'moment';
import { WarehouseServiceProxy,  CellServiceProxy,
  UserContollerServiceProxy,PagedWarehouseQueryDto,PagingUserListInput,PagedResultDto_1OfOfIdentityUserDtoAndContractsAnd_0AndCulture_neutralAndPublicKeyToken_null,
  CellLaneStatusDto } from '/@/services/ServiceProxies';
import { message } from 'ant-design-vue';
import { useLoading } from '/@/components/Loading';
import { SelectItem } from '/@/utils/SelectItem';
import { useI18n } from '/@/hooks/web/useI18n';
const { t } = useI18n();
const [openFullLoading, closeFullLoading] = useLoading({
  tip: 'Loading...',
});
const _CellServiceProxy = new CellServiceProxy()
export const cellTypeSelectItem: SelectItem[] = [
  {
    label: '库位',
    value: 'Cell',
    key: 0,
  },
  {
    label: '站台',
    value: 'Station',
    key: 1,
  },

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
    label: '空托盘',
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

export const warehouseSelectItem: SelectItem[] = [
  {
    label: '平面库',
    value: '平面库',
    key: 0,
  },
  {
    label: '五金库',
    value: '五金库',
    key: 1,
  },
  {
    label: '立库',
    value: '立库',
    key: 2,
  },
  {
    label: '四楼叉车库',
    value: '四楼叉车库',
    key: 3,
  },
];

export const warehouseAreaSelectItem: SelectItem[] = [
  {
    label: '正常区',
    value: '正常区',
    key: 0,
  },
  {
    label: '待处理区',
    value: '待处理区',
    key: 1,
  },
  {
    label: '暂存区',
    value: '暂存区',
    key: 2,
  },
  {
    label: '备料区',
    value: '备料区',
    key: 3,
  },
];


export const tableColumns: BasicColumn[] = [
  {
    title: t('routes.warehouse.cellManagement_cellCode'),
    dataIndex: 'cellCode',
    width: 150,
  },
  {
    title: t('routes.warehouse.cellManagement_warehouseName'),
    dataIndex: 'warehouseName',
    width: 150,
  },
  {
    title: t('routes.warehouse.cellManagement_warehouseAreaName'),
    dataIndex: 'warehouseAreaName',
    width: 150,
  },
  {
    title: t('routes.warehouse.cellManagement_cellType'),
    dataIndex: 'cellType',
    width: 120,
    customRender: ({ text }) => {
      const typeItem = cellTypeSelectItem.find((f) => f.key == text);
      return typeItem ? typeItem.label : text;
    },
  },
  {
    title: t('routes.warehouse.cellManagement_cellStatus'),
    dataIndex: 'cellStatus',
    width: 120,
    customRender: ({ text }) => {
      const statusItem = cellStatusSelectItem.find((f) => f.value == text);
      return statusItem ? statusItem.label : text;
    },
  },
  {
    title: t('routes.warehouse.cellManagement_runStatus'),
    dataIndex: 'runStatus',
    width: 120,
    customRender: ({ text }) => {
      const statusItem = runStatusSelectItem.find((f) => f.value == text);
      return statusItem ? statusItem.label : text;
    },
  },
];

export const searchFormSchema: FormSchema[] = [
  {
    field: 'cellCodeTip',
    label: t('routes.warehouse.cellManagement_cellCode'),
    component: 'Input',
    colProps: { span: 6 },
    componentProps: {
      placeholder: '请输入库位编号',
    },
  },
  {
    field: 'warehouseName',
    label: t('routes.warehouse.cellManagement_warehouseName'),
    component: 'Input',
    colProps: { span: 6 },
    componentProps: {
      placeholder: '请输入仓库名称',
    },
  },
  {
    field: 'warehouseAreaName',
    label: t('routes.warehouse.cellManagement_warehouseAreaName'),
    component: 'Input',
    colProps: { span: 6 },
    componentProps: {
      placeholder: '请输入库区名称',
    },
  },
  {
    field: 'cellStatus',
    label: t('routes.warehouse.cellManagement_cellStatus'),
    component: 'Select',
    colProps: { span: 6 },
    componentProps: {
      options: cellStatusSelectItem,
      placeholder: '请选择库位状态',
      allowClear: true,
    },
  },
  {
    field: 'runStatus',
    label: t('routes.warehouse.cellManagement_runStatus'),
    component: 'Select',
    colProps: { span: 6 },
    componentProps: {
      options: runStatusSelectItem,
      placeholder: '请选择运行状态',
      allowClear: true,
    },
  },
];
// 移除接口实现，保留前端界面
export const createFormSchema: FormSchema[] = [
  {
    field: 'warehouseName',
    component: 'Select',
    label: t('仓库名称'),
    labelWidth: 85,
    required: true,
    colProps: {
      span: 12,
    },
    componentProps: {
      options: warehouseSelectItem,
      placeholder: '请选择仓库名称',
      allowClear: false,
    },
  },
  {
    field: 'warehouseAreaName',
    component: 'Select',
    label: t('仓库区域'),
    labelWidth: 85,
    required: true,
    colProps: {
      span: 12,
    },
    componentProps: {
      options: warehouseAreaSelectItem,
      placeholder: '请选择仓库区域',
      allowClear: false,
    },
  },
  {
    field: 'cellCode',
    component: 'Input',
    label: t('routes.warehouse.cellManagement_cellCode'),
    labelWidth: 85,
    required: true,
    colProps: {
      span: 12,
    },
    componentProps: {
      autocomplete: 'off',
      placeholder: '请输入库位编码',
    },
  },
  {
    field: 'cellType',
    component: 'Input',
    label: t('routes.warehouse.cellManagement_cellType'),
    labelWidth: 85,
    required: true,
    defaultValue: 'Cell',
    colProps: {
      span: 12,
    },
    componentProps: {
      autocomplete: 'off',
      placeholder: '库位类型',
      disabled: true,
    },
  },
];

export const editFormSchema: FormSchema[] = [
  // {
  //   field: 'warehouseName',
  //   component: 'Input',
  //   label: t('仓库名称'),
  //   labelWidth: 85,
  //   required: true,
  //   colProps: {
  //     span: 12,
  //   },
  //   componentProps: {
  //     autocomplete: 'off',
  //   },
  // },
  // {
  //   field: 'warehouseAreaName',
  //   component: 'Input',
  //   label: t('仓库区域'),
  //   labelWidth: 85,
  //   required: true,
  //   colProps: {
  //     span: 12,
  //   },
  //   componentProps: {
  //     autocomplete: 'off',
  //   },
  // },
  // {
  //   field: 'shelfName',
  //   component: 'Input',
  //   label: t('料车名称'),
  //   labelWidth: 85,
  //   colProps: {
  //     span: 12,
  //   },
  //   componentProps: {
  //     autocomplete: 'off',
  //   },
  // },
  {
    field: 'cellCode',
    component: 'Input',
    label: t('routes.warehouse.cellManagement_cellCode'),
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
    label: t('物料区域'),
    field: 'materialArea',
    component: 'Select',
    colProps: { span: 12 },
    componentProps: {
      autocomplete: 'off',
      // api:getspecs, // 移除API调用
      options: [], // 使用静态选项
      placeholder: '请选择物料区域',
      allowClear: true,
    },
  },
  // {
  //   field: 'cellName',
  //   component: 'Input',
  //   label: t('库位名称'),
  //   labelWidth: 85,
  //   required: true,
  //   colProps: {
  //     span: 12,
  //   },
  //   componentProps: {
  //     autocomplete: 'off',
  //   },
  // },
  // {
  //   field: 'cellType',
  //   component: 'Input',
  //   label: t('routes.warehouse.cellManagement_cellType'),
  //   labelWidth: 85,
  //   required: true,
  //   colProps: {
  //     span: 12,
  //   },
  //   componentProps: {
  //     autocomplete: 'off',
  //     disabled: true,
  //   },
  // },{
  //   field: 'availableBoxSpecsNames',
  //   component: 'Input',
  //   label: t('允许容器尺寸'),
  //   labelWidth: 85,
  //   colProps: {
  //     span: 12,
  //   },
  //   componentProps: {
  //     autocomplete: 'off',
  //     disabled: true,
  //   },
  // },
];
// bindareaFormSchema已删除
export const cellChartFormSchema: FormSchema[] = [
  {
    field: 'rowcn',
    component: 'Select',
    label: t('选择货架') + ':',
    labelWidth: 85,
    defaultValue:'第1排',
    colProps: {
      span: 12,
    },
    componentProps: {
      options: [
        {
          label: '第1排',
          value: '第1排',
        },
        {
          label: '第2排',
          value: '第2排',
        },
        {
          label: '第3排',
          value: '第3排',
        },
        {
          label: '第4排',
          value: '第4排',
          // key: 2,
        },
      ],
    },

  },
  {
    field: 'cellCode',
    component: 'Input',
    label: t('库位') + ':',
    labelWidth: 85,
    colProps: {
      span: 12,
    },
    componentProps: {
      autocomplete: 'off',
      disabled: true,
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
  return _CellServiceProxy.pagedCellsGet(params);
}
/**
 * Excel导出
 * @param params
 * @returns
 */
export async function allCellsAndMaterialAreaGet(
  params
): Promise<any> {
  return _CellServiceProxy.allCellsAndMaterialAreaGet(params);
}
/**
 * 库位周转率分页列表
 * @param params
 * @returns
 */
export async function getCellTurnoverPaged(
  params
): Promise<any> {
  return _CellServiceProxy.getCellTurnoverPaged(params);
}

/**
 * 查询同巷道列下其他库位状态
 * @param cellCode 库位编码
 */
export async function getLaneCellStatusByCellCode(
  cellCode: string
): Promise<CellLaneStatusDto[]> {
  return _CellServiceProxy.getLaneCellStatusByCellCode(cellCode);
}
/**
 * 创建库位
 * @param param0
 */
export async function createCellAsync({
  request,
  changeOkLoading,
  validate,
  closeModal,
  resetFields,
}) {
  changeOkLoading(true);
  await validate();
  
  // 设置库位名称等于库位编码
  request.cellName = request.cellCode;
  
  await _CellServiceProxy.cellAdd(request)
  changeOkLoading(false);
  message.success(t('common.operationSuccess'));
  resetFields();
  closeModal();
}

// 绑定库区和解绑库区接口实现已删除
/**
 * 删除用户
 * @param param0
 */
export async function deleteCellAsync({ id, reload }) {
  try {
    await _CellServiceProxy.cellDel(id);
    closeFullLoading();
    message.success(t('common.operationSuccess'));
    reload();
  } catch (error) {
    closeFullLoading();
  }
}

/**
 * 编辑库位
 * @param param0
 */
export async function updateCellAsync({
  request,
  changeOkLoading,
  validate,
  closeModal,
  resetFields,
}) {
  changeOkLoading(true);
  await validate();
  await _CellServiceProxy.updateCellMaterialArea(request.cellCode,request.materialArea)

  changeOkLoading(false);
  resetFields();
  message.success(t('common.operationSuccess'));
  closeModal();
}



//禁用库位
export async function disableCellAsync(cellCode: string, reload: () => void) {
  try {
    openFullLoading();
    const result = await _CellServiceProxy.disableCell(cellCode);
    closeFullLoading();
    
    if (result.success) {
      message.success(result.message);
      reload();
    } else {
      message.error(result.message);
    }
  } catch (error) {
    closeFullLoading();
    message.error('禁用库位失败');
  }
}

//启用库位
export async function enableCellAsync(cellCode: string, reload: () => void) {
  try {
    openFullLoading();
    const result = await _CellServiceProxy.enableCell(cellCode);
    closeFullLoading();
    
    if (result.success) {
      message.success(result.message);
      reload();
    } else {
      message.error(result.message);
    }
  } catch (error) {
    closeFullLoading();
    message.error('启用库位失败');
  }
}

  //创建许多库位
  export async function createManyCellsAsync({ request, changeOkLoading, closeModal }) {
    changeOkLoading(true);
    // await validate();

    changeOkLoading(false);
    message.success(t('common.operationSuccess'));
    // resetFields();
    closeModal();
  }

  //通过cellid获取容器
export async function getBoxByCellId(
  cellid
): Promise<any> {

}
const _WarehouseServiceProxy = new WarehouseServiceProxy()
/**
 * 分页列表
 * @param params
 * @returns
 */
export async function getWareListAsync(
  param:PagedWarehouseQueryDto
):Promise<any>{
  return (await _WarehouseServiceProxy.pagedWarehouseGet(param)).items;
}
export async function getAreaListAsync(
  param
):Promise<any>{
  if(param == "1"){
    return []
  }
  return _WarehouseServiceProxy.warehouseAreasGet(param);
}