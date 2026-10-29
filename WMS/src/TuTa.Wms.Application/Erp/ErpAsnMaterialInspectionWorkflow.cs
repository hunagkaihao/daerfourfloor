using System;
using System.Linq;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Dto;
using TuTa.Wms.Erp.Entities;
using TuTa.Wms.Erp.IDto;
using TuTa.Wms.Stocks;
using Volo.Abp.Uow;

namespace TuTa.Wms.Erp
{
    public partial class ErpAsnAppService
    {
        /// <summary>检验状态名称，独立于ASN收货状态和U8接口推送状态。</summary>
        /// <param name="status">物料检验状态。</param>
        /// <returns>供PDA及管理页显示的中文名称。</returns>
        private static string GetMaterialInspectionStatusName(InspectionStatus status) => status switch
        {
            InspectionStatus.AwaitingInspection => "待检",
            InspectionStatus.InProgressInspection => "检验中",
            InspectionStatus.InspectionQualified => "合格",
            InspectionStatus.InspectionNotQualified => "不合格",
            _ => "未知"
        };

        /// <summary>重新核对已审核的U8检验单，同步已报检状态并确认其来源明细对应的ASN及库存检验结论。</summary>
        /// <param name="input">所选检验记录、查询页码与每页条数，以及2合格或3不合格的人工结论。</param>
        /// <returns>事务保存后的对应ASN明细；查询、审核或来源校验失败时不修改状态。</returns>
        [UnitOfWork(IsDisabled = true)]
        public async Task<ErpAsnValidateResponseDto> ConfirmMaterialInspectionAsync(U8MaterialInspectionConfirmDto input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.MaterialCode) ||
                string.IsNullOrWhiteSpace(input.AsnCode) || string.IsNullOrWhiteSpace(input.CheckCode) ||
                input.CheckId <= 0 || input.SourceAutoId <= 0)
                return new ErpAsnValidateResponseDto { Success = false, Message = "请选择具有完整来源的来料检验单" };
            if (input.InspectionStatus != (int)InspectionStatus.InspectionQualified &&
                input.InspectionStatus != (int)InspectionStatus.InspectionNotQualified)
                return new ErpAsnValidateResponseDto { Success = false, Message = "检验结果只能选择合格或不合格" };
            var asnCode = input.AsnCode.Trim();
            await LocalPushLock.WaitAsync();
            try
            {
                await using var handle = _u8Lock == null ? null : await _u8Lock.TryAcquireAsync("U8:ASN:" + asnCode);
                if (_u8Lock != null && handle == null)
                    return new ErpAsnValidateResponseDto { Success = false, Message = "该ASN正在处理，请稍后重试" };
                // 不信任前端传来的审核状态或明细列表，沿原分页条件重新取得真实U8记录。
                var result = await GetMaterialInspectionResultsAsync(new U8MaterialInspectionQueryDto {
                    MaterialCode = input.MaterialCode, BeginMakeTime = input.BeginMakeTime,
                    Page = input.Page, PageSize = input.PageSize
                });
                if (!result.Success)
                    return new ErpAsnValidateResponseDto { Success = false, Message = result.Message };
                var matches = result.Items.Where(item => item.CheckId == input.CheckId &&
                    item.CheckCode == input.CheckCode && item.SourceAutoId == input.SourceAutoId &&
                    item.AsnCode == asnCode).ToList();
                if (matches.Count != 1)
                    return new ErpAsnValidateResponseDto { Success = false, Message = "所选检验记录或ASN来源已变化，请重新查询并选择" };
                var selected = matches[0];
                if (!selected.IsVerified)
                    return new ErpAsnValidateResponseDto { Success = false, Message = "所选来料检验单尚未审核，不能确认检验结果" };
                using var uow = UnitOfWorkManager.Begin(true, true);
                var lines = (await _erpAsnRepository.GetListByAsnCodeAsync(asnCode)).Where(line =>
                    line.Status != AsnStatus.Cancelled && line.MaterialCode == selected.MaterialCode &&
                    line.ArrivalVoucherCode == selected.SourceCode && line.ArrivalDetailId == selected.SourceAutoId &&
                    (line.BatchCode ?? "") == (selected.BatchCode ?? "")).ToList();
                if (lines.Count == 0 || !lines.Select(line => line.Id.ToString()).ToHashSet()
                    .SetEquals(selected.AsnLines.Select(line => line.Autoid)))
                    return new ErpAsnValidateResponseDto { Success = false, Message = "所选检验单的本地ASN来源已变化，请重新查询" };
                var target = (InspectionStatus)input.InspectionStatus;
                if (lines.Any(line =>
                    (line.MaterialInspectionStatus != InspectionStatus.AwaitingInspection &&
                     line.MaterialInspectionStatus != InspectionStatus.InProgressInspection && line.MaterialInspectionStatus != target) ||
                    (line.IsPurchasePushed && line.MaterialInspectionStatus != target)))
                    return new ErpAsnValidateResponseDto { Success = false,
                        Message = "已确认的不同检验结果不能覆盖，已采购入库的物料不能重新判定" };
                // 只同步当前检验记录关联的库存，不扩大到同一采购订单的其他物料。
                if (_u8StockRepository != null)
                {
                    var lineIds = lines.Select(line => line.Id).ToList();
                    var stocks = await _u8StockRepository.GetListAsync(stock =>
                        stock.U8AsnLineId.HasValue && lineIds.Contains(stock.U8AsnLineId.Value));
                    if (stocks.Any(stock =>
                        (target == InspectionStatus.InspectionQualified && stock.InspectionStatus == InspectionStatus.InspectionNotQualified) ||
                        (target == InspectionStatus.InspectionNotQualified && stock.InspectionStatus == InspectionStatus.InspectionQualified)))
                        return new ErpAsnValidateResponseDto { Success = false, Message = "对应库存已有不同检验结果，请先核对，不能覆盖" };
                    foreach (var stock in stocks)
                    {
                        // 样品尚未归还时沿用既有库存回补流程，不改动数量。
                        if (target == InspectionStatus.InspectionQualified && (stock.InspectionCount ?? 0) > 0) continue;
                        stock.InspectionStatus = target;
                        await _u8StockRepository.UpdateAsync(stock);
                    }
                }
                foreach (var line in lines)
                {
                    // 兼容直接在U8报检的单据：严格核对的已审核记录可补齐本地报检状态。
                    line.SynchronizeVerifiedInspectionReport(selected.CheckId.Value, selected.CheckCode);
                    line.CompleteMaterialInspection(target);
                    await _erpAsnRepository.UpdateAsync(line);
                }
                await uow.CompleteAsync();
                return new ErpAsnValidateResponseDto { Success = true,
                    Message = $"检验单{selected.CheckCode}对应物料已检验（{GetMaterialInspectionStatusName(target)}）",
                    Data = lines.Select(MapToErpAsnDto).ToList() };
            }
            finally { LocalPushLock.Release(); }
        }

        /// <summary>锁定当前ASN，按采购订单确认全部明细的检验结果，不依赖是否已经组盘生成库存。</summary>
        /// <param name="asnCode">送货单对应ASN号，不能为空。</param>
        /// <param name="orderCode">该ASN下的采购订单号，不能为空。</param>
        /// <param name="inspectionStatus">仅接受2合格、3不合格。</param>
        /// <returns>确认结果和该订单的最新明细，失败时不更新其他订单。</returns>
        [UnitOfWork(IsDisabled = true)]
        public async Task<ErpAsnValidateResponseDto> SetAsnOrderInspectionStatusAsync(
            string asnCode, string orderCode, int inspectionStatus)
        {
            if (string.IsNullOrWhiteSpace(asnCode) || string.IsNullOrWhiteSpace(orderCode))
                return new ErpAsnValidateResponseDto { Success = false, Message = "ASN号和采购订单号不能为空" };
            if (inspectionStatus != (int)InspectionStatus.InspectionQualified &&
                inspectionStatus != (int)InspectionStatus.InspectionNotQualified)
                return new ErpAsnValidateResponseDto { Success = false, Message = "检验结果只能选择合格或不合格" };
            asnCode = asnCode.Trim();
            orderCode = orderCode.Trim();
            await LocalPushLock.WaitAsync();
            try
            {
                await using var handle = _u8Lock == null ? null : await _u8Lock.TryAcquireAsync("U8:ASN:" + asnCode);
                if (_u8Lock != null && handle == null)
                    return new ErpAsnValidateResponseDto { Success = false, Message = "该ASN正在处理，请稍后重试" };
                using (var uow = UnitOfWorkManager.Begin(true, true))
                {
                    var lines = (await _erpAsnRepository.GetListByAsnCodeAsync(asnCode))
                        .Where(x => x.Status != AsnStatus.Cancelled && x.OrderCode == orderCode).ToList();
                    if (lines.Count == 0)
                        return new ErpAsnValidateResponseDto { Success = false, Message = "当前ASN没有该采购订单的有效明细" };
                    var target = (InspectionStatus)inspectionStatus;
                    if (lines.Any(x => !x.IsInspectionReportPushed ||
                        (x.MaterialInspectionStatus != InspectionStatus.InProgressInspection && x.MaterialInspectionStatus != target)))
                        return new ErpAsnValidateResponseDto { Success = false,
                            Message = "请先成功推送该订单的来料报检单；仅检验中的物料允许确认结果，已确认的不同结果不能覆盖" };
                    // 同一订单的状态在一个事务内确认；库存仍保留待采购入库状态及原始数量。
                    foreach (var line in lines)
                    {
                        line.CompleteMaterialInspection(target);
                        await _erpAsnRepository.UpdateAsync(line, autoSave: true);
                    }
                    if (_u8StockRepository != null)
                    {
                        var lineIds = lines.Select(x => x.Id).ToList();
                        var stocks = await _u8StockRepository.GetListAsync(x =>
                            x.U8AsnLineId.HasValue && lineIds.Contains(x.U8AsnLineId.Value));
                        if (stocks.Any(x =>
                            (target == InspectionStatus.InspectionQualified && x.InspectionStatus == InspectionStatus.InspectionNotQualified) ||
                            (target == InspectionStatus.InspectionNotQualified && x.InspectionStatus == InspectionStatus.InspectionQualified)))
                            return new ErpAsnValidateResponseDto { Success = false,
                                Message = "对应库存存在已确认的不同检验结果，请先核对，不能覆盖" };
                        foreach (var stock in stocks)
                        {
                            // 抽检样品尚未归还时保留库存检验中，由原样品确认入口处理数量回补。
                            if (target == InspectionStatus.InspectionQualified && (stock.InspectionCount ?? 0) > 0) continue;
                            stock.InspectionStatus = target;
                            await _u8StockRepository.UpdateAsync(stock);
                        }
                    }
                    await uow.CompleteAsync();
                    return new ErpAsnValidateResponseDto { Success = true,
                        Message = $"订单{orderCode}已确认为{GetMaterialInspectionStatusName(target)}",
                        Data = lines.Select(MapToErpAsnDto).ToList() };
                }
            }
            finally { LocalPushLock.Release(); }
        }
    }
}
