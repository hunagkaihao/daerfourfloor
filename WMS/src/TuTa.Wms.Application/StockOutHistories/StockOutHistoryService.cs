using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuTa.Wms.StockOutHistories.Dtos;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Wms.LogTool;

namespace TuTa.Wms.StockOutHistories
{
    public class StockOutHistoryService : WmsAppService, IStockOutHistoryService
    {
        private readonly IStockOutHistoryRepository _stockOutHistoryRepository;
        private readonly ILogger<StockOutHistoryService> _logger;

        public StockOutHistoryService(
            IStockOutHistoryRepository stockOutHistoryRepository,
            ILogger<StockOutHistoryService> logger)
        {
            _stockOutHistoryRepository = stockOutHistoryRepository;
            _logger = logger;
        }

        public async Task<PagedResultDto<StockOutHistoryDto>> GetPagedStockOutHistoriesAsync(PagedStockOutHistoryQueryDto para)
        {
            try
            {
                var stockHistories = await _stockOutHistoryRepository.GetPagedStockOutHistoriesAsync(
                    para.Barcode,
                    para.MaterialCode, para.MaterialNameTip, para.MaterialSpecsTip,
                    para.StockOutType,
                    para.StockOutTimeMin, para.StockOutTimeMax, para.CheckNoTip, para.PickBatchTip,
                    false, para.SkipCount, para.MaxResultCount);

                if (stockHistories == null || stockHistories.TotalCount == 0 || stockHistories.Items == null)
                    return new PagedResultDto<StockOutHistoryDto>() { TotalCount = 0, Items = new List<StockOutHistoryDto>() };

                PagedResultDto<StockOutHistoryDto> result = new PagedResultDto<StockOutHistoryDto>() { TotalCount = stockHistories.TotalCount };

                List<StockOutHistoryDto> stockInHistoryDtos = new List<StockOutHistoryDto>();
                foreach (var stockHistory in stockHistories.Items)
                {
                    StockOutHistoryDto historyDto = new StockOutHistoryDto()
                    {
                        Id = stockHistory.Id,
                        Barcode = stockHistory.Barcode,
                        BoxCode = stockHistory.BoxCode,
                        BoxName = stockHistory.BoxName,
                        CellCode = stockHistory.CellCode,
                        CellName = stockHistory.CellName,
                        AreaCode = stockHistory.AreaCode,
                        AreaName = stockHistory.AreaName,
                        HouseCode = stockHistory.WarehouseCode,
                        HouseName = stockHistory.WarehouseName,
                        StockOutCount = stockHistory.OutCount,
                        StockOutTime = stockHistory.OutTime,
                        MaterialCode = stockHistory.MaterialCode,
                        MaterialName = stockHistory.MaterialName,
                        Specs = stockHistory.MaterialSpecs,
                        Unit = stockHistory.MaterialUnit,
                        StockOutType = stockHistory.StockOutType,
                        Operator = stockHistory.OperatorName,
                        PickBatch = stockHistory.BatchNo
                    };
                    stockInHistoryDtos.Add(historyDto);
                }

                result.Items = stockInHistoryDtos;

                return result;

            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }
    }
}