import { FormSchema } from '/@/components/Table';
import { BasicColumn } from '/@/components/Table';
import { SelectItem } from '/@/utils/SelectItem';
import moment from 'moment';
import {
  PickListServiceProxy,
  StockOutHistoryServiceProxy,
  DepartmentServiceProxy,
  ErpMidTableServiceProxy,
  NoPlanPickListDelDto
} from '/@/services/ServiceProxies';
import { message } from 'ant-design-vue';
import { useLoading } from '/@/components/Loading';
import { useI18n } from '/@/hooks/web/useI18n';
const _DepartmentServiceProxy = new DepartmentServiceProxy();
const _ErpMidTableServiceProxy = new ErpMidTableServiceProxy();
// 移除接口实现，保留前端界面
const _PickNotifierServiceProxy = new PickListServiceProxy();

const { t } = useI18n();
const [openFullLoading, closeFullLoading] = useLoading({
  tip: 'Loading...',
});
const _StockOutHistoryServiceProxy = new StockOutHistoryServiceProxy()

// 移除所有接口实现，保留前端界面
export const cellStatusSelectItem: SelectItem[] = [
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
        key: 1,
      },
  ];
export const tableColumns: BasicColumn[] = [
  {
    title: t('物料编号'),
    dataIndex: 'materialCode',
  },
  {
    title: t('物料名称'),
    dataIndex: 'materialName',
  },
  {
    title: t('规格'),
    dataIndex: 'specs',
  },
  {
    title: t('领用数量'),
    dataIndex: 'countToPick',
  },
  {
    title: t('单位'),
    dataIndex: 'unit',
  },
  {
    title: t('领用类型'),
    dataIndex: 'pickType',
  },
  {
    title: t('领用人'),
    dataIndex: 'pickManName',
  },
];



export const searchFormSchema: FormSchema[] = [
  {
    field: 'departmentId',
    label: t('部门'),
    component: 'ApiSelect',
    colProps: { span: 6 },
    componentProps: {
      // api:allDepartmentsGet, // 移除API调用
      options: [], // 使用静态选项
      autocomplete: 'off',
      labelField: 'label',
      valueField: 'value',
      immediate: true,
    },
  },
  // {
  //   field: 'departmentId',
  //   label: t('物料规格'),
  //   component: 'Input',
  //   colProps: { span: 6 },
  // },
  {
    field: 'materialNameTip',
    label: t('物料名称'),
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'materialCodeTip',
    label: t('物料编码'),
    component: 'Input',
    colProps: { span: 6 },
  },
 

  
];


export const createFormSchema: FormSchema[] = [
  {
    field: 'materialCode',
    component: 'Input',
    label: t('物料编码'),
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
    field: 'departmentId',
    component: 'ApiSelect',
    label: t('部门'),
    labelWidth: 85,

    required: true,
    colProps: {
      span: 12,
    },
    componentProps: {
      optionFilterProp: 'label',
      showSearch: true,
      // api:allDepartmentsGet, // 移除API调用
      options: [], // 使用静态选项
      autocomplete: 'off',
      labelField: 'label',
      valueField: 'value',
      immediate: true,
    },
  },
  {
    field: 'pickerName',
    component: 'ApiSelect',
    label: t('领用人'),
    labelWidth: 85,
    required: true,
    show:true,
    colProps: {
      span: 12,
    },
    componentProps: {
      showSearch: true,
      // api:pickerNamesGet, // 移除API调用
      options: [], // 使用静态选项
      autocomplete: 'off',
      labelField: 'label',
      valueField: 'value',
      immediate: true,
    },
  },
  {
    field: 'pickType',
    component: 'ApiSelect',
    label: t('领用类型'),
    labelWidth: 85,
    required: true,
    colProps: {
      span: 12,
    },
    componentProps: {
      // api:noPlanPickTypesGet, // 移除API调用
      options: [], // 使用静态选项
      autocomplete: 'off',
      labelField: 'label',
      valueField: 'value',
      immediate: true,
    },
  },
  {
    field: 'pickCount',
    component: 'Input',
    label: t('领用数量'),
    required: true,
    labelWidth: 85,
    colProps: {
      span: 12,
    },
    componentProps: {
      autocomplete: 'off',
    },
  },
 
 
];
export const editFormSchema: FormSchema[] = [
  {
    field: 'materialCode',
    component: 'Input',
    label: t('物料编码'),
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
  {
    field: 'departmentId',
    component: 'Input',
    label: t('部门'),
    labelWidth: 85,
    colProps: {
      span: 12,
    },
    componentProps: {
      autocomplete: 'off',
      disabled: true,
    },
  },
  {
    field: 'newPickerName',
    component: 'ApiSelect',
    label: t('领用人'),
    labelWidth: 85,
    required: true,
    show:true,
    colProps: {
      span: 12,
    },
    componentProps: {
      showSearch: true,
      // api:pickerNamesGet, // 移除API调用
      options: [], // 使用静态选项
      autocomplete: 'off',
      labelField: 'label',
      valueField: 'value',
      immediate: true,
    },
  },
  {
    field: 'newPickType',
    component: 'ApiSelect',
    label: t('领用类型'),
    required: true,
    labelWidth: 85,
    colProps: {
      span: 12,
    },
    componentProps: {
      // api:noPlanPickTypesGet, // 移除API调用
      options: [], // 使用静态选项
      autocomplete: 'off',
      labelField: 'label',
      valueField: 'value',
      immediate: true,
    },
  },
  {
    field: 'newPickCount',
    component: 'Input',
    label: t('领用数量'),
    required: true,
    labelWidth: 85,
    colProps: {
      span: 12,
    },
    componentProps: {
      autocomplete: 'off',
    },
  },
 
 
];

