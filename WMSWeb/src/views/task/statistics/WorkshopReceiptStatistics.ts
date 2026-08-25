import { FormSchema } from '/@/components/Table';
import { BasicColumn } from '/@/components/Table';
import moment from 'moment';
// 移除接口导入，保留前端界面
import { useI18n } from '/@/hooks/web/useI18n';

const { t } = useI18n();
// 移除ServiceProxy，不再需要接口调用

export const tableColumns: BasicColumn[] = [
  {
    title: t('领料单编号'),
    dataIndex: 'pickListCodes',
  },
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
    title: t('车间'),
    dataIndex: 'workshop',
  },
  {
    title: t('合计数量'),
    dataIndex: 'totalQuantity',
  },
  {
    title: t('收料人'),
    dataIndex: 'pickerName',
  },
  {
    title: t('领料单收料情况'),
    dataIndex: 'isCompleted',
  },
];

export const searchFormSchema: FormSchema[] = [
  {
    field: 'materialCode',
    label: t('物料编号'),
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'materialName',
    label: t('物料名称'),
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'workshopName',
    label: t('收料车间名称'),
    component: 'Select',
    colProps: { span: 6 },
    componentProps: {
      options: [
        { label: '一车间', value: '一车间' },
        { label: '二车间', value: '二车间' },
        { label: '三车间', value: '三车间' },
        { label: '五车间', value: '五车间' },
      ],
      placeholder: '请选择车间',
      allowClear: true,
    },
  },
  {
    field: 'time',
    component: 'RangePicker',
    label: '操作时间',
    labelWidth: 80,
    colProps: { span: 6 },
    defaultValue: [moment().subtract(2, 'days'), moment()],
    componentProps: {
      showTime: false,
      format: 'YYYY-MM-DD',
      placeholder: ['开始日期', '结束日期'],
      disabledDate: (current: any) => {
        // 禁用未来日期
        return current && current > moment().endOf('day');
      },
    },
    rules: [
      {
        validator: (rule: any, value: any) => {
          return new Promise((resolve, reject) => {
            if (!value || value.length !== 2) {
              resolve();
              return;
            }
            
            const startDate = moment(value[0]);
            const endDate = moment(value[1]);
            const daysDiff = endDate.diff(startDate, 'days');
            
            if (daysDiff > 3) {
              reject(new Error(t('timeRangeLimit')));
            } else {
              resolve();
            }
          });
        }
      }
    ]
  },
];

// 移除接口实现，保留前端界面
