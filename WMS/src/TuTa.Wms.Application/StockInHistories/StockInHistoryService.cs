using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuTa.Wms.StockInHistories.Dtos;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Wms.LogTool;

namespace TuTa.Wms.StockInHistories
{
    public class StockInHistoryService : WmsAppService, IStockInHistoryService
    {
        private readonly IStockInHistoryRepository _stockInHistoryRepository;
        private readonly ILogger<StockInHistoryService> _logger;

        public StockInHistoryService(
            IStockInHistoryRepository stockHistoryRepository,
            ILogger<StockInHistoryService> logger)
        {
            _stockInHistoryRepository = stockHistoryRepository;
            _logger = logger;
        }

        public async Task<PagedResultDto<StockInHistoryDto>> GetPagedStockInHistoriesAsync(PagedStockInHistoryQueryDto para)
        {
            try
            {
                var stockHistories = await _stockInHistoryRepository.GetPagedStockInHistoriesAsync(
                    para.Barcode,
                    para.MaterialCode, para.MaterialNameTip, para.MaterialSpecsTip,
                    para.StockInType,
                    para.StockInTimeStart, para.StockInTimeEnd, para.CheckNoTip,
                    false, para.SkipCount, para.MaxResultCount);

                if (stockHistories == null || stockHistories.TotalCount == 0 || stockHistories.Items == null) 
                    return new PagedResultDto<StockInHistoryDto>() { TotalCount = 0, Items = new List<StockInHistoryDto>() };

                PagedResultDto<StockInHistoryDto> result = new PagedResultDto<StockInHistoryDto>() { TotalCount = stockHistories.TotalCount };

                List<StockInHistoryDto> stockInHistoryDtos = new List<StockInHistoryDto>();
                foreach( var stockHistory in stockHistories.Items )
                {
                    StockInHistoryDto historyDto = new StockInHistoryDto()
                    {
                        Id = stockHistory.Id,
                        Barcode = stockHistory.Barcode,
                        BoxCode = stockHistory.BoxCode,
                        BoxName = stockHistory.BoxName,
                        CellCode = stockHistory.CellCode,
                        CellName = stockHistory.CellName,
                        AreaCode = stockHistory.AreaCode,
                        AreaName = stockHistory.AreaName,
                        WarehouseCode = stockHistory.WarehouseCode,
                        WarehouseName = stockHistory.WarehouseName,
                        InCount = stockHistory.InCount,
                        InTime = stockHistory.InTime,
                        MaterialCode = stockHistory.MaterialCode,
                        MaterialName = stockHistory.MaterialName,
                        MaterialSpecs = stockHistory.MaterialSpecs,
                        MaterialUnit = stockHistory.MaterialUnit,
                        StockInType = stockHistory.StockInType,
                        OperatorName = stockHistory.OperatorName,
                        BatchNo = stockHistory.BatchNo
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
