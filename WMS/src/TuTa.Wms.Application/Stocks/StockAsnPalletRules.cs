using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TuTa.Wms.Boxes.Aggregates;
using TuTa.Wms.Erp.Entities;
using TuTa.Wms.Stocks.Dtos;
using Volo.Abp;

namespace TuTa.Wms.Stocks
{
    public partial class StockService
    {
        /// <summary>
        /// ASN组盘提交前校验整托：最多两种规格、等级和纸箱包装一致。
        /// 同时统计已有正库存和本次全部箱料，拒绝后不写入库存或ASN累计数量。
        /// </summary>
        /// <param name="box">当前托盘；尚未创建时为空，按空托盘校验。</param>
        /// <param name="lines">已核实的箱料与ASN来源；规格、纸箱采用ASN Specs、Free3。</param>
        private async Task ValidateAsnPalletRulesAsync(Box box, IReadOnlyDictionary<StockCreateDto, ErpAsn> lines)
        {
            var specifications = new HashSet<string>(StringComparer.Ordinal);
            var grades = new HashSet<string>(StringComparer.Ordinal);
            var cartons = new HashSet<string>(StringComparer.Ordinal);
            foreach (var line in lines.Values)
                AddPalletAttributes(line.Specs, line.MaterialCode, line.Free5, line.Free3, specifications, grades, cartons);

            if (box != null)
            {
                var sources = lines.Values.GroupBy(x => x.Id).ToDictionary(x => x.Key, x => x.First());
                var stocks = await _stockRepository.GetByBoxIdAsync(box.Id);
                foreach (var stock in stocks.Where(x => x.TotalCountInTime > 0 || (x.InspectionCount ?? 0) > 0))
                {
                    if (!stock.U8AsnLineId.HasValue)
                        throw new UserFriendlyException("托盘已有库存缺少ASN来源，无法核实纸箱包装，请核对后分托组盘");
                    if (!sources.TryGetValue(stock.U8AsnLineId.Value, out var source))
                    {
                        source = await _erpAsnRepository.GetAsync(stock.U8AsnLineId.Value);
                        if (source == null) throw new UserFriendlyException("托盘已有库存的ASN来源不存在，请核对后组盘");
                        sources.Add(source.Id, source);
                    }
                    AddPalletAttributes(source.Specs, source.MaterialCode, stock.Grade, source.Free3,
                        specifications, grades, cartons);
                }
            }
            if (specifications.Count > 2)
                throw new UserFriendlyException("同一托盘至多放置2种不同规格的物料，请分托组盘");
            if (grades.Count > 1)
                throw new UserFriendlyException("不同等级的物料不允许放置在同一个托盘上，请分托组盘");
            if (cartons.Count > 1)
                throw new UserFriendlyException("不同纸箱包装不允许放置在同一个托盘上，请分托组盘");
        }

        /// <summary>规格为空时按物料编码保守计数；空等级或包装保留为独立值，不作为通配条件。</summary>
        private static void AddPalletAttributes(string specs, string materialCode, string grade, string carton,
            HashSet<string> specifications, HashSet<string> grades, HashSet<string> cartons)
        {
            specifications.Add(string.IsNullOrWhiteSpace(specs) ? "code:" + materialCode?.Trim() : "spec:" + specs.Trim());
            grades.Add(grade?.Trim() ?? "");
            cartons.Add(carton?.Trim() ?? "");
        }
    }
}
