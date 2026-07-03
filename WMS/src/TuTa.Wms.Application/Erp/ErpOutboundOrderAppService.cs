using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TuTa.Wms.Erp;
using TuTa.Wms.Erp.Aggregates;
using TuTa.Wms.Erp.Dto;
using TuTa.Wms.Erp.Repositories;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace TuTa.Wms.Erp
{
    /// <summary>
    /// ERP出库单应用服务
    /// </summary>
    public class ErpOutboundOrderAppService : ApplicationService, IErpOutboundOrderAppService
    {
        private readonly IErpOutboundOrderRepository _erpOutboundOrderRepository;
        private readonly ErpOutboundOrderManager _erpOutboundOrderManager;
        private readonly ILogger<ErpOutboundOrderAppService> _logger;

        public ErpOutboundOrderAppService(
            IErpOutboundOrderRepository erpOutboundOrderRepository,
            ErpOutboundOrderManager erpOutboundOrderManager,
            ILogger<ErpOutboundOrderAppService> logger)
        {
            _erpOutboundOrderRepository = erpOutboundOrderRepository;
            _erpOutboundOrderManager = erpOutboundOrderManager;
            _logger = logger;
        }

        /// <summary>
        /// 接收ERP出库单数据
        /// </summary>
        [UnitOfWork]
        public async Task<ErpOutboundOrderResponseDto> ReceiveOutboundOrderAsync(ErpOutboundOrderRequestDto request)
        {
            try
            {
                var outboundOrderzq = await _erpOutboundOrderRepository.FindByOutboundOrderNoAsync(request.fStkOutLogNo);
                if (outboundOrderzq != null)
                    return new ErpOutboundOrderResponseDto { Succeed = false, Message = $"{request.fStkOutLogNo}出库单号的请求数据已经存在" };

                // 验证请求数据
                if (request == null)
                    return new ErpOutboundOrderResponseDto { Succeed = false, Message = "请求数据不能为空" };

                if (request.FStkOutMxs == null || request.FStkOutMxs.Count == 0)
                    return new ErpOutboundOrderResponseDto { Succeed = false, Message = "出库单明细不能为空" };

                // 解析计划出库日期
                if (!DateTime.TryParse(request.fPlanOutDate, out DateTime planOutboundDate))
                    return new ErpOutboundOrderResponseDto { Succeed = false, Message = "计划出库日期格式不正确" };

                // 验证数据
                _erpOutboundOrderManager.ValidateOutboundOrderData(
                    request.fStkOutLogNo,
                    request.FStkCode,
                    planOutboundDate);

                // 创建出库单
                var outboundOrder = await _erpOutboundOrderManager.CreateOutboundOrderAsync(
                    request.fStkOutLogNo,
                    request.FStkCode,
                    planOutboundDate);

                // 添加出库单项
                foreach (var item in request.FStkOutMxs)
                {
                    if (!decimal.TryParse(item.fPlanOutQty, out decimal planQty))
                        return new ErpOutboundOrderResponseDto { Succeed = false, Message = $"物料{item.fGoodsCode}的计划出库数量格式不正确" };

                    if (!decimal.TryParse(item.fActOutQty, out decimal actualQty))
                        return new ErpOutboundOrderResponseDto { Succeed = false, Message = $"物料{item.fGoodsCode}的实际出库数量格式不正确" };

                    outboundOrder.AddOutboundItem(
                        item.fGoodsCode,
                        item.fGoodsName,
                        planQty,
                        actualQty,
                        item.fUnitCode,
                        item.fMoNo,
                        item.fLvlCode,
                        item.fLotNo,
                        item.fPlaceCode,
                        item.deliveryCode);
                }

                // 保存到数据库
                await _erpOutboundOrderRepository.InsertAsync(outboundOrder);

                _logger.LogInformation($"成功接收ERP出库单：{request.fStkOutLogNo}，包含{request.FStkOutMxs.Count}个物料项");

                return new ErpOutboundOrderResponseDto
                {
                    Succeed = true,
                    Message = "出库单接收成功"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"接收ERP出库单失败：{request?.fStkOutLogNo}");
                return new ErpOutboundOrderResponseDto
                {
                    Succeed = false,
                    Message = $"出库单接收失败：{ex.Message}"
                };
            }
        }

        /// <summary>
        /// 根据ID获取出库单
        /// </summary>
        public async Task<ErpOutboundOrderDto> GetAsync(Guid id)
        {
            var outboundOrder = await _erpOutboundOrderRepository.GetWithItemsAsync(id);
            return ObjectMapper.Map<ErpOutboundOrder, ErpOutboundOrderDto>(outboundOrder);
        }

        /// <summary>
        /// 根据出库单号获取出库单
        /// </summary>
        public async Task<ErpOutboundOrderDto> GetByOutboundOrderNoAsync(string outboundOrderNo)
        {
            var outboundOrder = await _erpOutboundOrderRepository.FindByOutboundOrderNoAsync(outboundOrderNo);
            if (outboundOrder == null)
                return null;

            return ObjectMapper.Map<ErpOutboundOrder, ErpOutboundOrderDto>(outboundOrder);
        }

        /// <summary>
        /// 获取出库单列表
        /// </summary>
        public async Task<List<ErpOutboundOrderDto>> GetListAsync(
            string warehouseCode = null,
            int? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            List<ErpOutboundOrder> outboundOrders;

            if (!string.IsNullOrEmpty(warehouseCode))
            {
                outboundOrders = await _erpOutboundOrderRepository.GetByWarehouseCodeAsync(warehouseCode);
            }
            else if (status.HasValue)
            {
                outboundOrders = await _erpOutboundOrderRepository.GetByStatusAsync((OutboundOrderStatus)status.Value);
            }
            else if (startDate.HasValue && endDate.HasValue)
            {
                outboundOrders = await _erpOutboundOrderRepository.GetByPlanOutboundDateRangeAsync(startDate.Value, endDate.Value);
            }
            else
            {
                outboundOrders = await _erpOutboundOrderRepository.GetListWithItemsAsync();
            }

            return ObjectMapper.Map<List<ErpOutboundOrder>, List<ErpOutboundOrderDto>>(outboundOrders);
        }

        /// <summary>
        /// 更新出库单状态
        /// </summary>
        [UnitOfWork]
        public async Task<bool> UpdateStatusAsync(Guid id, int status)
        {
            try
            {
                var outboundOrder = await _erpOutboundOrderRepository.GetWithItemsAsync(id);
                outboundOrder.SetStatus((OutboundOrderStatus)status);
                await _erpOutboundOrderRepository.UpdateAsync(outboundOrder);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新出库单状态失败：{id}");
                return false;
            }
        }

        /// <summary>
        /// 删除出库单
        /// </summary>
        [UnitOfWork]
        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                await _erpOutboundOrderRepository.DeleteAsync(id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除出库单失败：{id}");
                return false;
            }
        }
    }
}
