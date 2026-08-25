import { FormSchema } from '/@/components/Table';
import { BasicColumn } from '/@/components/Table';
import { SelectItem } from '/@/utils/SelectItem';
import moment from 'moment';
import { h } from 'vue';
import {
  BarcodeListServiceProxy,
  PagedBarcodeListQueryDto,
  BarcodeDto
} from '/@/services/ServiceProxies';
import { message } from 'ant-design-vue';
import { useLoading } from '/@/components/Loading';
import { useI18n } from '/@/hooks/web/useI18n';

const { t } = useI18n();
const [openFullLoading, closeFullLoading] = useLoading({
  tip: 'Loading...',
});

const _BarcodeListServiceProxy = new BarcodeListServiceProxy();

// 入库类型选择项
export const stockInTypeSelectItem: SelectItem[] = [
  {
    label: '全部',
    value: null,
    key: 0,
  },
  {
    label: '正常采购',
    value: '1',
    key: 1,
  },
  {
    label: '生产入库',
    value: '2', 
    key: 2,
  },
  {
    label: '委托加工',
    value: '4',
    key: 4,
  },
  {
    label: '超期复检',
    value: '7',
    key: 7,
  },
];


// 表格列定义
export const tableColumns: BasicColumn[] = [
  {
    title: t('收料码'),
    dataIndex: 'barcode',
    width: 150,
  },
  {
    title: t('物料编号'),
    dataIndex: 'materialCode',
    width: 120,
  },
  {
    title: t('物料名称'),
    dataIndex: 'materialName',
    width: 200,
  },
  {
    title: t('规格'),
    dataIndex: 'specs',
    width: 150,
  },
  {
    title: t('单位'),
    dataIndex: 'unit',
    width: 80,
  },
  {
    title: t('总收料数量'),
    dataIndex: 'receiveTotalCount',
    width: 120,
  },
  {
    title: t('已绑定数'),
    dataIndex: 'inBindCount',
    width: 100,
  },
  {
    title: t('剩余绑定数'),
    dataIndex: 'surplusCount',
    width: 120,
  },
  {
    title: t('已抽检数'),
    dataIndex: 'inCheckOutCount',
    width: 100,
  },
  {
    title: t('是否已抽检'),
    dataIndex: 'isCheckOut',
    width: 120,
    slots: { customRender: 'isCheckOut' },
  },
  {
    title: t('供应商编号'),
    dataIndex: 'supplierCode',
    width: 120,
  },
  {
    title: t('供应商名称'),
    dataIndex: 'supplierName',
    width: 150,
  },
  {
    title: t('采购单号'),
    dataIndex: 'purchaseId',
    width: 120,
  },
  {
    title: t('入库类型'),
    dataIndex: 'stockInType',
    width: 100,
  },
  {
    title: t('收料日期'),
    dataIndex: 'slDate',
    width: 120,
    customRender: ({ text }) => {
      if (text == null || text == undefined) {
        return '';
      }
      return moment(text).format('YYYY-MM-DD');
    },
  },
  {
    title: t('目标仓库'),
    dataIndex: 'targetWarehouseName',
    width: 120,
  },
  {
    title: t('检验编号'),
    dataIndex: 'checkNo',
    width: 120,
  },
  {
    title: t('操作'),
    key: 'action',
    width: 100,
    customRender: ({ record }) => {
      return h('a-button', {
        type: 'primary',
        size: 'small',
        style: {
          margin: '2px',
          padding: '4px 8px',
          height: '24px',
          fontSize: '12px',
          borderRadius: '4px',
          border: '1px solid #1890ff',
          backgroundColor: '#1890ff',
          color: '#fff',
          cursor: 'pointer',
        },
        onClick: () => {
          console.log('操作按钮被点击', record);
          // 调用父组件的handleEdit函数
          if (window.handleBarcodeEdit) {
            window.handleBarcodeEdit(record);
          } else {
            console.error('handleBarcodeEdit函数未定义');
          }
        },
      }, '修改');
    },
  },
];

// 搜索表单配置
export const searchFormSchema: FormSchema[] = [
  {
    field: 'barcode',
    label: t('收料码'),
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
    field: 'materialName',
    label: t('物料名称'),
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'specs',
    label: t('规格'),
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'supplierCode',
    label: t('供应商编号'),
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'supplierName',
    label: t('供应商名称'),
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'stockInType',
    label: t('入库类型'),
    component: 'Select',
    componentProps: {
      options: stockInTypeSelectItem,
    },
    colProps: { span: 6 },
  },
  {
    field: 'purchaseId',
    label: t('采购单号'),
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'targetWarehouseCode',
    label: t('目标仓库编码'),
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'targetWarehouseName',
    label: t('目标仓库名称'),
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'slDateRange',
    label: t('收料日期'),
    component: 'RangePicker',
    componentProps: {
      style: { width: '100%' },
      defaultValue: [moment().subtract(7, 'days'), moment()],
      placeholder: ['开始日期', '结束日期'],
      format: 'YYYY-MM-DD',
    },
    colProps: { span: 6 },
  },
];

// 分页查询到货单信息
export async function getTableListAsync(params: PagedBarcodeListQueryDto) {
  try {
    console.log('=== getTableListAsync 调用 ===');
    console.log('传入参数:', JSON.stringify(params, null, 2));
    console.log('参数类型检查:', {
      SLDateStart: params.SLDateStart,
      SLDateEnd: params.SLDateEnd,
      SLDateStartType: typeof params.SLDateStart,
      SLDateEndType: typeof params.SLDateEnd,
      SLDateStartIsMoment: params.SLDateStart && typeof params.SLDateStart.format === 'function',
      SLDateEndIsMoment: params.SLDateEnd && typeof params.SLDateEnd.format === 'function',
      SLDateStartValue: params.SLDateStart ? params.SLDateStart.format('YYYY-MM-DD') : 'undefined',
      SLDateEndValue: params.SLDateEnd ? params.SLDateEnd.format('YYYY-MM-DD') : 'undefined'
    });
    
    // 检查参数是否包含日期
    if (!params.SLDateStart || !params.SLDateEnd) {
      console.warn('⚠️ 警告：收料日期参数缺失！', {
        SLDateStart: params.SLDateStart,
        SLDateEnd: params.SLDateEnd
      });
    } else {
      console.log('✅ 收料日期参数存在:', {
        SLDateStart: params.SLDateStart.format('YYYY-MM-DD'),
        SLDateEnd: params.SLDateEnd.format('YYYY-MM-DD')
      });
    }
    
    openFullLoading();
    const result = await _BarcodeListServiceProxy.getPagedBarcodeLists(params);
    console.log('接口返回结果:', result);
    return result;
  } catch (error) {
    console.error('查询到货单信息失败:', error);
    message.error('查询到货单信息失败');
    throw error;
  } finally {
    closeFullLoading();
  }
}

// 获取所有到货单信息（用于导出）
export async function getTableAllAsync(): Promise<BarcodeDto[]> {
  try {
    openFullLoading();
    // 这里需要调用一个获取所有数据的方法，如果没有的话，可以设置一个很大的分页参数
    const result = await _BarcodeListServiceProxy.getPagedBarcodeLists({
      pageIndex: 1,
      pageSize: 10000, // 设置一个很大的值来获取所有数据
    });
    return result.items || [];
  } catch (error) {
    console.error('获取所有到货单信息失败:', error);
    message.error('获取所有到货单信息失败');
    throw error;
  } finally {
    closeFullLoading();
  }
}
