using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TuTa.Wms.Erp.Entities;
using TuTa.Wms.Erp.IDto;
using Volo.Abp.Uow;

namespace TuTa.Wms.Erp
{
    public partial class ErpAsnAppService
    {
        /// <summary>按ASN逐行查询来料检验单，不用接口返回顺序推断来源。</summary>
        [UnitOfWork(IsDisabled = true)]
        public async Task<U8InspectionResultsDto> GetAsnInspectionResultsAsync(string asnCode)
        {
            var result = new U8InspectionResultsDto();
            if (string.IsNullOrWhiteSpace(asnCode)) { result.Message = "ASN码不能为空"; return result; }
            try
            {
                var lines = (await ReadAsnLinesAsync(asnCode.Trim())).Where(x => x.Status != AsnStatus.Cancelled).ToList();
                if (lines.Count == 0) { result.Message = "未找到ASN明细"; return result; }
                foreach (var line in lines) result.Items.Add(await QueryInspectionLineAsync(line));
                // 查询检验在组盘前即可进行，采购入库仍需整张ASN全部组盘校验完成。
                if (lines.Any(x => x.InWarehouseQuantity <= 0 || (x.AlreadyStockInQuantity ?? 0) != x.InWarehouseQuantity))
                    foreach (var row in result.Items) row.CanPurchase = false;
                result.Success = true;
                result.Message = "查询完成；仅来源匹配、已审核且整批合格的检验单可采购入库";
            }
            catch (Exception ex) { result.Message = "U8检验单查询失败：" + ex.Message; }
            return result;
        }

        /// <summary>兼容data、rows多层JSON字符串；异常结构不能被解释成空的合格结果。</summary>
        private static JToken DecodeU8Json(JToken token)
        {
            for (var depth = 0; token?.Type == JTokenType.String && depth < 4; depth++)
                token = JToken.Parse(token.Value<string>());
            return token;
        }

        /// <summary>读取U8大小写不一致的字段，避免绑定到默认0或空字符串。</summary>
        private static JToken InspectionField(JToken row, string name)
            => (row as JObject)?.GetValue(name, StringComparison.OrdinalIgnoreCase);

        /// <summary>U8数量可为字符串；格式无效时拒绝，不将其静默转换成0。</summary>
        private static decimal InspectionQuantity(JToken row, string name)
        {
            if (!decimal.TryParse(InspectionField(row, name)?.ToString(), NumberStyles.Number,
                    CultureInfo.InvariantCulture, out var quantity))
                throw new InvalidOperationException("检验单缺少有效数量：" + name);
            return quantity;
        }

        /// <summary>查询当前到货行；分页总时限30秒，最多20页，未查全不允许自动生单。</summary>
        private async Task<U8InspectionLineDto> QueryInspectionLineAsync(ErpAsn line)
        {
            var result = new U8InspectionLineDto { AsnLineId = line.Id, MaterialCode = line.MaterialCode,
                BatchCode = line.BatchCode, IsPurchasePushed = line.IsPurchasePushed };
            if (!line.IsInspectionReportPushed || string.IsNullOrWhiteSpace(line.ArrivalVoucherCode) || !line.ArrivalDetailId.HasValue)
            {
                result.Message = "请先完成到货单审核及来料报检"; return result;
            }
            var token = await GetErpTokenAsync(Guid.NewGuid().ToString());
            if (string.IsNullOrWhiteSpace(token)) throw new InvalidOperationException("U8登录失败");
            using var client = CreateU8Client();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var matches = new List<JToken>();
            var complete = false;
            const int size = 100;
            for (var page = 1; page <= 20; page++)
            {
                var filter = new JObject { ["cInvCode"] = line.MaterialCode, ["sourcecode"] = line.ArrivalVoucherCode,
                    ["pagesize"] = page, ["pagecount"] = size };
                var body = new JObject { ["Cmd"] = "GetLLJYDList",
                    ["Data"] = filter.ToString(Newtonsoft.Json.Formatting.None) };
                var response = await client.PostAsync(GetU8Endpoint("List"),
                    new StringContent(body.ToString(), Encoding.UTF8, "application/json"), deadline.Token);
                var responseText = await response.Content.ReadAsStringAsync(deadline.Token);
                if (!response.IsSuccessStatusCode) throw new InvalidOperationException("查询检验单HTTP错误：" + (int)response.StatusCode);
                var envelope = JObject.Parse(responseText);
                if (envelope["success"]?.Value<bool>() != true)
                    throw new InvalidOperationException(envelope["data"]?.ToString() ?? envelope["msg"]?.ToString());
                var data = DecodeU8Json(envelope["data"]);
                var rows = DecodeU8Json(InspectionField(data, "rows")) as JArray
                    ?? throw new InvalidOperationException("U8检验列表缺少rows数组");
                foreach (var row in rows)
                {
                    if (InspectionField(row, "SOURCECODE")?.ToString() == line.ArrivalVoucherCode &&
                        ReadU8Id(InspectionField(row, "SOURCEAUTOID")) == line.ArrivalDetailId &&
                        InspectionField(row, "CINVCODE")?.ToString() == line.MaterialCode &&
                        (InspectionField(row, "CBATCH")?.ToString() ?? "") == (line.BatchCode ?? ""))
                        matches.Add(row);
                }
                var total = InspectionField(data, "total");
                if (total == null || !int.TryParse(total.ToString(), out var count) || count < 0)
                    throw new InvalidOperationException("检验列表缺少有效total，无法确认分页完整性");
                var expected = Math.Min(size, Math.Max(count - (page - 1) * size, 0));
                if (rows.Count != expected)
                    throw new InvalidOperationException("U8检验列表分页数量与total不一致，不能使用不完整结果");
                if (page * size >= count) { complete = true; break; }
                if (rows.Count == 0) throw new InvalidOperationException("检验列表分页不完整，请核对U8查询条件");
            }
            if (!complete) throw new InvalidOperationException("检验列表超过20页，请收窄U8查询条件后重试，未使用不完整结果生单");
            // 重复行和多张检验单都需人工核对，不能自行选择第一条。
            if (matches.Count != 1)
            {
                result.Message = matches.Count == 0 ? "未找到与当前到货单、明细、物料及批次匹配的来料检验单，请等待检验完成"
                    : "存在多条匹配检验记录，请核对U8，暂不自动采购入库";
                return result;
            }
            var check = matches[0];
            result.CheckId = ReadU8Id(InspectionField(check, "ID"));
            result.CheckCode = InspectionField(check, "CCHECKCODE")?.ToString();
            result.SourceAutoId = ReadU8Id(InspectionField(check, "SOURCEAUTOID"));
            result.QualifiedQuantity = InspectionQuantity(check, "FREGQUANTITY");
            var verified = !string.IsNullOrWhiteSpace(InspectionField(check, "CVERIFIER")?.ToString());
            var rejected = string.Equals(InspectionField(check, "BREJFLAG")?.ToString(), "true", StringComparison.OrdinalIgnoreCase)
                || InspectionField(check, "BREJFLAG")?.ToString() == "1";
            var disqualified = InspectionQuantity(check, "FDISQUANTITY");
            result.CanGroup = result.CheckId.HasValue && !string.IsNullOrWhiteSpace(result.CheckCode) && verified &&
                !rejected && disqualified == 0 && result.QualifiedQuantity > 0 &&
                result.QualifiedQuantity == line.InWarehouseQuantity;
            result.GroupedQuantity = line.AlreadyStockInQuantity ?? 0;
            result.PendingGroupingQuantity = Math.Max(0, line.InWarehouseQuantity - result.GroupedQuantity);
            result.CanPurchase = result.CanGroup && result.GroupedQuantity == line.InWarehouseQuantity;
            result.Message = result.CanGroup ? line.PurchasePushError ?? (line.IsPurchasePushed ? "采购入库已推送" :
                result.PendingGroupingQuantity > 0 ? "检验已合格，请一箱一扫组盘" : "组盘完成，可推送采购入库")
                : "检验单未审核、非整批合格或数量不一致，禁止组盘及采购入库；部分合格请人工处理";
            return result;
        }

        /// <summary>按真实检验单生采购入库；本地样品未归还时不可推送，失败只重试U8。</summary>
        [UnitOfWork(IsDisabled = true)]
        public async Task<CGRKDAddResponseDto> PushInspectionPurchaseAsync(Guid asnLineId)
        {
            ErpAsn snapshot;
            using (var read = UnitOfWorkManager.Begin(true, false))
            {
                snapshot = await _erpAsnRepository.GetAsync(asnLineId);
                await read.CompleteAsync();
            }
            await LocalPushLock.WaitAsync();
            try
            {
                await using var handle = _u8Lock == null ? null : await _u8Lock.TryAcquireAsync("U8:ASN:" + snapshot.AsnCode);
                if (_u8Lock != null && handle == null)
                    return new CGRKDAddResponseDto { Success = false, Message = "该ASN正在处理，请稍后重试" };
                var asnLines = (await ReadAsnLinesAsync(snapshot.AsnCode)).Where(x => x.Status != AsnStatus.Cancelled).ToList();
                var line = asnLines.SingleOrDefault(x => x.Id == asnLineId);
                if (line == null) return new CGRKDAddResponseDto { Success = false, Message = "ASN已取消或不存在" };
                if (line.IsPurchasePushed) return new CGRKDAddResponseDto { Success = true, Message = "采购入库已推送，无需重复生成" };
                if (line.Status == AsnStatus.Cancelled) return new CGRKDAddResponseDto { Success = false, Message = "ASN已取消" };
                try
                {
                    if (asnLines.Any(x => x.InWarehouseQuantity <= 0 || (x.AlreadyStockInQuantity ?? 0) != x.InWarehouseQuantity))
                        return new CGRKDAddResponseDto { Success = false, Message = "整张ASN尚未全部组盘及校验完成，不能采购入库" };
                    var check = await QueryInspectionLineAsync(line);
                    if (!check.CanPurchase) return new CGRKDAddResponseDto { Success = false, Message = check.Message };
                    if (_u8StockRepository == null)
                        return new CGRKDAddResponseDto { Success = false, Message = "库存校验服务不可用，禁止采购入库" };
                    using (var read = UnitOfWorkManager.Begin(true, false))
                    {
                        var stocks = await _u8StockRepository.GetListAsync(x => x.U8AsnLineId == line.Id);
                        if (stocks.Count == 0 || stocks.Sum(x => x.TotalCountInTime) != line.InWarehouseQuantity ||
                            stocks.Any(x => (x.InspectionCount ?? 0) > 0 || x.InspectionStatus != Stocks.InspectionStatus.InspectionQualified))
                            return new CGRKDAddResponseDto { Success = false, Message = "组盘数量或合格状态不一致，请确认全部箱料及抽检样品已回盘" };
                        await read.CompleteAsync();
                    }
                    var payload = new JArray(new JObject {
                        ["AddType"] = 3, ["bIsRedVouch"] = false,
                        // 测试示例Base协议使用cARVCode；标准AddType=3使用cOrderCode，均指向真实检验单号。
                        ["cARVCode"] = check.CheckCode, ["cOrderCode"] = check.CheckCode,
                        ["cMemo"] = "WMS检验合格采购入库", ["cWhCode"] = line.WarehouseCode,
                        ["cRdCode"] = _erpSettings.PurchaseReceiptCategory ?? "301",
                        ["Details"] = new JArray(new JObject { ["iPOsID"] = check.SourceAutoId,
                            ["cInvCode"] = line.MaterialCode, ["cbatch"] = line.BatchCode,
                            ["iQuantity"] = check.QualifiedQuantity }) }).ToString(Newtonsoft.Json.Formatting.None);
                    if (string.IsNullOrWhiteSpace(line.WarehouseCode)) throw new InvalidOperationException("ASN未配置入库仓库");
                    if (line.PurchasePayload != null && !JToken.DeepEquals(JToken.Parse(line.PurchasePayload), JToken.Parse(payload)))
                        throw new InvalidOperationException("重试来源或数量已变更，请先核对U8，禁止更换原任务数据");
                    line.BeginPurchasePush(line.PurchaseTaskId ?? Guid.NewGuid().ToString("N"), payload);
                    await SaveU8LinesAsync(new List<ErpAsn> { line });
                    var result = await PushCGRKDAddAsync(new CGRKDAddRequestDto { Cmd = "CGRKDAdd",
                        Taskid = line.PurchaseTaskId, Id = check.CheckId.Value,
                        Maker = line.Maker ?? "WMS", Data = line.PurchasePayload });
                    using (var write = UnitOfWorkManager.Begin(true, true))
                    {
                        line.RecordPurchasePush(result.Success, result.Message, result.Data);
                        if (result.Success)
                        {
                            var stocks = await _u8StockRepository.GetListAsync(x => x.U8AsnLineId == line.Id);
                            foreach (var stock in stocks)
                            {
                                stock.ReturnToAvailable();
                                await _u8StockRepository.UpdateAsync(stock);
                            }
                        }
                        await _erpAsnRepository.UpdateAsync(line, autoSave: true);
                        await write.CompleteAsync();
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    line.RecordPurchasePush(false, ex.Message, null);
                    await SaveU8LinesAsync(new List<ErpAsn> { line });
                    return new CGRKDAddResponseDto { Success = false, Message = ex.Message };
                }
            }
            finally { LocalPushLock.Release(); }
        }
    }
}

