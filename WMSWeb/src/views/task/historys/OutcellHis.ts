import { FormSchema } from '/@/components/Table';
import { BasicColumn } from '/@/components/Table';
import { SelectItem } from '/@/utils/SelectItem';
import moment from 'moment';
import {
    StockOutHistoryServiceProxy,
    ChkResultListServiceProxy,
    StockOutHistoryDto,
} from '/@/services/ServiceProxies';
import { message } from 'ant-design-vue';
import { useLoading } from '/@/components/Loading';
import { useI18n } from '/@/hooks/web/useI18n';
const { t } = useI18n();
const [openFullLoading, closeFullLoading] = useLoading({
  tip: 'Loading...',
});
const _StockOutHistoryServiceProxy = new StockOutHistoryServiceProxy()
const _ChkResultListServiceProxy = new ChkResultListServiceProxy()
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
/** 只显示明确记录的出库阶段，旧行不按库位或类型推断完成状态。 */
export function getRecordStageText(stage?: string | null): string {
  const labels: Record<string, string> = { Completed: '出库完成', Direct: '直接出库' };
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
    dataIndex: 'specs',
  },
  {
    title: t('数量'),
    dataIndex: 'stockOutCount',
  },
  {
    title: t('生产批号'),
    dataIndex: 'pickBatch',
  },
  {
    title: t('出库时间'),
    dataIndex: 'stockOutTime',
    customRender: ({ text }) => {
         return moment(text).format('YYYY-MM-DD HH:mm:ss');
     },
  },
   
  {
    title: t('收料码'),
    dataIndex: 'barcode',
  },
  {
    title: t('单位'),
    dataIndex: 'unit',
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
    dataIndex: 'houseCode',
  },
  {
    title: t('仓库名称'),
    dataIndex: 'houseName',
  },
  {
    title: t('检验时间'),
    dataIndex: 'checkDate',
    customRender: ({ text }) => {
      return moment(text).format('YYYY-MM-DD HH:mm:ss');
  },
  },
  // {
  //   title: t('检验类型'),
  //   dataIndex: 'checkType',
  // },
  {
    title: t('检验编号'),
    dataIndex: 'checkOrderCode',
  },
  {
    title: t('检验结果'),
    dataIndex: 'checkResult',
  },

  
  {
    title: t('供应商送货号'),
    dataIndex: 'supplierCode',
  },
  {
    title: t('供应商名称'),
    dataIndex: 'supplierName',
  },
  {
    title: t('出库类型'),
    dataIndex: 'stockOutType',
  },
  {
    title: t('产品编号'),
    dataIndex: 'goodsCode',
  },
  {
    title: t('加工产品'),
    dataIndex: 'goodsName',
  },
  {
    title: t('产品规格'),
    dataIndex: 'goodsSpecs',
  },
  {
    title: t('环保要求'),
    dataIndex: 'isHB',
  },
  {
    title: t('保质期'),
    dataIndex: 'expiryDate',
  },
  {
    title: t('领出人'),
    dataIndex: 'pickManName',
  },
  
  // {
  //   title: t('批号'),
  //   dataIndex: 'batchCode',
  // },
  // {
  //   title: t('对应备料单号'),
  //   dataIndex: 'blCode',
  // },
  // {
  //   title: t('对应备货单号'),
  //   dataIndex: 'bhCode',
  // },
  {
    title: t('操作者'),
    dataIndex: 'operator',
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
    field: 'materialCode',
    label: t('物料编号'),
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'pickBatchTip',
    label: t('生产批号'),
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
    field: 'barcode',
    label: t('收料码'),
    component: 'Input',
    colProps: { span: 6 },
  },
  {
    field: 'stockOutType',
    label: t('出库类型'),
    component: 'Select',
    colProps: { span: 6 },
    componentProps:{
      options:[
        {
          label: '生产领用',
          value: '1',

        },
        {
          label: '超计划l领用',
          value: '14',
        },
        {
            label: '无计划领用',
            value: '15',
          },
          {
            label: '外协领用',
            value: '2',
          },        
        ]
    }
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



/**
 * 分页列表
 * @param params 查询和分页条件；出库日期覆盖起止日期整天，空筛选不发送
 * @returns 当前页记录和匹配总数
 */
export async function getTableListAsync(
  params
): Promise<any> {

  const query = { ...params };
  if (query.time?.length === 2) {
    [query.stockOutTimeMin, query.stockOutTimeMax] = query.time;
  }
  delete query.time;
  if (query.stockOutTimeMin) {
    query.stockOutTimeMin = moment(query.stockOutTimeMin).startOf('day').format('YYYY-MM-DD HH:mm:ss');
  }
  if (query.stockOutTimeMax) {
    query.stockOutTimeMax = moment(query.stockOutTimeMax).endOf('day').format('YYYY-MM-DD HH:mm:ss');
  }
  Object.keys(query).forEach((key) => {
    if (query[key] === '' || query[key] == null) delete query[key];
  });
  return _StockOutHistoryServiceProxy.pagedStockOutHistoriesGet(query);
}
/** 按当前时间及其他筛选逐页取全出库记录；中途失败或分页中断时抛错，避免导出残缺文件。 */
export async function allStockOutHistoriesGet(
  params
): Promise<StockOutHistoryDto[]> {
  const query = { ...params, pageIndex: 1, pageSize: 1000 };
  const records: StockOutHistoryDto[] = [];
  let totalCount = 0;
  do {
    const result = await getTableListAsync(query);
    const items = result.items || [];
    totalCount = result.totalCount;
    if (items.length === 0 && records.length < totalCount) {
      throw new Error('出库历史数据已变化，请重新导出');
    }
    records.push(...items);
    query.pageIndex += 1;
  } while (records.length < totalCount);
  return records;
}
//重新入库

export async function checkDataCreateByOutHistory({ id}) {
  try {
    openFullLoading();
    await  _ChkResultListServiceProxy.checkDataCreateByOutHistory(id).then((res)=>{
      message.success(res.message);
    })
    closeFullLoading();
    
  } catch (error) {
    message.error(error.error.message);
    closeFullLoading();
  }
}
