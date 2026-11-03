using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TuTa.Wms.Erp.IDto;
using Volo.Abp.Uow;

namespace TuTa.Wms.Erp
{
    public partial class ErpAsnAppService
    {
        /// <summary>查询U8来料检验单并将当前页新增或变化的记录保存到本地；内容一致时跳过。</summary>
        /// <param name="input">分页及可选存货编码、到货单、制单开始日期；包含开始日期当天及之后的单据。</param>
        /// <returns>已保存的U8检验记录及可关联的本地ASN来源；不修改人工检验结论或重置采购完成标记。</returns>
        [UnitOfWork(IsDisabled = true)]
        public async Task<U8MaterialInspectionResultDto> GetMaterialInspectionResultsAsync(U8MaterialInspectionQueryDto input)
        {
            input ??= new U8MaterialInspectionQueryDto();
            if (input.Page < 1 || input.PageSize < 1 || input.PageSize > 100)
                return new U8MaterialInspectionResultDto { MaterialCode = input.MaterialCode?.Trim(), Page = input.Page,
                    PageSize = input.PageSize, Message = "页码必须从1开始，每页条数必须为1至100" };
            try
            {
                var token = await GetErpTokenAsync(Guid.NewGuid().ToString());
                if (string.IsNullOrWhiteSpace(token)) throw new InvalidOperationException("U8登录失败");
                using var client = CreateU8Client();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var result = await ReadU8MaterialInspectionPageAsync(input, client, CancellationToken.None);
                if (result.Success)
                {
                    try
                    {
                        var saved = await SaveMaterialInspectionItemsAsync(result.Items);
                        _logger.LogInformation("WMS来料检验单查询落库：第{Page}页，返回{Count}条，新增{Inserted}条，更新{Updated}条，一致跳过{Unchanged}条，回填采购完成{Completed}条",
                            result.Page, result.Items.Count, saved.Inserted, saved.Updated, saved.Unchanged, saved.Completed);
                        if (result.Items.Count > 0) result.Message += "；查询结果已同步到本地";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "WMS来料检验单查询成功但落库失败：第{Page}页，原因={Reason}", result.Page, ex.Message);
                        result.Success = false;
                        result.Message = "U8查询成功，但本地保存失败：" + ex.Message;
                        return result;
                    }
                    await AttachInspectionAsnSourcesAsync(result.Items);
                }
                return result;
            }
            catch (Exception ex)
            {
                return new U8MaterialInspectionResultDto { MaterialCode = input.MaterialCode?.Trim(), Page = input.Page,
                    PageSize = input.PageSize, Message = "U8来料检验单查询失败：" + ex.Message };
            }
        }

        /// <summary>复用已登录客户端读取一页U8数据；后台同步遍历全页时不重复登录或读取本地ASN。</summary>
        /// <param name="input">U8筛选及分页条件。</param>
        /// <param name="client">已设置Bearer令牌的U8客户端，由调用方释放。</param>
        /// <param name="cancellationToken">宿主停止时取消请求；每页另设30秒时限。</param>
        /// <returns>当前页记录；失败保留明确原因，不伪装成空页。</returns>
        private async Task<U8MaterialInspectionResultDto> ReadU8MaterialInspectionPageAsync(
            U8MaterialInspectionQueryDto input, HttpClient client, CancellationToken cancellationToken)
        {
            input ??= new U8MaterialInspectionQueryDto();
            var result = new U8MaterialInspectionResultDto {
                MaterialCode = input.MaterialCode?.Trim(), Page = input.Page, PageSize = input.PageSize
            };
            if (result.Page < 1 || result.PageSize < 1 || result.PageSize > 100)
            {
                result.Message = "页码必须从1开始，每页条数必须为1至100";
                return result;
            }
            try
            {
                var sourceCode = input.SourceCode?.Trim();
                using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                deadline.CancelAfter(TimeSpan.FromSeconds(30));
                var items = new List<U8MaterialInspectionItemDto>();
                // 沿用U8既有协议：pagesize为页码，pagecount为本页条数。
                var filter = new JObject { ["pagesize"] = result.Page, ["pagecount"] = result.PageSize };
                if (!string.IsNullOrWhiteSpace(result.MaterialCode)) filter["cInvCode"] = result.MaterialCode;
                if (!string.IsNullOrWhiteSpace(sourceCode)) filter["sourcecode"] = sourceCode;
                if (input.BeginMakeTime.HasValue)
                    // 开始条件按U8本地日期取零点，包含所选日期当天及之后的检验单。
                    filter["begMakeTime"] = input.BeginMakeTime.Value.Date.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                var body = new JObject { ["Cmd"] = "GetLLJYDList", ["Data"] = filter.ToString(Formatting.None) };
                using var response = await client.PostAsync(GetU8Endpoint("List"),
                    new StringContent(body.ToString(Formatting.None), Encoding.UTF8, "application/json"), deadline.Token);
                var text = await response.Content.ReadAsStringAsync(deadline.Token);
                if (!response.IsSuccessStatusCode)
                    throw new InvalidOperationException("U8来料检验单查询HTTP错误：" + (int)response.StatusCode);
                var envelope = JObject.Parse(text);
                if (envelope["success"]?.Value<bool>() != true)
                    throw new InvalidOperationException(envelope["data"]?.ToString() ?? envelope["msg"]?.ToString() ?? "U8查询失败");
                var data = DecodeU8Json(envelope["data"]);
                var rows = DecodeU8Json(InspectionField(data, "rows")) as JArray
                    ?? throw new InvalidOperationException("U8检验列表缺少rows数组");
                if (!int.TryParse(InspectionField(data, "total")?.ToString(), out var total) || total < 0)
                    throw new InvalidOperationException("U8检验列表缺少有效total，无法确认查询完整性");
                var offset = (long)(result.Page - 1) * result.PageSize;
                var expected = Math.Min(result.PageSize, Math.Max(total - offset, 0));
                if (rows.Count != expected)
                    throw new InvalidOperationException("U8检验列表分页数量与total不一致，请重试查询");
                foreach (var row in rows)
                {
                    if (!(row is JObject)) throw new InvalidOperationException("U8检验明细格式错误");
                    var materialCode = InspectionField(row, "CINVCODE")?.ToString();
                    // 仅核对已填写的条件；默认查询保留每条记录的真实物料码。
                    if ((!string.IsNullOrWhiteSpace(result.MaterialCode) && materialCode != result.MaterialCode) ||
                        (!string.IsNullOrWhiteSpace(sourceCode) && InspectionField(row, "SOURCECODE")?.ToString() != sourceCode)) continue;
                    var rejected = InspectionField(row, "BREJFLAG")?.ToString();
                    items.Add(new U8MaterialInspectionItemDto {
                        CheckId = ReadU8Id(InspectionField(row, "ID")),
                        CheckCode = InspectionField(row, "CCHECKCODE")?.ToString(),
                        SourceCode = InspectionField(row, "SOURCECODE")?.ToString(),
                        SourceAutoId = ReadU8Id(InspectionField(row, "SOURCEAUTOID")),
                        MaterialCode = materialCode, BatchCode = InspectionField(row, "CBATCH")?.ToString(),
                        QualifiedQuantity = ReadOptionalInspectionQuantity(row, "FREGQUANTITY"),
                        DisqualifiedQuantity = ReadOptionalInspectionQuantity(row, "FDISQUANTITY"),
                        Verifier = InspectionField(row, "CVERIFIER")?.ToString(),
                        IsVerified = !string.IsNullOrWhiteSpace(InspectionField(row, "CVERIFIER")?.ToString()),
                        IsRejected = string.Equals(rejected, "true", StringComparison.OrdinalIgnoreCase) || rejected == "1" ? true :
                            string.Equals(rejected, "false", StringComparison.OrdinalIgnoreCase) || rejected == "0" ? false : (bool?)null
                    });
                }
                result.TotalCount = total;
                result.HasMore = offset + rows.Count < total;
                result.Success = true;
                result.HasInspection = items.Count > 0;
                result.Items = items;
                result.Message = result.HasInspection ? $"第{result.Page}页查询到{items.Count}条对应的U8来料检验记录" :
                    result.HasMore || result.Page > 1 ? $"第{result.Page}页未查询到对应记录" : "未查询到符合条件的U8来料检验单";
                if (result.HasMore) result.Message += "；还有后续记录，可点击加载更多或补充筛选条件";
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (OperationCanceledException) { result.Message = "U8来料检验单单页查询超时，请稍后重试，或补充物料码、制单开始日期及减小每页条数后查询"; }
            catch (Exception ex) { result.Message = "U8来料检验单查询失败：" + ex.Message; }
            return result;
        }

        /// <summary>按到货单、来源明细、物料和批号关联当前本地ASN，不把单据存在解释为人工检验合格。</summary>
        /// <param name="items">待补充ASN来源的检验单记录。</param>
        private async Task AttachInspectionAsnSourcesAsync(List<U8MaterialInspectionItemDto> items)
        {
            var materialCodes = items.Select(item => item.MaterialCode).Where(code => !string.IsNullOrWhiteSpace(code)).Distinct().ToList();
            if (materialCodes.Count == 0) return;
            using var read = UnitOfWorkManager.Begin(true, false);
            var localLines = await _erpAsnRepository.GetInspectionSourcesAsync(materialCodes);
            foreach (var item in items)
            {
                var matches = localLines.Where(line => !string.IsNullOrWhiteSpace(item.SourceCode) && item.SourceAutoId.HasValue &&
                    line.MaterialCode == item.MaterialCode && line.ArrivalVoucherCode == item.SourceCode &&
                    line.ArrivalDetailId == item.SourceAutoId && (line.BatchCode ?? "") == (item.BatchCode ?? "")).ToList();
                if (matches.Select(line => line.AsnCode).Distinct().Count() != 1) continue;
                item.AsnCode = matches[0].AsnCode;
                item.AsnLines = matches.Select(MapToErpAsnDto).ToList();
            }
            await read.CompleteAsync();
        }

        /// <summary>读取可选数量，缺少字段时保留未知值，非法值不得伪装成0。</summary>
        private static decimal? ReadOptionalInspectionQuantity(JToken row, string name)
        {
            var value = InspectionField(row, name);
            return value == null || value.Type == JTokenType.Null ? (decimal?)null : InspectionQuantity(row, name);
        }
    }
}
