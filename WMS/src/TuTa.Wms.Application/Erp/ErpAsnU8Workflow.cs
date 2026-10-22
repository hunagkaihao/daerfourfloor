using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json.Linq;
using TuTa.Wms.Erp.Dto;
using TuTa.Wms.Erp.Entities;
using TuTa.Wms.Erp.IDto;
using Volo.Abp.Uow;

namespace TuTa.Wms.Erp
{
    public partial class ErpAsnAppService
    {
        /// <summary>查询已保存的U8发货记录及推送结果，支持跨会话补录和失败重试。</summary>
        public async Task<List<ErpOutboundRecordDto>> GetSalesRecordsAsync(string deliveryOrderNo)
        {
            if (string.IsNullOrWhiteSpace(deliveryOrderNo)) return new List<ErpOutboundRecordDto>();
            return (await _u8OutboundRepository.GetListAsync(x => x.DeliveryOrderNo == deliveryOrderNo.Trim()))
                .Select(x => new ErpOutboundRecordDto {
                    Id = x.Id, Warehouse = x.Warehouse, CustomerCode = x.CustomerCode, MasterId = x.MasterId,
                    Quantity = x.Quantity, QtyPerBox = x.QtyPerBox, MaterialCode = x.MaterialCode,
                    Package = x.Package, Grade = x.Grade, LabelText = x.LabelText, DeliveryOrderNo = x.DeliveryOrderNo,
                    ActualOutboundQuantity = x.ActualOutboundQuantity, CreationTime = x.CreationTime,
                    SourceDetailId = x.SourceDetailId, SalesDepartmentCode = x.SalesDepartmentCode,
                    IsSalesPushed = x.IsSalesPushed, SalesPushError = x.SalesPushError,
                    IsDeliveryScanCompleted = x.IsDeliveryScanCompleted, SalesPushTime = x.SalesPushTime
                }).ToList();
        }
        /// <summary>确认发货单所有明细已扫描，未确认前禁止仅凭当前已录入行生成销售单。</summary>
        [UnitOfWork(IsDisabled = true)]
        public async Task<CGRKDAddResponseDto> ConfirmDeliveryScanAsync(string deliveryOrderNo)
        {
            if (string.IsNullOrWhiteSpace(deliveryOrderNo)) return new CGRKDAddResponseDto { Success = false, Message = "发货单号不能为空" };
            await LocalPushLock.WaitAsync();
            try
            {
                await using var handle = _u8Lock == null ? null : await _u8Lock.TryAcquireAsync("U8:SALES:" + deliveryOrderNo.Trim());
                if (_u8Lock != null && handle == null) return new CGRKDAddResponseDto { Success = false, Message = "该发货单正在处理，请稍后重试" };
                var rows = await ReadSalesLinesAsync(deliveryOrderNo);
                if (rows.Count == 0) return new CGRKDAddResponseDto { Success = false, Message = "请先扫描发货单明细" };
                foreach (var row in rows) row.ConfirmDeliveryScan();
                await SaveSalesLinesAsync(rows);
                return new CGRKDAddResponseDto { Success = true, Message = "整张发货单扫码完成，全部实际出库后可推送U8" };
            }
            finally { LocalPushLock.Release(); }
        }

        /// <summary>补充二维码未携带的发货明细ID和销售部门；仅影响U8生单。</summary>
        [UnitOfWork(IsDisabled = true)]
        public async Task<CGRKDAddResponseDto> SetSalesSourceAsync(string recordId, long sourceDetailId, string departmentCode)
        {
            ErpOutboundRecord snapshot;
            using (var read = UnitOfWorkManager.Begin(true, false))
            {
                snapshot = await _u8OutboundRepository.GetAsync(recordId);
                await read.CompleteAsync();
            }
            await LocalPushLock.WaitAsync();
            try
            {
                await using var handle = _u8Lock == null ? null : await _u8Lock.TryAcquireAsync("U8:SALES:" + snapshot.DeliveryOrderNo);
                if (_u8Lock != null && handle == null) return new CGRKDAddResponseDto { Success = false, Message = "该发货单正在处理，请稍后重试" };
                using (var write = UnitOfWorkManager.Begin(true, true))
                {
                    var record = await _u8OutboundRepository.GetAsync(recordId);
                    record.SetU8SalesSource(sourceDetailId, departmentCode);
                    await _u8OutboundRepository.UpdateAsync(record, autoSave: true);
                    await write.CompleteAsync();
                }
                return new CGRKDAddResponseDto { Success = true, Message = "U8发货来源已补充" };
            }
            finally { LocalPushLock.Release(); }
        }

        /// <summary>整张发货单实际出库完成后生成销售出库单；失败时保留本地出库，允许独立重试。</summary>
        [UnitOfWork(IsDisabled = true)]
        public async Task<CGRKDAddResponseDto> PushSalesOutboundAsync(string deliveryOrderNo)
        {
            if (string.IsNullOrWhiteSpace(deliveryOrderNo)) return new CGRKDAddResponseDto { Success = false, Message = "发货单号不能为空" };
            await LocalPushLock.WaitAsync();
            try
            {
                await using var handle = _u8Lock == null ? null : await _u8Lock.TryAcquireAsync("U8:SALES:" + deliveryOrderNo.Trim());
                if (_u8Lock != null && handle == null) return new CGRKDAddResponseDto { Success = false, Message = "该发货单正在推送" };
                var lines = await ReadSalesLinesAsync(deliveryOrderNo);
                if (lines.Count == 0 || lines.Any(x => !x.IsDeliveryScanCompleted || x.Quantity <= 0 || x.ActualOutboundQuantity != x.Quantity))
                    return new CGRKDAddResponseDto { Success = false, Message = "请确认整单扫码完成，并等待所有明细实际出库完成" };
                if (lines.All(x => x.IsSalesPushed)) return new CGRKDAddResponseDto { Success = true, Message = "销售出库单已推送，无需重复生成" };
                var uid = lines.Select(x => x.SalesPushTaskId).FirstOrDefault(x => x.HasValue) ?? GeneratePuArrVouchUid();
                CGRKDAddResponseDto result;
                try
                {
                    if (lines.Any(x => x.IsSalesPushed)) throw new InvalidOperationException("历史销售推送状态不一致，禁止重复生单");
                    var first = lines[0];
                    if (lines.Any(x => x.SourceDetailId <= 0 || !x.SourceDetailId.HasValue))
                        throw new InvalidOperationException("缺少U8发货来源明细ID(iSodid)，请补充；二维码主表ID不能替代");
                    if (string.IsNullOrWhiteSpace(first.SalesDepartmentCode) || lines.Any(x => x.Warehouse != first.Warehouse ||
                        x.CustomerCode != first.CustomerCode || x.SalesDepartmentCode != first.SalesDepartmentCode))
                        throw new InvalidOperationException("发货单仓库、客户或销售部门缺失/不一致");
                    var details = new JArray();
                    foreach (var line in lines)
                    {
                        if (line.QtyPerBox <= 0 || string.IsNullOrWhiteSpace(line.ActualOutboundDetailsJson))
                            throw new InvalidOperationException("缺少实际出库批次或每箱数量，禁止用计划数量代替实际明细");
                        var actual = JArray.Parse(line.ActualOutboundDetailsJson);
                        if (actual.Any(x => string.IsNullOrWhiteSpace(x["batch"]?.ToString()) || x["quantity"]?.Value<decimal>() <= 0) ||
                            actual.Sum(x => x["quantity"].Value<decimal>()) != line.Quantity)
                            throw new InvalidOperationException("实际出库明细与发货数量不一致");
                        foreach (var row in actual)
                            details.Add(JObject.FromObject(new { iSodid = line.SourceDetailId.Value, irowno = details.Count + 1,
                                cInvCode = line.MaterialCode, cFree5 = line.Grade, cFree3 = line.Package, cFree2 = line.LabelText,
                                cBatch = row["batch"]?.ToString(), iQuantity = row["quantity"].Value<decimal>(),
                                iNum = row["quantity"].Value<decimal>() / line.QtyPerBox, iPUnitCost = 0, iPPrice = 0 }));
                    }
                    foreach (var line in lines) line.RecordSalesPush(uid, false, line.SalesPushError);
                    await SaveSalesLinesAsync(lines);
                    var data = new JArray(JObject.FromObject(new { AddType = 1, bIsRedVouch = false,
                        cWhCode = first.Warehouse, cCusCode = first.CustomerCode, cBusCode = first.DeliveryOrderNo,
                        cRdCode = "203", cDepCode = first.SalesDepartmentCode, dDate = DateTime.Today.ToString("yyyy/MM/dd"),
                        cMaker = "WMS", cMemo = "PDA实际出库", Details = details }));
                    var body = JObject.FromObject(new { Cmd = "XSCKDAdd", Taskid = uid.ToString(),
                        Params = new { Json = data.ToString(Newtonsoft.Json.Formatting.None) } });
                    var token = await GetErpTokenAsync(Guid.NewGuid().ToString());
                    if (string.IsNullOrWhiteSpace(token)) throw new InvalidOperationException("U8登录失败");
                    using var client = CreateU8Client();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var response = await client.PostAsync(GetU8Endpoint("Cmd"), new StringContent(body.ToString(), Encoding.UTF8, "application/json"));
                    var json = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode) throw new InvalidOperationException($"U8销售出库失败(HTTP {(int)response.StatusCode})：{json}");
                    var returned = JObject.Parse(json);
                    result = new CGRKDAddResponseDto { Success = returned["success"]?.Value<bool>() ?? false,
                        Message = returned["success"]?.Value<bool>() == true ? "销售出库单推送成功" : returned["data"]?.ToString() ?? returned["msg"]?.ToString(),
                        Data = returned["data"]?.ToString() };
                }
                catch (Exception ex) { result = new CGRKDAddResponseDto { Success = false, Message = ex.Message }; }
                foreach (var line in lines.Where(x => !x.IsSalesPushed)) line.RecordSalesPush(uid, result.Success, result.Message);
                await SaveSalesLinesAsync(lines);
                return result;
            }
            finally { LocalPushLock.Release(); }
        }

        /// <summary>独立提交U8销售推送状态，不影响已完成的库存出库事务。</summary>
        private async Task SaveSalesLinesAsync(List<ErpOutboundRecord> lines)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                foreach (var line in lines) await _u8OutboundRepository.UpdateAsync(line, autoSave: true);
                await uow.CompleteAsync();
            }
        }
        /// <summary>取U8命令地址；未配置时由登录服务的同站根路径推导，兼容配置的Cmd/Base地址。</summary>
        private string GetU8Endpoint(string suffix)
        {
            var root = _erpSettings.U8ApiUrl;
            if (string.IsNullOrWhiteSpace(root))
                root = new Uri(new Uri(_erpSettings.LoginUrl), "/API/U8API").ToString();
            root = root.TrimEnd('/');
            foreach (var ending in new[] { "/Base", "/Cmd", "/List" })
                if (root.EndsWith(ending, StringComparison.OrdinalIgnoreCase)) root = root.Substring(0, root.Length - ending.Length);
            return root + (string.IsNullOrEmpty(suffix) ? "" : "/" + suffix);
        }

        /// <summary>按新Base协议转发已明确来源的采购入库单，不查询尚未确定的来料检验接口。</summary>
        private async Task<CGRKDAddResponseDto> PushPurchaseBaseAsync(CGRKDAddRequestDto input)
        {
            try
            {
                if (input.Cmd != "CGRKDAdd" || input.Params != null || string.IsNullOrWhiteSpace(input.Taskid))
                    return new CGRKDAddResponseDto { Success = false, Message = "新采购入库报文必须指定CGRKDAdd、Taskid，且不能混用Params" };
                var vouchers = JArray.Parse(input.Data);
                if (vouchers.Count == 0 || vouchers.Any(x => string.IsNullOrWhiteSpace(x["cARVCode"]?.ToString()) ||
                    (x["Details"] as JArray)?.Count == 0 || x["Details"] == null))
                    return new CGRKDAddResponseDto { Success = false, Message = "采购入库单缺少来源单号或明细；检验单接口待客户确认，禁止使用本地ASN的GUID代替" };
                var token = await GetErpTokenAsync(Guid.NewGuid().ToString());
                if (string.IsNullOrWhiteSpace(token)) return new CGRKDAddResponseDto { Success = false, Message = "U8登录失败" };
                using var client = CreateU8Client();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var body = Newtonsoft.Json.JsonConvert.SerializeObject(new { Cmd = "CGRKDAdd", input.Taskid, input.Id, input.Maker, input.Data });
                var response = await client.PostAsync(GetU8Endpoint("Base"), new StringContent(body, Encoding.UTF8, "application/json"));
                var json = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    return new CGRKDAddResponseDto { Success = false, Message = $"U8采购入库失败(HTTP {(int)response.StatusCode})：{json}" };
                var data = JObject.Parse(json);
                return new CGRKDAddResponseDto { Success = data["success"]?.Value<bool>() ?? false,
                    Message = data["success"]?.Value<bool>() == true ? "采购入库推送成功" : data["data"]?.ToString() ?? data["msg"]?.ToString(),
                    Code = data["code"]?.Value<int>() ?? 0, Data = data["data"]?.ToString() };
            }
            catch (Exception ex) { return new CGRKDAddResponseDto { Success = false, Message = "采购入库推送失败：" + ex.Message }; }
        }
        // 主机已注册Redis分布式锁；无分布式锁的单机/测试环境串行化整个推送流程。
        internal static readonly SemaphoreSlim LocalPushLock = new SemaphoreSlim(1, 1);

        /// <summary>独立只读工作单元读取ASN快照，网络请求期间不持有数据库事务。</summary>
        private async Task<List<ErpAsn>> ReadAsnLinesAsync(string asnCode)
        {
            using (var uow = UnitOfWorkManager.Begin(true, false))
            {
                var rows = await _erpAsnRepository.GetListByAsnCodeAsync(asnCode);
                await uow.CompleteAsync();
                return rows;
            }
        }

        /// <summary>独立只读工作单元读取U8发货明细快照。</summary>
        private async Task<List<ErpOutboundRecord>> ReadSalesLinesAsync(string deliveryOrderNo)
        {
            using (var uow = UnitOfWorkManager.Begin(true, false))
            {
                var rows = await _u8OutboundRepository.GetListAsync(x => x.DeliveryOrderNo == deliveryOrderNo.Trim());
                await uow.CompleteAsync();
                return rows;
            }
        }

        /// <summary>按完整ASN推送到货单；本地组盘成功后独立执行，失败可在管理页重试。</summary>
        [UnitOfWork(IsDisabled = true)]
        public async Task<PuArrVouchAddResponseDto> PushAsnArrivalAsync(string asnCode)
        {
            if (string.IsNullOrWhiteSpace(asnCode))
                return new PuArrVouchAddResponseDto { Success = false, Message = "ASN码不能为空" };
            asnCode = asnCode.Trim();
            await LocalPushLock.WaitAsync();
            try
            {
                await using var handle = _u8Lock == null ? null : await _u8Lock.TryAcquireAsync("U8:ASN:" + asnCode);
                if (_u8Lock != null && handle == null)
                    return new PuArrVouchAddResponseDto { Success = false, Message = "该ASN正在推送，请稍后刷新" };
                return await PushAsnArrivalCoreAsync(asnCode);
            }
            finally { LocalPushLock.Release(); }
        }

        /// <summary>校验实际组盘数量、保存稳定UID、生成到货单并持久化U8关联。</summary>
        private async Task<PuArrVouchAddResponseDto> PushAsnArrivalCoreAsync(string asnCode)
        {
            var lines = (await ReadAsnLinesAsync(asnCode))
                .Where(x => x.Status != AsnStatus.Cancelled).ToList();
            if (lines.Count == 0)
                return new PuArrVouchAddResponseDto { Success = false, Message = "没有可推送的ASN明细" };
            if (lines.All(x => x.IsPushedToErp))
                return new PuArrVouchAddResponseDto { Success = true, Message = "已生成到货单，无需重复推送" };
            if (lines.Any(x => x.IsPushedToErp))
                return new PuArrVouchAddResponseDto { Success = false, Message = "历史推送状态不一致，请核对U8单据后处理，禁止重复生单" };
            // 新流程先按已保存ASN生成到货，检验合格后才逐箱组盘。
            if (lines.Any(x => x.InWarehouseQuantity <= 0))
                return new PuArrVouchAddResponseDto { Success = false, Message = "ASN到货数量必须大于0" };
            var first = lines[0];
            if (lines.Any(x => x.SupplierCode != first.SupplierCode))
                return new PuArrVouchAddResponseDto { Success = false, Message = "同一ASN供应商不一致" };
            var uid = lines.Select(x => x.ArrivalPushUid).FirstOrDefault(x => x.HasValue) ?? GeneratePuArrVouchUid();
            foreach (var line in lines) line.BeginArrivalPush(uid);
            await SaveU8LinesAsync(lines);
            var request = new PuArrVouchAddRequestDto {
                Uid = uid, CAsnCode = asnCode, CVenCode = first.SupplierCode, Cpocode = first.OrderCode,
                DepartmentCode = first.DepartmentCode, Maker = first.Maker, ProcessTypeCode = first.ProcessTypeCode,
                ExchangeName = first.ExchangeName, TaxRate = first.TaxRate, ExchangeRate = first.ExchangeRate,
                Data = lines.Select(x => new PuArrVouchDetailRequestDto {
                    CInvCode = x.MaterialCode, CBatch = x.BatchCode, Cordercode = x.OrderCode,
                    IPoDetailId = x.PoDetailId, WarehouseCode = x.WarehouseCode, Bgsp = x.IsGsp ? 1 : 0,
                    IQuantity = x.InWarehouseQuantity, FRealQuantity = x.InWarehouseQuantity,
                    INum = 1, FRealNumy = 1, CFree2 = x.Free2, CFree3 = x.Free3, CFree5 = x.Free5
                }).ToList()
            };
            var result = await PushPuArrVouchAsync(request);
            if (result.Success)
            {
                JToken data;
                try
                {
                    data = result.ErpData == null ? new JObject() : JToken.FromObject(result.ErpData);
                    if (data.Type == JTokenType.String) data = JToken.Parse(data.Value<string>());
                    if (data.Type != JTokenType.Object) data = new JObject();
                }
                catch (Newtonsoft.Json.JsonException) { data = new JObject(); }
                var entries = (data["entry"] ?? data["Entry"] ?? data["Details"]) as JArray ?? new JArray();
                foreach (var line in lines)
                {
                    var matches = entries.Where(x =>
                        string.Equals(x["cinvcode"]?.ToString(), line.MaterialCode, StringComparison.OrdinalIgnoreCase) &&
                        (x["cbatch"]?.ToString() ?? "") == (line.BatchCode ?? "") &&
                        ReadU8Id(x["iposid"]) == line.PoDetailId).ToList();
                    // 仅在来源明确时绑定明细，不能凭数组顺序或物料编码猜测来源ID。
                    var detailId = matches.Count == 1 ? ReadU8Id(matches[0]["autoid"]) : null;
                    line.CompleteArrivalPush(ReadU8Id(data["id"]), data["ccode"]?.ToString(), detailId);
                }
                if (lines.Any(x => !x.ArrivalDetailId.HasValue))
                    result.Message += "；部分U8来源明细未能关联，请核对到货单，暂不能报检";
            }
            else foreach (var line in lines) line.FailArrivalPush(result.Message);
            await SaveU8LinesAsync(lines);
            return result;
        }

        /// <summary>兼容U8数值和字符串ID；缺失或无效时不猜测来源。</summary>
        private static long? ReadU8Id(JToken value)
        {
            return long.TryParse(value?.ToString(), out var id) && id > 0 ? id : (long?)null;
        }

        /// <summary>持久化网络调用前后的状态；独立提交使UID在超时后仍可用于重试。</summary>
        private async Task SaveU8LinesAsync(List<ErpAsn> lines)
        {
            using (var uow = UnitOfWorkManager.Begin(true, true))
            {
                foreach (var line in lines) await _erpAsnRepository.UpdateAsync(line, autoSave: true);
                await uow.CompleteAsync();
            }
        }

        /// <summary>仓管员在U8审核到货单后主动报检；未审核错误保留失败状态供后续重试。</summary>
        [UnitOfWork(IsDisabled = true)]
        public async Task<LLBJDAddResponseDto> PushAsnInspectionReportAsync(string asnCode)
        {
            if (string.IsNullOrWhiteSpace(asnCode))
                return new LLBJDAddResponseDto { Success = false, Message = "ASN码不能为空" };
            asnCode = asnCode.Trim();
            await LocalPushLock.WaitAsync();
            try
            {
                await using var handle = _u8Lock == null ? null : await _u8Lock.TryAcquireAsync("U8:ASN:" + asnCode);
                if (_u8Lock != null && handle == null)
                    return new LLBJDAddResponseDto { Success = false, Message = "该ASN正在推送，请稍后刷新" };
                var lines = (await ReadAsnLinesAsync(asnCode))
                    .Where(x => x.Status != AsnStatus.Cancelled).ToList();
                if (lines.Count == 0 || lines.Any(x => !x.IsPushedToErp || x.ArrivalVoucherId == null ||
                    string.IsNullOrWhiteSpace(x.ArrivalVoucherCode) || !x.ArrivalDetailId.HasValue))
                    return new LLBJDAddResponseDto { Success = false, Message = "请先生成并关联完整U8到货单" };
                if (lines.All(x => x.IsInspectionReportPushed))
                    return new LLBJDAddResponseDto { Success = true, Message = "来料报检单已推送" };
                if (lines.Any(x => x.IsInspectionReportPushed))
                    return new LLBJDAddResponseDto { Success = false, Message = "历史报检状态不一致，请核对U8单据" };
                var uid = lines.Select(x => x.InspectionPushUid).FirstOrDefault(x => x.HasValue) ?? GeneratePuArrVouchUid();
                foreach (var line in lines) line.RecordInspectionPush(uid, false, line.InspectionPushError);
                await SaveU8LinesAsync(lines);
                var result = await PushLLBJDAddAsync(new LLBJDAddRequestDto {
                    Cmd = "LLBJDAdd", TaskId = uid.ToString(), Id = lines[0].ArrivalVoucherId.Value,
                    Maker = lines[0].Maker ?? "WMS",
                    Data = lines.GroupBy(x => x.ArrivalVoucherCode).Select(g => new LLBJDDataItem {
                        AddType = 1, CSourceCode = g.Key, CInspectDepCode = lines[0].DepartmentCode,
                        CMemo = "WMS到货审核后报检", Details = g.Select(x => new LLBJDDetail {
                            SourceAutoId = x.ArrivalDetailId.Value, CInvCode = x.MaterialCode,
                            CBatch = x.BatchCode, FQuantity = x.InWarehouseQuantity
                        }).ToList()
                    }).ToList()
                });
                // 按现场业务约定：U8用该错误表示到货单尚未审核；保留失败状态和任务号供审核后重试。
                if (!result.Success && result.Message?.Contains("到货单信息为空", StringComparison.Ordinal) == true)
                    result.Message = "到货单尚未由仓管员在U8审核，请等待审核通过后重新推送来料报检单。U8返回：" + result.Message;
                foreach (var line in lines) line.RecordInspectionPush(uid, result.Success, result.Message);
                await SaveU8LinesAsync(lines);
                return result;
            }
            finally { LocalPushLock.Release(); }
        }
    }
}
