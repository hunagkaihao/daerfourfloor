import { FormSchema } from '/@/components/Table';
import { BasicColumn } from '/@/components/Table';
import { SelectItem } from '/@/utils/SelectItem';
import moment from 'moment';
import {
  StockServiceProxy
} from '/@/services/ServiceProxies';
import { message } from 'ant-design-vue';
import { useLoading } from '/@/components/Loading';
import { useI18n } from '/@/hooks/web/useI18n';
import { h } from 'vue';

const { t } = useI18n();
const [openFullLoading, closeFullLoading] = useLoading({
  tip: 'Loading...',
});
const _StockServiceProxy = new StockServiceProxy();

// 调整类型选项
export const adjustmentTypeSelectItem: SelectItem[] = [
  {
    label: '全部',
    value: null,
    key: 0,
  },
  {
    label: '增加',
    value: 1,
    key: 1,
  },
  {
    label: '减少',
    value: 2,
    key: 2,
  },
  {
    label: '清零',
    value: 3,
    key: 3,
  },
];

// 表格列配置
export const tableColumns: BasicColumn[] = [
  {
    title: t('物料编码'),
    dataIndex: 'materialCode',
    width: 120,
  },
  {
    title: t('物料名称'),
    dataIndex: 'materialName',
    width: 150,
  },
  {
    title: t('规格'),
    dataIndex: 'specs',
    width: 120,
  },
  {
    title: t('单位'),
    dataIndex: 'unit',
    width: 80,
  },
  {
    title: t('收料条形码'),
    dataIndex: 'barcode',
    width: 150,
  },
  {
    title: t('检验编号'),
    dataIndex: 'checkNo',
    width: 120,
  },
  {
    title: t('检验单号'),
    dataIndex: 'checkOrderCode',
    width: 120,
  },
  {
    title: t('调整前数量'),
    dataIndex: 'originalQuantity',
    width: 100,
  },
  {
    title: t('调整数量'),
    dataIndex: 'adjustmentQuantity',
    width: 100,
  },
  {
    title: t('调整类型'),
    dataIndex: 'adjustmentTypeDescription',
    width: 100,
  },
  {
    title: t('调整原因'),
    dataIndex: 'adjustmentReason',
    width: 150,
  },
  {
    title: t('操作人'),
    dataIndex: 'operatorName',
    width: 100,
  },
  {
    title: t('调整时间'),
    dataIndex: 'adjustmentTime',
    width: 150,
    customRender: ({ text }) => {
      if (text == null || text == undefined) {
        return '';
      }
      return moment(text).format('YYYY-MM-DD HH:mm:ss');
    },
  },
  {
    title: t('供应商编码'),
    dataIndex: 'supplierCode',
    width: 120,
  },
  {
    title: t('供应商名称'),
    dataIndex: 'supplierName',
    width: 150,
  },
  {
    title: t('批次号'),
    dataIndex: 'batchCode',
    width: 120,
  },
  {
    title: t('容器编号'),
    dataIndex: 'boxCode',
    width: 120,
  },
  {
    title: t('库位编号'),
    dataIndex: 'cellCode',
    width: 120,
  },
  {
    title: t('仓库编码'),
    dataIndex: 'warehouseCode',
    width: 120,
  },
  {
    title: t('仓库名称'),
    dataIndex: 'warehouseName',
    width: 120,
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
        danger: true,
        style: {
          margin: '2px',
          padding: '4px 8px',
          height: '24px',
          fontSize: '12px',
          borderRadius: '4px',
          border: '1px solid #ff4d4f',
          backgroundColor: '#ff4d4f',
          color: '#fff',
          cursor: 'pointer',
        },
        onClick: () => {
          window.dispatchEvent(new CustomEvent('stock-restore', { detail: record }));
        }
      }, '撤销');
    },
  },
];

// 搜索表单配置
export const searchFormSchema: FormSchema[] = [
  {
    field: 'materialCode',
    label: t('物料编码'),
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
    field: 'barcode',
    label: t('收料条形码'),
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
    field: 'checkOrderCode',
    label: t('检验单号'),
    component: 'Input',
    colProps: { span: 4 },
  },
  {
    field: 'adjustmentType',
    label: t('调整类型'),
    component: 'Select',
    colProps: { span: 4 },
    componentProps: {
      options: adjustmentTypeSelectItem
    }
  },
  {
    field: 'operatorName',
    label: t('操作人'),
    component: 'Input',
    colProps: { span: 4 },
  },
  {
    field: 'supplierCode',
    label: t('供应商编码'),
    component: 'Input',
    colProps: { span: 4 },
  },
  {
    field: 'supplierName',
    label: t('供应商名称'),
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
    field: 'warehouseCode',
    label: t('仓库编码'),
    component: 'Input',
    colProps: { span: 4 },
  },
  {
    field: 'warehouseName',
    label: t('仓库名称'),
    component: 'Input',
    colProps: { span: 4 },
  },
  {
    field: 'adjustmentTimeRange',
    component: 'RangePicker',
    label: t('调整时间'),
    labelWidth: 80,
    colProps: { span: 4 },
  },
];

/**
 * 分页获取库存调整记录
 * @param params
 * @returns
 */
export async function getStockAdjustmentsAsync(
  params: any
): Promise<any> {
  // 确保分页参数符合后端验证要求
  const queryParams = {
    pageIndex: 1,
    pageSize: 10,
    ...params,
  };
  
  // 确保pageSize在有效范围内
  if (queryParams.pageSize < 1) {
    queryParams.pageSize = 1;
  } else if (queryParams.pageSize > 1000) {
    queryParams.pageSize = 1000;
  }
  
  return _StockServiceProxy.getStockAdjustments(queryParams);
}

/**
 * 获取所有库存调整记录（用于导出）
 * @param params
 * @returns
 */
export async function getStockAdjustmentsAllAsync(
  params: any
): Promise<any> {
  // 设置合理的页面大小，符合后端验证要求
  const exportParams = {
    ...params,
    pageIndex: 1,
    pageSize: 1000, // 设置为后端允许的最大值
  };
  const result = await _StockServiceProxy.getStockAdjustments(exportParams);
  return result.items || [];
}

