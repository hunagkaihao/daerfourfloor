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
using Microsoft.Extensions.Logging;
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
                result.Success = true;
                result.Message = "查询完成；各单据独立核对组盘数量，账实一致且U8检验单来源完整、已审核合格后可逐单采购入库";
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

        /// <summary>复核当前到货行的检验单；已确认来源按主键和单号精确查找，历史未选来源须查全分页以排除歧义。</summary>
        private async Task<U8InspectionLineDto> QueryInspectionLineAsync(ErpAsn line)
        {
            var result = new U8InspectionLineDto { AsnLineId = line.Id, MaterialCode = line.MaterialCode,
                BatchCode = line.BatchCode, IsPurchasePushed = line.IsPurchasePushed,
                CanGroup = line.IsInspectionReportPushed && line.MaterialInspectionStatus == Stocks.InspectionStatus.InspectionQualified,
                GroupedQuantity = line.AlreadyStockInQuantity ?? 0,
                PendingGroupingQuantity = Math.Max(0, line.InWarehouseQuantity - (line.AlreadyStockInQuantity ?? 0)) };
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
            var selectedCheckId = line.VerifiedInspectionId;
            var selectedCheckCode = line.VerifiedInspectionCode;
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
                        (InspectionField(row, "CBATCH")?.ToString() ?? "") == (line.BatchCode ?? "") &&
                        (!selectedCheckId.HasValue ||
                            (ReadU8Id(InspectionField(row, "ID")) == selectedCheckId &&
                             InspectionField(row, "CCHECKCODE")?.ToString() == selectedCheckCode)))
                        matches.Add(row);
                }
                var total = InspectionField(data, "total");
                if (total == null || !int.TryParse(total.ToString(), out var count) || count < 0)
                    throw new InvalidOperationException("检验列表缺少有效total，无法确认分页完整性");
                var expected = Math.Min(size, Math.Max(count - (page - 1) * size, 0));
                if (rows.Count != expected)
                    throw new InvalidOperationException("U8检验列表分页数量与total不一致，不能使用不完整结果");
                _logger.LogInformation("U8采购前复核检验单：ASN明细{AsnLineId}，目标检验单{CheckId}，第{Page}页，共{Total}条，本页匹配{Matches}条",
                    line.Id, selectedCheckId, page, count, matches.Count);
                // 已由PDA确认的主键唯一定位单据；仍严格核对本页完整性、来源及后续最新审核结果。
                if (selectedCheckId.HasValue && matches.Count > 0) { complete = true; break; }
                if (page * size >= count) { complete = true; break; }
                if (rows.Count == 0) throw new InvalidOperationException("检验列表分页不完整，请核对U8查询条件");
            }
            if (!complete) throw new InvalidOperationException("检验列表超过20页，请收窄U8查询条件后重试，未使用不完整结果生单");
            // 无明确选择时不得自行取第一张；指定主键后仍拒绝同页重复的记录。
            if (matches.Count != 1)
            {
                result.Message = (result.CanGroup ? "本地已确认合格，可组盘；" : "请在物料状态变更页确认合格；") +
                    (matches.Count == 0 ? "未找到匹配的U8来料检验单，暂不能采购入库"
                    : "存在多条匹配检验记录，请核对U8，暂不自动采购入库");
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
            var u8Qualified = result.CheckId.HasValue && !string.IsNullOrWhiteSpace(result.CheckCode) && verified &&
                !rejected && disqualified == 0 && result.QualifiedQuantity > 0 &&
                result.QualifiedQuantity == line.InWarehouseQuantity;
            result.CanPurchase = result.CanGroup && u8Qualified && result.GroupedQuantity == line.InWarehouseQuantity;
            result.Message = !result.CanGroup
                ? $"当前{GetMaterialInspectionStatusName(line.MaterialInspectionStatus)}，请在物料状态变更页确认合格后组盘"
                : !u8Qualified ? "本地已确认合格，可逐箱组盘；U8检验单未审核、非整批合格或数量不一致，暂不能采购入库"
                : line.PurchasePushError ?? (line.IsPurchasePushed ? "采购入库已推送" :
                    result.PendingGroupingQuantity > 0 ? "人工检验已合格，请一箱一扫组盘" : "组盘完成，可推送采购入库");
            return result;
        }

        /// <summary>按当前明细对应的真实检验单独立生成采购入库；只核对该明细账实和样品归还，不等待其他ASN明细。</summary>
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
                    if (line.InWarehouseQuantity <= 0 || (line.AlreadyStockInQuantity ?? 0) != line.InWarehouseQuantity)
                        return new CGRKDAddResponseDto { Success = false, Message = "当前单据对应明细尚未组盘完成或数量不一致，不能采购入库" };
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
                    var checkCode = check.CheckCode.Trim();
                    // Base样例的纯数字来源单号按整数发送；含字母或前导零的真实单号保留字符串，避免改写单号。
                    JToken sourceOrderCode = long.TryParse(checkCode, NumberStyles.None, CultureInfo.InvariantCulture, out var numericCode) &&
                        numericCode.ToString(CultureInfo.InvariantCulture) == checkCode ? new JValue(numericCode) : new JValue(checkCode);
                    var payload = new JArray(new JObject {
                        ["AddType"] = 3, ["blsRedVouch"] = false,
                        ["cARVCode"] = checkCode, ["cOrderCode"] = sourceOrderCode,
                        ["cpersoncode"] = string.IsNullOrWhiteSpace(line.PersonCode) ? JValue.CreateNull() : new JValue(line.PersonCode),
                        ["cMemo"] = "Webservice生成", ["cWhCode"] = line.WarehouseCode,
                        ["cRdCode"] = _erpSettings.PurchaseReceiptCategory ?? "301",
                        ["Details"] = new JArray(new JObject { ["iPOsID"] = check.SourceAutoId,
                            ["cinvcode"] = line.MaterialCode, ["cbatch"] = line.BatchCode,
                            ["iQuantity"] = check.QualifiedQuantity.ToString(CultureInfo.InvariantCulture),
                            ["iUnitCost"] = 0 }) }).ToString(Newtonsoft.Json.Formatting.None);
                    if (string.IsNullOrWhiteSpace(line.WarehouseCode)) throw new InvalidOperationException("ASN未配置入库仓库");
                    if (line.PurchasePayload != null && !JToken.DeepEquals(JToken.Parse(line.PurchasePayload), JToken.Parse(payload)))
                        throw new InvalidOperationException("当前来源、数量或报文格式与原采购任务不一致，请先核对U8原任务，禁止自动更换任务号或报文");
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

