using System;
using System.Linq;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Dto;
using TuTa.Wms.Erp.Entities;
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
