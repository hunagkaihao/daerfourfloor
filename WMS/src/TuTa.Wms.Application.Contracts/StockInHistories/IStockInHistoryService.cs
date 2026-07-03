using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TuTa.Wms.StockInHistories.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TuTa.Wms.StockInHistories
{
    public interface IStockInHistoryService : IApplicationService
    {
        Task<PagedResultDto<StockInHistoryDto>> GetPagedStockInHistoriesAsync(PagedStockInHistoryQueryDto para);
    }
}
