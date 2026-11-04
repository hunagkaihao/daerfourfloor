import { FormSchema } from '/@/components/Table';
import { BasicColumn } from '/@/components/Table';
import { SelectItem } from '/@/utils/SelectItem';
import moment from 'moment';
import {
    StockInHistoryServiceProxy,
    StockInHistoryDto,
} from '/@/services/ServiceProxies';
import { message } from 'ant-design-vue';
import { useLoading } from '/@/components/Loading';
import { useI18n } from '/@/hooks/web/useI18n';
const { t } = useI18n();
const [openFullLoading, closeFullLoading] = useLoading({
  tip: 'Loading...',
});
const _StockInHistoryServiceProxy = new StockInHistoryServiceProxy()
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
/** 显示新记录业务阶段；旧记录保持未区分，不根据库位猜测业务结果。 */
export function getRecordStageText(stage?: string | null): string {
  const labels: Record<string, string> = { Grouping: '组盘登记', Completed: '上架完成', Direct: '直接入库' };
  return stage ? labels[stage] || stage : '历史记录／阶段未区分';
}

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
    dataIndex: 'materialSpecs',
  },
  {
    title: t('数量'),
    dataIndex: 'inCount',
  },
  {
    title: t('单位'),
    dataIndex: 'materialUnit',
  },
  {
    title: t('生产批号'),
    dataIndex: 'batchNo',
  },
  {
    title: t('入库时间'),
    dataIndex: 'inTime',
    customRender: ({ text }) => {
         return moment(text).format('YYYY-MM-DD HH:mm:ss');
     },
  },
   
  {
    title: t('收料码'),
    dataIndex: 'barcode',
  },
  {
    title: t('容器编号'),
    dataIndex: 'boxCode',
  },
  {
    title: t('记录阶段'),
    dataIndex: 'recordStage',
    customRender: ({ text }) => getRecordStageText(text),
  },
  {
    title: t('起始库位编号'),
    dataIndex: 'startCellCode',
    customRender: ({ text }) => text || '—',
  },
  {
    title: t('起始库位名称'),
    dataIndex: 'startCellName',
    customRender: ({ text }) => text || '—',
  },
  {
    title: t('起始容器编号'),
    dataIndex: 'startBoxCode',
    customRender: ({ text }) => text || '—',
  },
  {
    title: t('起始容器名称'),
    dataIndex: 'startBoxName',
    customRender: ({ text }) => text || '—',
  },
  {
    title: t('目标库位编号'),
    dataIndex: 'endCellCode',
    customRender: ({ text }) => text || '—',
  },
  {
    title: t('目标库位名称'),
    dataIndex: 'endCellName',
    customRender: ({ text }) => text || '—',
  },
  {
    title: t('目标容器编号'),
    dataIndex: 'endBoxCode',
    customRender: ({ text }) => text || '—',
  },
  {
    title: t('目标容器名称'),
    dataIndex: 'endBoxName',
    customRender: ({ text }) => text || '—',
  },
  {
    title: t('关联AGV任务'),
    dataIndex: 'agvTaskReqCode',
    customRender: ({ text }) => text || '—',
  },
  {
    title: t('容器名称'),
    dataIndex: 'boxName',
  },
  {
    title: t('库位编号'),
    dataIndex: 'cellCode',
  },
  {
    title: t('库位名称'),
    dataIndex: 'cellName',
  },
  {
    title: t('区域编号'),
    dataIndex: 'areaCode',
  },
  {
    title: t('区域名称'),
    dataIndex: 'areaName',
  },
  {
    title: t('仓库编号'),
    dataIndex: 'warehouseCode',
  },
  {
    title: t('仓库名称'),
    dataIndex: 'warehouseName',
  },
  {
    title: t('入库类型'),
    dataIndex: 'stockInType',
  },
  {
    title: t('环保要求'),
    dataIndex: 'isHB',
    customRender: ({ text }) => {
      return text ? '是' : '否';
    },
  },
  {
    title: t('保质期'),
    dataIndex: 'expiryDate',
    customRender: ({ text }) => {
      return text ? moment(text).format('YYYY-MM-DD') : '';
    },
  },
  {
    title: t('备料单'),
    dataIndex: 'blCode',
  },
  {
    title: t('备货单号'),
    dataIndex: 'bhCode',
  },
  {
    title: t('操作者'),
    dataIndex: 'operatorName',
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
    field: 'batchNo',
    label: t('生产批号'),
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
    componentProps:{
      options:[
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

/**
 * 分页列表
 * @param params 查询和分页条件；入库日期包含起止日期整天，空筛选不发送
 * @returns 当前页记录和匹配总数
 */
export async function getTableListAsync(
  params
): Promise<any> {

  const query = { ...params };
  if (query.time?.length === 2) {
    [query.stockInTimeStart, query.stockInTimeEnd] = query.time;
  }
  delete query.time;
  if (query.stockInTimeStart) {
    query.stockInTimeStart = moment(query.stockInTimeStart).startOf('day').format('YYYY-MM-DD HH:mm:ss');
  }
  if (query.stockInTimeEnd) {
    query.stockInTimeEnd = moment(query.stockInTimeEnd).endOf('day').format('YYYY-MM-DD HH:mm:ss');
  }
  Object.keys(query).forEach((key) => {
    if (query[key] === '' || query[key] == null) delete query[key];
  });
  return _StockInHistoryServiceProxy.pagedStockInHistoriesGet(query);
}


/**
 * 按当前时间及物料筛选逐页获取导出记录，不受表格当前页码或每页条数限制。
 * @param params 当前搜索条件，调用方需提供完整入库时间范围
 * @returns 所有匹配记录；中途请求失败或分页数据不完整时抛错，避免导出残缺文件
 */
export async function allStockInHistoriesGet(
  params
): Promise<StockInHistoryDto[]> {
  const query = { ...params, pageIndex: 1, pageSize: 1000 };
  const records: StockInHistoryDto[] = [];
  let totalCount = 0;
  do {
    const result = await getTableListAsync(query);
    const items = result.items || [];
    totalCount = result.totalCount;
    if (items.length === 0 && records.length < totalCount) {
      throw new Error('入库历史数据已变化，请重新导出');
    }
    records.push(...items);
    query.pageIndex += 1;
  } while (records.length < totalCount);
  return records;
}
