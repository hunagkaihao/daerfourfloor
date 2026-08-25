import { FormSchema } from '/@/components/Table';
import { BasicColumn } from '/@/components/Table';
import { SelectItem } from '/@/utils/SelectItem';
import moment from 'moment';
import {
  StockServiceProxy,
  PickListServiceProxy,
  DepartmentServiceProxy
} from '/@/services/ServiceProxies';
import { message } from 'ant-design-vue';
import { useLoading } from '/@/components/Loading';
import { useI18n } from '/@/hooks/web/useI18n';
const { t } = useI18n();
const [openFullLoading, closeFullLoading] = useLoading({
  tip: 'Loading...',
});
const _StockServiceProxy = new StockServiceProxy()
const _PickListServiceProxy= new PickListServiceProxy()
const _DepartmentServiceProxy = new DepartmentServiceProxy();
export const cellStatusSelectItem: SelectItem[] = [
  {
    label: '全部',
    value: null,
    key: 0,
  },
  {
    label: '可用',
    value: 'Available',
    key: 0,
  },
  {
    label: '锁定',
    value: 'Locked',
    key: 1,
  },
  {
    label: '冻结',
    value: 'Freezing',
    key: 2,
  },
];
export const tableColumns: BasicColumn[] = [
  {
    title: t('领用单号'),
    dataIndex: 'pickListCode',
  },
  {
    title: t('领用部门'),
    dataIndex: 'departmentName',
  },
  {
    title: t('批次号'),
    dataIndex: 'pickBatch',
  },
  {
    title:"领用类型",
    dataIndex: 'pickType',
  },
  {
    title:"成品编号",
    dataIndex: 'goodsCode',
  },
  {
    title:"成品名称",
    dataIndex: 'goodsName',
  },
  {
    title:"成品规格",
    dataIndex: 'goodsSpecs',
  },
  {
    title:"时间",
    dataIndex: 'pickListDate',
  },
  {
    title:"领用状态",
    dataIndex: 'status',
  }
];



export const searchFormSchema: FormSchema[] = [
  {
    field: 'materialCode',
    label: t('物料编号'),
    component: 'Input',
    colProps: { span: 4 },
  },
  {
    field: 'batchCode',
    label: t('批次号'),
    component: 'Input',
    colProps: { span: 4 },
  },
  {
    field: 'materialName',
    label: t('物料名称'),
    component: 'Input',
    colProps: { span: 4 },
  },
  {
    field: 'specs',
    label: t('物料规格'),
    component: 'Input',
    colProps: { span: 4 },
  },
  {
    field: 'departmentName',
    label: t('部门'),
    component: 'ApiSelect',
    colProps: { span: 4 },
    componentProps: {
      api:allDepartmentsGet,
      autocomplete: 'off',
      labelField: 'departmentName',
      valueField: 'departmentName',
      immediate: true,
    },
  },
  {
    field: 'isCompleted',
    component: 'Checkbox',
    label: '领料单完成',
    labelWidth: 80,
    colProps: { span: 4 },
    defaultValue: false,
  },
  {
    field: 'time',
    component: 'RangePicker',
    label: '出库时间',
    labelWidth: 80,
    colProps: { span: 6 },
    defaultValue: [moment().subtract(7, 'days'), moment().add(1, 'days')],
  },

];

//查询部门
export function allDepartmentsGet(
): Promise<any> {
  return _DepartmentServiceProxy.allDepartmentsGet();
}

/**
 * 分页列表
 * @param params
 * @returns
 */
export async function getTableListAsync(
  params
): Promise<any> {

  return _PickListServiceProxy.pagedPickListsGet(params);
}
export async function allPickListItemsFlat(
  params
): Promise<any> {

  return (await _PickListServiceProxy.allPickListItemsFlat(params)).items;
}
export async function stocksMoveWall(
  params
): Promise<any> {

  return _StockServiceProxy.stocksMoveWall(params);
}
/**
 * Excel表
 * @param params
 * @returns
 */
export async function getTableAllAsync(
  params
): Promise<any> {

  return _StockServiceProxy.stocksQuery(params);
}

