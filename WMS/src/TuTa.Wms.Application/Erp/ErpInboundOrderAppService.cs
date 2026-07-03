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
    /// ERP入库单应用服务
    /// </summary>
    public class ErpInboundOrderAppService : ApplicationService, IErpInboundOrderAppService
    {
        private readonly IErpInboundOrderRepository _erpInboundOrderRepository;
        private readonly ErpInboundOrderManager _erpInboundOrderManager;
        private readonly ILogger<ErpInboundOrderAppService> _logger;

        public ErpInboundOrderAppService(
            IErpInboundOrderRepository erpInboundOrderRepository,
            ErpInboundOrderManager erpInboundOrderManager,
            ILogger<ErpInboundOrderAppService> logger)
        {
            _erpInboundOrderRepository = erpInboundOrderRepository;
            _erpInboundOrderManager = erpInboundOrderManager;
            _logger = logger;
        }

        /// <summary>
        /// 接收ERP入库单数据
        /// </summary>
        [UnitOfWork]
        public async Task<ErpInboundOrderResponseDto> ReceiveInboundOrderAsync(ErpInboundOrderRequestDto request)
        {
            try
            {
                var inboundOrderzq = await _erpInboundOrderRepository.FindByInboundOrderNoAsync(request.FStkInLogNo);
                if (inboundOrderzq!=null)
                    return new ErpInboundOrderResponseDto { Succeed = false, Message = $"{request.FStkInLogNo}入库单号的请求数据已经存在" };

                // 验证请求数据
                if (request == null)
                    return new ErpInboundOrderResponseDto { Succeed = false, Message = "请求数据不能为空" };

                if (request.FStkInMxs == null || request.FStkInMxs.Count == 0)
                    return new ErpInboundOrderResponseDto { Succeed = false, Message = "入库单明细不能为空" };

                // 解析计划入库日期
                if (!DateTime.TryParse(request.FPlanInDate, out DateTime planInboundDate))
                    return new ErpInboundOrderResponseDto { Succeed = false, Message = "计划入库日期格式不正确" };

                // 验证数据
                _erpInboundOrderManager.ValidateInboundOrderData(
                    request.FStkInLogNo,
                    request.FStkCode,
                    planInboundDate);

                // 创建入库单
                var inboundOrder = await _erpInboundOrderManager.CreateInboundOrderAsync(
                    request.FStkInLogNo,
                    request.FStkCode,
                    planInboundDate);

                // 添加入库单项
                foreach (var item in request.FStkInMxs)
                {
                    if (!decimal.TryParse(item.fPlanInQty, out decimal planQty))
                        return new ErpInboundOrderResponseDto { Succeed = false, Message = $"物料{item.fGoodsCode}的计划入库数量格式不正确" };

                    if (!decimal.TryParse(item.fActInQty, out decimal actualQty))
                        return new ErpInboundOrderResponseDto { Succeed = false, Message = $"物料{item.fGoodsCode}的实际入库数量格式不正确" };

                    inboundOrder.AddInboundItem(
                        item.fGoodsCode,
                        item.fGoodsName,
                        planQty,
                        actualQty,
                        item.fUnitCode,
                        item.fMoNo,
                        item.fLvlCode,
                        item.fLotNo);
                }

                // 保存到数据库
                await _erpInboundOrderRepository.InsertAsync(inboundOrder);

                _logger.LogInformation($"成功接收ERP入库单：{request.FStkInLogNo}，包含{request.FStkInMxs.Count}个物料项");

                return new ErpInboundOrderResponseDto
                {
                    Succeed = true,
                    Message = "入库单接收成功"
                    
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"接收ERP入库单失败：{request?.FStkInLogNo}");
                return new ErpInboundOrderResponseDto
                {
                    Succeed = false,
                    Message = $"入库单接收失败：{ex.Message}"
                };
            }
        }

        /// <summary>
        /// 根据ID获取入库单
        /// </summary>
        public async Task<ErpInboundOrderDto> GetAsync(Guid id)
        {
            var inboundOrder = await _erpInboundOrderRepository.GetWithItemsAsync(id);
            return ObjectMapper.Map<ErpInboundOrder, ErpInboundOrderDto>(inboundOrder);
        }

        /// <summary>
        /// 根据入库单号获取入库单
        /// </summary>
        public async Task<ErpInboundOrderDto> GetByInboundOrderNoAsync(string inboundOrderNo)
        {
            var inboundOrder = await _erpInboundOrderRepository.FindByInboundOrderNoAsync(inboundOrderNo);
            if (inboundOrder == null)
                return null;

            return ObjectMapper.Map<ErpInboundOrder, ErpInboundOrderDto>(inboundOrder);
        }

        /// <summary>
        /// 获取入库单列表
        /// </summary>
        public async Task<List<ErpInboundOrderDto>> GetListAsync(
            string warehouseCode = null,
            int? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            List<ErpInboundOrder> inboundOrders;

            if (!string.IsNullOrEmpty(warehouseCode))
            {
                inboundOrders = await _erpInboundOrderRepository.GetByWarehouseCodeAsync(warehouseCode);
            }
            else if (status.HasValue)
            {
                inboundOrders = await _erpInboundOrderRepository.GetByStatusAsync((InboundOrderStatus)status.Value);
            }
            else if (startDate.HasValue && endDate.HasValue)
            {
                inboundOrders = await _erpInboundOrderRepository.GetByPlanInboundDateRangeAsync(startDate.Value, endDate.Value);
            }
            else
            {
                inboundOrders = await _erpInboundOrderRepository.GetListWithItemsAsync();
            }

            return ObjectMapper.Map<List<ErpInboundOrder>, List<ErpInboundOrderDto>>(inboundOrders);
        }

        /// <summary>
        /// 更新入库单状态
        /// </summary>
        [UnitOfWork]
        public async Task<bool> UpdateStatusAsync(Guid id, int status)
        {
            try
            {
                var inboundOrder = await _erpInboundOrderRepository.GetWithItemsAsync(id);
                inboundOrder.SetStatus((InboundOrderStatus)status);
                await _erpInboundOrderRepository.UpdateAsync(inboundOrder);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新入库单状态失败：{id}");
                return false;
            }
        }

        /// <summary>
        /// 删除入库单
        /// </summary>
        [UnitOfWork]
        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                await _erpInboundOrderRepository.DeleteAsync(id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除入库单失败：{id}");
                return false;
            }
        }
    }
}
