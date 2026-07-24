import { FormSchema } from '/@/components/Table';
import { BasicColumn } from '/@/components/Table';
import { SelectItem } from '/@/utils/SelectItem';
import moment from 'moment';
import { h } from 'vue';
import {
  StockServiceProxy
} from '/@/services/ServiceProxies';
import { message } from 'ant-design-vue';
import { useLoading } from '/@/components/Loading';
import { useI18n } from '/@/hooks/web/useI18n';
const { t } = useI18n();
const [openFullLoading, closeFullLoading] = useLoading({
  tip: 'Loading...',
});
const _StockServiceProxy = new StockServiceProxy()
export const cellStatusSelectItem: SelectItem[] = [
  {
    label: '全部',
    value: null,
    key: 0,
  },
  {
    label:'待入库',
    value:'Waiting',
    key:3
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
  {
    label: '发送车间',
    value: 'StockOut',
    key: 4,
  },
  {
    label: '筛选',
    value: 'Filtrate',
    key: 5,
  },
];

export const runStatusSelectItem: SelectItem[] = [
  {
    label: '待入',
    value: 'In',
    key: 0,
  },
  {
    label: '库存',
    value: 'Enable',
    key: 1,
  },
  {
    label: '出库',
    value: 'Out',
    key: 2,
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
    width: 240,
    ellipsis: false,
    customRender: ({ text }) =>
      h(
        'div',
        {
          style: {
            whiteSpace: 'normal',
            wordBreak: 'break-word',
            lineHeight: '1.4',
          },
        },
        text ?? '',
      ),
  },
  {
    title: t('库位编号'),
    dataIndex: 'cellCode',
  },
  {
    title: t('规格'),
    dataIndex: 'specs',
  },
  {
    title: t('箱号'),
    dataIndex: 'processNo',
    // 原先是boxData_BoxNumber
  },
  {
    title: t('等级'),
    dataIndex: 'grade',
  },
  {
    title: t('库存数量'),
    dataIndex: 'totalCountInTime',
  },
  {
    title: t('库存状态'),
    dataIndex: 'status',
    customRender: ({ text }) => {
      if(text == null || text == undefined){
        return '';
      }else{
        return cellStatusSelectItem.filter((f) => f.value == text)[0].label;
      }
    },
  },
  {
    title: t('运行状态'),
    dataIndex: 'runStatus',
    customRender: ({ text }) => {
      if (text == null || text === undefined || text === '') {
        return '';
      }
      const statusItem = runStatusSelectItem.find((f) => f.value == text || f.key == text);
      return statusItem?.label ?? text;
    },
  },
  {
    title: t('检验编号'),
    dataIndex: 'checkNo',
  },
  {
    title: t('检验时间'),
    dataIndex: 'checkDate',
  },
  {
    title: t('入库时间'),
    dataIndex: 'stockInDate',
  },
  {
    title: t('收料条形码'),
    dataIndex: 'barcode',
  },
  {
    title: t('所在仓库'),
    dataIndex: 'houseName',
  },
  {
    title: t('所在库区'),
    dataIndex: 'areaName',
  },
  {
    title: t('操作'),
    dataIndex: 'action',
    width: 100,
    fixed: 'right',
    customRender: ({ record }) => {
      return h('a-button', {
        type: 'primary',
        size: 'small',
        style: {
          borderRadius: '8px',
          boxShadow: '0 2px 6px rgba(135, 206, 250, 0.4)',
          fontWeight: '500',
          height: '32px',
          width: '60px',
          padding: '0',
          fontSize: '13px',
          backgroundColor: '#87CEEB',
          color: '#ffffff',
          border: 'none',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
        },
        onClick: () => {
          // 直接将数据存储到sessionStorage
          sessionStorage.setItem('selectedStockRecord', JSON.stringify(record));
          // 触发事件
          window.dispatchEvent(new Event('openStockAdjustment'));
        },
      }, '调整');
    },
  },
  // {
  //   title: t('routes.warehouse.storageBoxManagement_createTime'),
  //   dataIndex: 'creationTime',
  //   customRender: ({ text }) => {
  //     return moment(text).format('YYYY-MM-DD HH:mm:ss');
  //   },
  // },
];



export const searchFormSchema: FormSchema[] = [
  {
    field: 'materialCode',
    label: t('物料编号'),
    component: 'Input',
    colProps: { span: 4 },
  },
  {
    field: 'materialNameTip',
    label: t('物料名称'),
    component: 'Input',
    colProps: { span: 4 },
  },
  {
    field: 'materialSpecsTip',
    label: t('物料规格'),
    component: 'Input',
    colProps: { span: 4 },
  },
  {
    field: 'checkNo',
    label: t('检验编号'),
    component: 'Input',
    colProps: { span: 4 },
  },
  {
    field: 'boxCode',
    label: t('容器编号'),
    component: 'Input',
    colProps: { span: 4 },
  },

  {
    field: 'cellCode',
    label: t('库位编号'),
    component: 'Input',
    colProps: { span: 4 },
  },
  {
    field: 'warehouseAreaName',
    label: t('库区名称'),
    component: 'Select',
    colProps: { span: 4 },
    defaultValue: '正常区',
    componentProps: {
      options: [
        {
          label: '正常区',
          value: '正常区',

        },
        {
          label: '周转区',
          value: '周转区',
        },
        {
          label: '暂存区',
          value: '暂存区',
        },
        {
          label: '待处理区',
          value: '待处理区',
        },
      ]
    }
  },
  {
    field: 'warehouseName',
    label: t('仓库名称'),
    component: 'Input',
    colProps: { span: 4 },
  },
  {
    field: 'barcode',
    label: t('收料条码'),
    component: 'Input',
    colProps: { span: 4 },

  },
  {
    field: 'status',
    label: t('状态'),
    component: 'Select',
    colProps: { span: 4 },
    defaultValue: null,
    componentProps: {
      options: [
        {
          label: '可用',
          value: 'Available',

        },
        {
          label: '锁定',
          value: 'Locked',
        },
        {
          label: '待入库',
          value: 'Waiting',
        },
        {
          label: '筛选',
          value: 'Filtrate',
        },
        {
          label: '发送车间',
          value: 'StockOut',
        },
        {
          label: '冻结',
          value: 'Freezing',
        },
      ]
    }
  },
  {
    field: 'stockInType',
    label: t('入库类型'),
    component: 'Select',
    defaultValue: null,
    colProps: { span: 4 },
    componentProps: {
      options: [
        {
          label: '正常采购',
          value: '1',

        },
        {
          label: '生产入库',
          value: '2',
        },
        {
          label: '委托加工',
          value: '4',
        },
        {
          label: '超期复检',
          value: '7',
        },
      ]
    }
  },
  {
    field: 'time',
    component: 'RangePicker',
    label: '入库时间',
    labelWidth: 80,
    colProps: { span: 4 },
    // defaultValue: [moment().subtract(7, 'days'), moment().add(1, 'days')],
  },
  {
    field: 'checkType',
    label: t('检验类型'),
    defaultValue: null,
    component: 'Select',
    colProps: { span: 4 },
    componentProps: {
      options: [
        {
          label: '进料检验',
          value: '1',

        },
        {
          label: '半成品质检',
          value: '2',
        },
        {
          label: '超期复检',
          value: '4',
        },
        {
          label: '期初库存',
          value: '10',
        },
      ]
    }
  },
  {
    field: 'checkResult',
    label: t('检验结果'),
    defaultValue: null,
    component: 'Select',
    colProps: { span: 4 },
    componentProps: {
      options: [
        {
          label: '合格入仓',
          value: '1',

        },
        {
          label: '不合格',
          value: '2',
        },
        {
          label: '超筛代用',
          value: '3',
        },
      ]
    }
  },
  {
    field: 'fullBoxRateStart',
    label: t('满箱率开始'),
    defaultValue: null,
    component: 'Input',
    colProps: { span: 4 },
  },
  {
    field: 'fullBoxRateEnd',
    label: t('满箱率结束'),
    defaultValue: null,
    component: 'Input',
    colProps: { span: 4 },
  },
  {
    field: 'avaType',
    label: t('所在区域'),
    defaultValue: null,
    component: 'Input',
    colProps: { span: 4 },
  },
  {
    field: 'wareType',
    label: t('仓库类型'),
    component: 'Select',
    defaultValue: '0',
    colProps: { span: 4 },
    componentProps: {
      options: [
        {
          label: '全部',
          value: '0',
        },
        {
          label: '料箱',
          value: '1',
        },
        {
          label: '托盘',
          value: '2',
        },
        {
          label: '分拨墙',
          value: '3',
        },
        {
          label: '手工',
          value: '4',
        },
      ]
    }
  },
  {
    field: 'finGoods',
    label: t('加工成品'),
    defaultValue: null,
    component: 'Input',
    colProps: { span: 4 },
  },
  {
    field: 'targetWarehouseName',
    label: t('所属仓库'),
    defaultValue: null,
    component: 'Input',
    colProps: { span: 4 },
  },
  {
    field: 'productiontime',
    component: 'RangePicker',
    label: '生产日期',
    labelWidth: 80,
    colProps: { span: 4 },
    // defaultValue: [moment().subtract(7, 'days'), moment().add(1, 'days')],
  },
  {
    field: 'bztime',
    component: 'RangePicker',
    label: '保质期',
    labelWidth: 80,
    colProps: { span: 4 },
    // defaultValue: [moment().subtract(7, 'days'), moment().add(1, 'days')],
  },
];



// 库存查询接口
export async function gettable(params) {
  openFullLoading();
  try {
    const res = await _StockServiceProxy.stocksQuery(params);
    closeFullLoading();
    return res;
  } catch (error) {
    closeFullLoading();
    message.error('查询失败');
    return [];
  }
}

