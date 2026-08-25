import { ERP_ASNServiceProxy } from '/@/services/ServiceProxies';

const asnService = new ERP_ASNServiceProxy();

export async function getAsnList(params: {
  page: number;
  pageSize: number;
  asnCode?: string;
  supplierName?: string;
  startDate?: string;
  endDate?: string;
  status?: number;
}) {
  const { page, pageSize, asnCode, supplierName, startDate, endDate, status } = params;
  return asnService.list(page, pageSize, asnCode, supplierName, startDate, endDate, status);
}

export async function pushReceipt(asnCode: string) {
  return asnService.pushReceipt(asnCode);
}
