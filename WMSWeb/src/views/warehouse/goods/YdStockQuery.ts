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
    field: 'materialCode',
    label: t('物料编号'),
    component: 'Input',
    colProps: { span: 8 },
    componentProps: {
      placeholder: '请输入物料编号',
    },
  },
  {
    field: 'cellCode',
    label: t('库位编码'),
    component: 'Input',
    colProps: { span: 8 },
    componentProps: {
      placeholder: '请输入库位编码',
    },
  },
  {
    field: 'barcode',
    label: t('收料码'),
    component: 'Input',
    colProps: { span: 8 },
    componentProps: {
      placeholder: '请输入收料码',
    },
  },
];

export async function getYdStockTableListAsync(params: any) {
  const queryDto = new PagedStockQueryDto();
  queryDto.materialCode = params.materialCode || undefined;
  queryDto.cellCode = params.cellCode || undefined;
  queryDto.barcode = params.barcode || undefined;
  queryDto.pageIndex = params.pageNo || 1;
  queryDto.pageSize = params.pageSize || 10;

  try {
    const result = await _StockServiceProxy.pagedStocksQuery(queryDto);
    return {
      items: result.items || [],
      totalCount: result.totalCount || 0,
    };
  } catch (error) {
    console.error('库存查询失败:', error);
    throw error;
  }
}
