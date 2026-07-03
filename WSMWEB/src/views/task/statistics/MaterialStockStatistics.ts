import { FormSchema } from '/@/components/Table';
import { BasicColumn } from '/@/components/Table';
import moment from 'moment';
// 移除接口导入，保留前端界面
import { useI18n } from '/@/hooks/web/useI18n';

const { t } = useI18n();
// 移除ServiceProxy，不再需要接口调用

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
    title: t('单位'),
    dataIndex: 'unit',
  },
  {
    title: t('入库类型'),
    dataIndex: 'stockInType',
  },
  {
    title: t('合计数量'),
    dataIndex: 'totalQuantity',
  },
  {
    title: t('入库次数'),
    dataIndex: 'stockInCount',
  },
];

export const searchFormSchema: FormSchema[] = [
  {
    field: 'materialNameTip',
    label: t('物料名称'),
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'materialSpecsTip',
    label: t('物料规格'),
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'materialCode',
    label: t('物料编号'),
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'barcode',
    label: t('收料码'),
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'stockInType',
    label: t('入库类型'),
    component: 'Select',
    colProps: { span: 6 },
    componentProps: {
      options: [
        {
          label: '正常采购',
          value: '正常采购',
        },
        {
          label: '生产入库',
          value: '生产入库',
        },
        {
          label: '委托加工',
          value: '委托加工',
        },
        {
          label: '盘点入库',
          value: '盘点入库',
        },
        {
          label: '超期复检',
          value: '超期复检',
        },
      ]
    }
  },
  {
    field: 'time',
    component: 'RangePicker',
    label: '入库时间',
    labelWidth: 80,
    colProps: { span: 6 },
    defaultValue: [moment().subtract(7, 'days'), moment().add(1, 'days')],
  },
];

// 移除接口实现，保留前端界面
