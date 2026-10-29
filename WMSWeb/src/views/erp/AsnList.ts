import { getAsnList, pushReceipt, pushAsnArrival, pushAsnInspection, pushAsnOrderArrival, pushAsnOrderInspection } from '/@/api/erp/asn';
import type { AsnWorkflowLine } from '/@/api/erp/asn';
import { BasicColumn } from '/@/components/Table';
import { Tag } from 'ant-design-vue';
import { message } from 'ant-design-vue';
import moment from 'moment';
import { h } from 'vue';

const wrapTextCell = {
  ellipsis: false,
  customCell: () => ({
    style: {
      whiteSpace: 'normal',
      wordBreak: 'break-word',
      lineHeight: '1.4',
    },
  }),
};

const STATUS_COLOR_MAP: Record<number, string> = {
  1: 'blue',
  2: 'processing',
  3: 'success',
  4: 'default',
};

function formatQuantity(value?: number | null) {
  if (value === null || value === undefined || Number.isNaN(Number(value))) {
    return '0';
  }
  return String(Number(value));
}

const STATUS_NAME_MAP: Record<number, string> = {
  1: '已创建',
  2: '收货中',
  3: '已完成',
  4: '已取消',
};

function getStatusName(record: Recordable) {
  if (record.statusSummary) {
    return record.statusSummary;
  }
  if (record.status && STATUS_NAME_MAP[record.status]) {
    return STATUS_NAME_MAP[record.status];
  }
  if (record.statusName === '已收货') {
    return '收货中';
  }
  return record.statusName || '-';
}

function renderStatusTag(record: Recordable) {
  const color = STATUS_COLOR_MAP[record.status] || 'default';
  return h(Tag, { color }, () => getStatusName(record));
}

export interface AsnGroupRow {
  rowKey: string;
  ccode: string;
  cvenabbname?: string;
  cvencode?: string;
  cwhcode?: string;
  cwhname?: string;
  ddate?: string;
  darridate?: string;
  cmaker?: string;
  headcmemo?: string;
  cbustype?: string;
  statusSummary?: string;
  status?: number;
  totalAlreadyStockInQuantity: number;
  totalPendingStockInQuantity: number;
  lineCount: number;
  /** 完整ASN明细，包含取消行供追溯。 */
  asnItems: AsnWorkflowLine[];
  /** 是否允许生成/重试到货单。 */
  canPushArrival: boolean;
  /** 是否允许生成/重试来料报检单。 */
  canPushInspection: boolean;
  /** 到货单推送状态和关联单号。 */
  arrivalStatus: string;
  arrivalVoucherCode: string;
  pushTime?: string;
  arrivalPushError: string;
  /** 来料报检单推送状态及最近失败原因。 */
  inspectionStatus: string;
  inspectionPushError: string;
  /** 当前送货单各采购订单的物料检验状态汇总。 */
  materialInspectionStatusName?: string;
}

export const parentTableColumns: BasicColumn[] = [
  {
    title: 'ASN码',
    dataIndex: 'ccode',
    width: 180,
  },
  {
    title: '供应商简称',
    dataIndex: 'cvenabbname',
    width: 150,
  },
  {
    title: '供应商编码',
    dataIndex: 'cvencode',
    width: 100,
  },
  {
    title: '仓库编码',
    dataIndex: 'cwhcode',
    width: 100,
  },
  {
    title: '仓库名称',
    dataIndex: 'cwhname',
    width: 150,
  },
  {
    title: '明细行数',
    dataIndex: 'lineCount',
    width: 90,
  },
  {
    title: '组盘状态',
    dataIndex: 'statusSummary',
    width: 100,
    customRender: ({ record }) => renderStatusTag(record),
  },
  {
    title: '已组盘总数',
    dataIndex: 'totalAlreadyStockInQuantity',
    width: 110,
    customRender: ({ record }) => formatQuantity(record.totalAlreadyStockInQuantity),
  },
  {
    title: '待组盘总数',
    dataIndex: 'totalPendingStockInQuantity',
    width: 130,
    ellipsis: false,
    customRender: ({ record }) => formatQuantity(record.totalPendingStockInQuantity),
  },
  {
    title: '制单日期',
    dataIndex: 'darridate',
    width: 120,
  },
  {
    title: '审核日期',
    dataIndex: 'ddate',
    width: 120,
  },
  {
    title: '制单人',
    dataIndex: 'cmaker',
    width: 100,
  },
  {
    title: '业务类型',
    dataIndex: 'cbustype',
    width: 120,
  },
  {
    title: '表头备注',
    dataIndex: 'headcmemo',
    width: 120,
  },
];

parentTableColumns.splice(9, 0,
  { title: 'U8到货单状态', dataIndex: 'arrivalStatus', width: 130 },
  { title: 'U8到货单号', dataIndex: 'arrivalVoucherCode', width: 180 },
  { title: '到货推送时间', dataIndex: 'pushTime', width: 170 },
  { title: '到货失败原因', dataIndex: 'arrivalPushError', width: 240, ...wrapTextCell },
  { title: '报检单状态', dataIndex: 'inspectionStatus', width: 130 },
  { title: '物料检验状态', dataIndex: 'materialInspectionStatusName', width: 160 },
  { title: '报检失败原因', dataIndex: 'inspectionPushError', width: 240, ...wrapTextCell },
);

export const detailTableColumns: BasicColumn[] = [
  {
    title: '采购订单号',
    dataIndex: 'cordercode',
    width: 180,
  },
  {
    title: '物料编码',
    dataIndex: 'cinvcode',
    width: 150,
  },
  {
    title: '物料名称',
    dataIndex: 'cinvname',
    width: 200,
    ...wrapTextCell,
  },
  {
    title: '规格型号',
    dataIndex: 'cinvstd',
    width: 150,
    ...wrapTextCell,
  },
  {
    title: '计量单位',
    dataIndex: 'cinfvm_unit',
    width: 80,
  },
  {
    title: '应组盘数量',
    dataIndex: 'iquantity',
    width: 110,
    customRender: ({ record }) => formatQuantity(record.iquantity),
  },
  {
    title: '已组盘数量',
    dataIndex: 'alreadyStockInQuantity',
    width: 120,
    customRender: ({ record }) => formatQuantity(record.alreadyStockInQuantity),
  },
  {
    title: '待组盘数量',
    dataIndex: 'pendingStockInQuantity',
    width: 110,
    customRender: ({ record }) => formatQuantity(record.pendingStockInQuantity),
  },
  {
    title: '组盘状态',
    dataIndex: 'statusName',
    width: 100,
    customRender: ({ record }) => renderStatusTag(record),
  },
  {
    title: '批次号',
    dataIndex: 'cbatch',
    width: 120,
  },
  {
    title: '发货日期',
    dataIndex: 'dshipdate',
    width: 120,
  },
  {
    title: '采购类型',
    dataIndex: 'cptname',
    width: 120,
  },
  {
    title: '部门名称',
    dataIndex: 'cdepname',
    width: 120,
  },
  {
    title: '备注',
    dataIndex: 'cmemo',
    width: 120,
  },
];

detailTableColumns.push(
  { title: '物料检验状态', dataIndex: 'materialInspectionStatusName', width: 120 },
  { title: 'U8到货单号', dataIndex: 'arrivalVoucherCode', width: 180 },
  { title: 'U8到货明细ID', dataIndex: 'arrivalDetailId', width: 140 },
);

export const searchFormSchema = [
  {
    field: 'asnCode',
    label: 'ASN码',
    component: 'Input',
    colProps: { xs: 24, sm: 12, xl: 6 },
    componentProps: {
      placeholder: '请输入ASN码',
    },
  },
  {
    field: 'supplierName',
    label: '供应商名称',
    component: 'Input',
    colProps: { xs: 24, sm: 12, xl: 6 },
    componentProps: {
      placeholder: '请输入供应商名称',
    },
  },
  {
    field: 'supplierCode',
    label: '供应商编码',
    component: 'Input',
    colProps: { xs: 24, sm: 12, xl: 6 },
    componentProps: {
      placeholder: '请输入供应商编码',
      allowClear: true,
    },
  },
  {
    field: 'status',
    label: '状态',
    component: 'Select',
    colProps: { xs: 24, sm: 12, xl: 6 },
    componentProps: {
      allowClear: true,
      placeholder: '请选择状态',
      options: [
        { label: '已创建', value: 1 },
        { label: '收货中', value: 2 },
        { label: '已完成', value: 3 },
        { label: '已取消', value: 4 },
      ],
    },
  },
  {
    field: 'dateRange',
    label: '日期范围',
    component: 'RangePicker',
    colProps: { xs: 24, sm: 24, xl: 12 },
    defaultValue: [moment().subtract(7, 'days'), moment().add(1, 'days')],
  },
];

function groupAsnListItems(items: AsnWorkflowLine[] = []): AsnGroupRow[] {
  const groupMap = new Map<string, AsnWorkflowLine[]>();

  items.forEach((item) => {
    const code = item.ccode || '';
    if (!groupMap.has(code)) {
      groupMap.set(code, []);
    }
    groupMap.get(code)!.push(item);
  });

  return Array.from(groupMap.entries()).map(([ccode, lines]) => {
    const first = lines[0] || ({} as AsnWorkflowLine);
    const activeLines = lines.filter((line) => line.status !== 4);
    const allPushed = activeLines.length > 0 && activeLines.every((line) => line.isPushedToErp);
    const anyPushed = activeLines.some((line) => line.isPushedToErp);

    const arrivalPushError = [...new Set(activeLines.map((line) => line.arrivalPushError).filter(Boolean))].join('；');
    const inspectionPushError = [...new Set(activeLines.map((line) => line.inspectionPushError).filter(Boolean))].join('；');
    const allReported = activeLines.length > 0 && activeLines.every((line) => line.isInspectionReportPushed);
    const anyReported = activeLines.some((line) => line.isInspectionReportPushed);
    const statusLabels = [
      ...new Set(lines.map((line) => getStatusName(line)).filter((name) => name !== '-')),
    ];
    const totalAlreadyStockInQuantity = activeLines.reduce(
      (sum, line) => sum + (Number(line.alreadyStockInQuantity) || 0),
      0,
    );
    const totalPendingStockInQuantity = activeLines.reduce(
      (sum, line) => sum + (Number(line.pendingStockInQuantity) || 0),
      0,
    );

    return {
      rowKey: ccode,
      ccode,
      cvenabbname: first.cvenabbname,
      cvencode: first.cvencode,
      cwhcode: first.cwhcode,
      cwhname: first.cwhname,
      ddate: first.ddate,
      darridate: first.darridate,
      cmaker: first.cmaker,
      headcmemo: first.headcmemo,
      cbustype: first.cbustype,
      statusSummary:
        statusLabels.length === 1
          ? statusLabels[0]
          : statusLabels.length > 1
            ? '多种状态'
            : '-',
      status: statusLabels.length === 1 ? lines[0]?.status : undefined,
      totalAlreadyStockInQuantity,
      totalPendingStockInQuantity,
      lineCount: lines.length,
      asnItems: lines,
      canPushArrival: activeLines.length > 0 && activeLines.every(line => Number(line.iquantity) > 0) && !allPushed,
      canPushInspection: allPushed && !allReported && activeLines.every((line) =>
        !!line.arrivalVoucherId && !!line.arrivalVoucherCode && !!line.arrivalDetailId),
      arrivalStatus: allPushed ? '已推送' : anyPushed ? '部分已推送' : arrivalPushError ? '推送失败' : '待推送',
      arrivalVoucherCode: [...new Set(activeLines.map((line) => line.arrivalVoucherCode).filter(Boolean))].join('、'),
      pushTime: activeLines.find((line) => line.pushTime)?.pushTime,
      arrivalPushError,
      inspectionStatus: allReported ? '已推送' : anyReported ? '部分已推送' : inspectionPushError ? '推送失败' : '待推送',
      inspectionPushError,
      materialInspectionStatusName: [...new Set(activeLines.map(line => line.materialInspectionStatusName || '待检'))].join('、'),
    };
  });
}

export async function getAsnListAsync(params: any) {
  try {
    const { page, pageSize, asnCode, supplierName, supplierCode, status, dateRange } = params;

    let startDate = '';
    let endDate = '';
    if (dateRange && dateRange.length === 2) {
      startDate = moment(dateRange[0]).format('YYYY-MM-DD');
      endDate = moment(dateRange[1]).format('YYYY-MM-DD');
    }

    const result = await getAsnList({
      page,
      pageSize,
      asnCode,
      supplierName,
      supplierCode: supplierCode?.trim() || undefined,
      status,
      startDate,
      endDate,
    });

    const groupedItems = groupAsnListItems(result.items || []);

    return {
      items: groupedItems,
      total: result.total,
    };
  } catch (error) {
    message.error('获取ASN列表失败');
    return {
      items: [],
      total: 0,
    };
  }
}

export async function pushErpReceiptAsync(asnCode: string) {
  try {
    const result = await pushReceipt(asnCode);
    if (result.success) {
      message.success('推送成功');
    } else {
      message.error('推送失败');
    }
    return result;
  } catch (error) {
    message.error('推送失败');
    return { success: false };
  }
}

/** 展示U8具体结果，网络错误也保留失败语义供页面决定是否刷新。 */
async function pushAsnDocument(action: () => ReturnType<typeof pushAsnArrival>) {
  try {
    const result = await action();
    if (result.success) message.success(result.message || '推送成功');
    else message.error(result.message || '推送失败');
    return result;
  } catch (error: any) {
    message.error(error?.response?.data?.error?.message || error?.message || '推送失败');
    return { success: false };
  }
}

/** 不传订单时推送整张ASN；明细入口限定所属采购订单。 */
export function pushAsnArrivalAsync(asnCode: string, orderCode?: string) {
  return pushAsnDocument(() => orderCode ? pushAsnOrderArrival(asnCode, orderCode) : pushAsnArrival(asnCode));
}

/** 来源关联齐全后报检，明细入口只处理所属订单对应的到货单。 */
export function pushAsnInspectionAsync(asnCode: string, orderCode?: string) {
  return pushAsnDocument(() => orderCode ? pushAsnOrderInspection(asnCode, orderCode) : pushAsnInspection(asnCode));
}
