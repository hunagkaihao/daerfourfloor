using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TuTa.Wms.Application.Contracts.Erp;
using TuTa.Wms.Application.Contracts.Erp.IDto;
using TuTa.Wms.Erp;
using TuTa.Wms.Erp.Entities;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.Application.Erp
{
    public class ErpDeliveryOrderAppService : ApplicationService, IErpDeliveryOrderAppService
    {
        private readonly IErpoutboundRepository _outboundRepository;
        private readonly IErpDeliveryOrderItemRepository _outboundItemRepository;
        private readonly ILogger<ErpDeliveryOrderAppService> _logger;

        public ErpDeliveryOrderAppService(
            IErpoutboundRepository outboundRepository,
            IErpDeliveryOrderItemRepository deliveryOrderItemRepository,
            ILogger<ErpDeliveryOrderAppService> logger)
        {
            _outboundRepository = outboundRepository;
            _outboundItemRepository = deliveryOrderItemRepository;
            _logger = logger;
        }

        public async Task<ErpDeliveryOrderListResponseDto> GetDeliveryOrderListAsync(
            int page,
            int pageSize,
            string deliveryOrderNo = null,
            string warehouseCode = null,
            string startDate = null,
            string endDate = null)
        {
            try
            {
                _logger.LogInformation($"开始获取发货单列表，页码：{page}，每页数量：{pageSize}");

                DateTime? startDateTime = null;
                DateTime? endDateTime = null;

                if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out var start))
                {
                    startDateTime = start;
                }

                if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out var end))
                {
                    endDateTime = end;
                }

                var (orders, total) = await _outboundRepository.GetDeliveryOrdersAsync(
                    page, pageSize, deliveryOrderNo, warehouseCode, startDateTime, endDateTime);

                var items = new List<ErpDeliveryOrderDto>();

                foreach (var order in orders)
                {
                    var orderItems = await _outboundItemRepository.GetByOrderIdAsync(order.Id);

                    items.Add(new ErpDeliveryOrderDto
                    {
                        Id = order.Id,
                        DeliveryOrderNo = order.DeliveryOrderNo,
                        WarehouseCode = order.WarehouseCode,
                        WarehouseName = order.WarehouseName,
                        DeliveryDate = order.DeliveryDate,
                        Status = order.Status,
                        CompletedTime = order.CompletedTime,
                        Remarks = order.Remarks,
                        CreationTime = order.CreationTime,
                        Items = orderItems.Select(item => new ErpDeliveryOrderItemDto
                        {
                            Id = item.Id,
                            DeliveryOrderId = item.DeliveryOrderId,
                            MaterialCode = item.MaterialCode,
                            MaterialName = item.MaterialName,
                            Specs = item.Specs,
                            Unit = item.Unit,
                            DeliveryQuantity = item.DeliveryQuantity,
                            BatchCode = item.BatchCode,
                            BoxNo = item.BoxNo,
                            Packaging = item.Packaging,
                            Grade = item.Grade,
                            LabelPrint = item.LabelPrint,
                            QuantityPerBox = item.QuantityPerBox,
                            ShippedQuantity = item.ShippedQuantity
                        }).ToList()
                    });
                }

                var response = new ErpDeliveryOrderListResponseDto
                {
                    Items = items,
                    Total = total,
                    Page = page,
                    PageSize = pageSize
                };

                _logger.LogInformation($"获取发货单列表成功，总数：{total}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取发货单列表异常");
                return new ErpDeliveryOrderListResponseDto
                {
                    Items = new List<ErpDeliveryOrderDto>(),
                    Total = 0,
                    Page = page,
                    PageSize = pageSize
                };
            }
        }

        public async Task<ErpDeliveryOrderDto> GetDeliveryOrderByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation($"获取发货单详情，ID：{id}");

                var order = await _outboundRepository.GetAsync(id);

                if (order == null)
                {
                    _logger.LogWarning($"发货单不存在，ID：{id}");
                    return null;
                }

                var items = await _outboundItemRepository.GetByOrderIdAsync(order.Id);

                return new ErpDeliveryOrderDto
                {
                    Id = order.Id,
                    DeliveryOrderNo = order.DeliveryOrderNo,
                    WarehouseCode = order.WarehouseCode,
                    WarehouseName = order.WarehouseName,
                    DeliveryDate = order.DeliveryDate,
                    Status = order.Status,
                    CompletedTime = order.CompletedTime,
                    Remarks = order.Remarks,
                    CreationTime = order.CreationTime,
                    Items = items.Select(item => new ErpDeliveryOrderItemDto
                    {
                        Id = item.Id,
                        DeliveryOrderId = item.DeliveryOrderId,
                        MaterialCode = item.MaterialCode,
                        MaterialName = item.MaterialName,
                        Specs = item.Specs,
                        Unit = item.Unit,
                        DeliveryQuantity = item.DeliveryQuantity,
                        BatchCode = item.BatchCode,
                        BoxNo = item.BoxNo,
                        Packaging = item.Packaging,
                        Grade = item.Grade,
                        LabelPrint = item.LabelPrint,
                        QuantityPerBox = item.QuantityPerBox,
                        ShippedQuantity = item.ShippedQuantity
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取发货单详情异常，ID：{id}");
                return null;
            }
        }

        public async Task<ErpDeliveryOrderDto> CreateDeliveryOrderAsync(ErpDeliveryOrderCreateDto input)
        {
            try
            {
                _logger.LogInformation($"创建发货单，单号：{input.DeliveryOrderNo}");

                if (await _outboundRepository.ExistsByOrderNoAsync(input.DeliveryOrderNo))
                {
                    _logger.LogWarning($"发货单已存在，单号：{input.DeliveryOrderNo}");
                    throw new Exception($"发货单已存在：{input.DeliveryOrderNo}");
                }

                var orderId = GuidGenerator.Create();

                var order = ErpDeliveryOrder.Create(
                    orderId,
                    input.DeliveryOrderNo,
                    input.WarehouseCode,
                    input.WarehouseName,
                    input.DeliveryDate);

                if (!string.IsNullOrEmpty(input.Remarks))
                {
                    order.UpdateRemarks(input.Remarks);
                }

                await _outboundRepository.InsertAsync(order);

                foreach (var itemInput in input.Items)
                {
                    var item = ErpDeliveryOrderItem.Create(
                        GuidGenerator.Create(),
                        orderId,
                        itemInput.MaterialCode,
                        itemInput.MaterialName,
                        itemInput.Specs,
                        itemInput.Unit,
                        itemInput.DeliveryQuantity,
                        itemInput.BatchCode,
                        itemInput.BoxNo,
                        itemInput.Packaging,
                        itemInput.Grade,
                        itemInput.LabelPrint,
                        itemInput.QuantityPerBox);

                    await _outboundItemRepository.InsertAsync(item);
                }

                _logger.LogInformation($"创建发货单成功，ID：{orderId}");

                return await GetDeliveryOrderByIdAsync(orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"创建发货单异常，单号：{input.DeliveryOrderNo}");
                throw;
            }
        }

        public async Task<ErpDeliveryOrderDto> UpdateDeliveryOrderAsync(Guid id, ErpDeliveryOrderCreateDto input)
        {
            try
            {
                _logger.LogInformation($"更新发货单，ID：{id}");

                var order = await _outboundRepository.GetAsync(id);

                if (order == null)
                {
                    _logger.LogWarning($"发货单不存在，ID：{id}");
                    throw new Exception("发货单不存在");
                }

                if (order.Status == DeliveryOrderStatus.Completed)
                {
                    _logger.LogWarning($"发货单已完成，无法更新，ID：{id}");
                    throw new Exception("发货单已完成，无法更新");
                }

                order.UpdateRemarks(input.Remarks);

                await _outboundRepository.UpdateAsync(order);

                var existingItems = await _outboundItemRepository.GetByOrderIdAsync(id);

                foreach (var existingItem in existingItems)
                {
                    await _outboundItemRepository.DeleteAsync(existingItem);
                }

                foreach (var itemInput in input.Items)
                {
                    var item = ErpDeliveryOrderItem.Create(
                        GuidGenerator.Create(),
                        id,
                        itemInput.MaterialCode,
                        itemInput.MaterialName,
                        itemInput.Specs,
                        itemInput.Unit,
                        itemInput.DeliveryQuantity,
                        itemInput.BatchCode,
                        itemInput.BoxNo,
                        itemInput.Packaging,
                        itemInput.Grade,
                        itemInput.LabelPrint,
                        itemInput.QuantityPerBox);

                    await _outboundItemRepository.InsertAsync(item);
                }

                _logger.LogInformation($"更新发货单成功，ID：{id}");

                return await GetDeliveryOrderByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新发货单异常，ID：{id}");
                throw;
            }
        }

        public async Task DeleteDeliveryOrderAsync(Guid id)
        {
            try
            {
                _logger.LogInformation($"删除发货单，ID：{id}");

                var order = await _outboundRepository.GetAsync(id);

                if (order == null)
                {
                    _logger.LogWarning($"发货单不存在，ID：{id}");
                    throw new Exception("发货单不存在");
                }

                if (order.Status == DeliveryOrderStatus.Completed)
                {
                    _logger.LogWarning($"发货单已完成，无法删除，ID：{id}");
                    throw new Exception("发货单已完成，无法删除");
                }

                var items = await _outboundItemRepository.GetByOrderIdAsync(id);

                foreach (var item in items)
                {
                    await _outboundItemRepository.DeleteAsync(item);
                }

                await _outboundRepository.DeleteAsync(order);

                _logger.LogInformation($"删除发货单成功，ID：{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除发货单异常，ID：{id}");
                throw;
            }
        }

        public async Task<ErpDeliveryOrderDto> CompleteDeliveryOrderAsync(Guid id)
        {
            try
            {
                _logger.LogInformation($"完成发货单，ID：{id}");

                var order = await _outboundRepository.GetAsync(id);

                if (order == null)
                {
                    _logger.LogWarning($"发货单不存在，ID：{id}");
                    throw new Exception("发货单不存在");
                }

                order.SetStatus(DeliveryOrderStatus.Completed);

                await _outboundRepository.UpdateAsync(order);

                _logger.LogInformation($"完成发货单成功，ID：{id}");

                return await GetDeliveryOrderByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"完成发货单异常，ID：{id}");
                throw;
            }
        }
    }
}