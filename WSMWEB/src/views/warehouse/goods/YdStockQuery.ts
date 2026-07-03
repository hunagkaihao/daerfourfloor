import { FormSchema } from '/@/components/Table';
import { BasicColumn } from '/@/components/Table';
import {
  StockServiceProxy,
  PagedStockQueryDto
} from '/@/services/ServiceProxies';
import { useI18n } from '/@/hooks/web/useI18n';

const { t } = useI18n();
const _StockServiceProxy = new StockServiceProxy();

export const ydStockTableColumns: BasicColumn[] = [
  {
    title: t('物料编号'),
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
    title: t('库存数量'),
    dataIndex: 'totalCountInTime',
    width: 100,
  },
  {
    title: t('检验编号'),
    dataIndex: 'checkNo',
    width: 120,
  },
  {
    title: t('检验时间'),
    dataIndex: 'checkDate',
    width: 120,
  },
  {
    title: t('容器码'),
    dataIndex: 'boxCode',
    width: 120,
  },
  {
    title: t('收料码'),
    dataIndex: 'barcode',
    width: 150,
  },
];

export const ydStockSearchFormSchema: FormSchema[] = [
  {
    field: 'workshop',
    label: t('车间'),
    component: 'Select',
    colProps: { span: 8 },
    componentProps: {
      placeholder: '请选择车间',
      options: [
        { label: '一车间', value: '一车间' },
        { label: '二车间', value: '二车间' },
        { label: '三车间', value: '三车间' },
        { label: '五车间', value: '五车间' },
      ],
    },
  },
  {
    field: 'materialCode',
    label: t('物料编号'),
    component: 'Input',
    colProps: { span: 8 },
    componentProps: {
      placeholder: '请输入物料编号',
    },
  },
];

export async function getYdStockTableListAsync(params: any) {
  console.log('YD库存查询参数:', params);
  
  // 构建查询参数
  const queryDto = new PagedStockQueryDto();
  queryDto.workshop = params.workshop || undefined;
  queryDto.materialCode = params.materialCode || undefined;
  
  // 分页参数
  queryDto.pageIndex = params.pageNo || 1;
  queryDto.pageSize = params.pageSize || 10;
  queryDto.skipCount = ((params.pageNo || 1) - 1) * (params.pageSize || 10);
  queryDto.maxResultCount = params.pageSize || 10;

  try {
    const result = await _StockServiceProxy.getPagedYDStocks(queryDto);
    return {
      items: result.items || [],
      totalCount: result.totalCount || 0,
    };
  } catch (error) {
    console.error('YD库存查询失败:', error);
    throw error;
  }
}
