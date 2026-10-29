using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TuTa.Wms.Erp.Entities;
using TuTa.Wms.Erp.IDto;
using Volo.Abp.Uow;

namespace TuTa.Wms.Erp
{
    public partial class ErpAsnAppService
    {
        /// <summary>调用U8的POST List/GetLLJYDList，筛选留空时按默认条件单页查询，避免连续翻页耗尽请求时限。</summary>
        /// <param name="input">分页及可选存货编码、到货单、制单开始日期；包含开始日期当天及之后的单据。</param>
        /// <returns>U8检验记录及可严格关联的本地ASN来源；不据此修改本地检验状态。</returns>
        [UnitOfWork(IsDisabled = true)]
        public async Task<U8MaterialInspectionResultDto> GetMaterialInspectionResultsAsync(U8MaterialInspectionQueryDto input)
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
                var token = await GetErpTokenAsync(Guid.NewGuid().ToString());
                if (string.IsNullOrWhiteSpace(token)) throw new InvalidOperationException("U8登录失败");
                using var client = CreateU8Client();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(30));
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
                // 查询存在性不要求已保存ASN；只有明确匹配本地来源的记录才能交给组盘区。
                var localLines = new List<ErpAsn>();
                var materialCodes = items.Select(item => item.MaterialCode).Where(code => !string.IsNullOrWhiteSpace(code)).Distinct().ToList();
                if (materialCodes.Count > 0)
                {
                    using var read = UnitOfWorkManager.Begin(true, false);
                    localLines = await _erpAsnRepository.GetInspectionSourcesAsync(materialCodes);
                    await read.CompleteAsync();
                }
                foreach (var item in items)
                {
                    var matches = localLines.Where(line => !string.IsNullOrWhiteSpace(item.SourceCode) && item.SourceAutoId.HasValue &&
                        line.MaterialCode == item.MaterialCode &&
                        line.ArrivalVoucherCode == item.SourceCode && line.ArrivalDetailId == item.SourceAutoId &&
                        (line.BatchCode ?? "") == (item.BatchCode ?? "")).ToList();
                    if (matches.Select(line => line.AsnCode).Distinct().Count() != 1) continue;
                    item.AsnCode = matches[0].AsnCode;
                    item.AsnLines = matches.Select(MapToErpAsnDto).ToList();
                }
                result.Success = true;
                result.HasInspection = items.Count > 0;
                result.Items = items;
                result.Message = result.HasInspection ? $"第{result.Page}页查询到{items.Count}条对应的U8来料检验记录" :
                    result.HasMore || result.Page > 1 ? $"第{result.Page}页未查询到对应记录" : "未查询到符合条件的U8来料检验单";
                if (result.HasMore) result.Message += "；还有后续记录，可点击加载更多或补充筛选条件";
            }
            catch (OperationCanceledException) { result.Message = "U8来料检验单单页查询超时，请稍后重试，或补充物料码、制单开始日期及减小每页条数后查询"; }
            catch (Exception ex) { result.Message = "U8来料检验单查询失败：" + ex.Message; }
            return result;
        }

        /// <summary>读取可选数量，缺少字段时保留未知值，非法值不得伪装成0。</summary>
        private static decimal? ReadOptionalInspectionQuantity(JToken row, string name)
        {
            var value = InspectionField(row, name);
            return value == null || value.Type == JTokenType.Null ? (decimal?)null : InspectionQuantity(row, name);
        }
    }
}
