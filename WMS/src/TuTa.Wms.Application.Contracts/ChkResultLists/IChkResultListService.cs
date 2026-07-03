using System;
using System.Threading.Tasks;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.ChkResultLists.Dtos;
using TuTa.Wms.PickLists.Dtos;

using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TuTa.Wms.ChkResultLists
{
    public interface IChkResultListService : IApplicationService
    {
        //Task<ResponseDto> CreateStockAndBindToCellAsync(List<MaterialToInBoundDto> paras, string cellCode);

        /// <summary>
        /// 处理误出库的情况，根据出库历史数据重新创建检验数据，再次入库
        /// </summary>
        /// <param name="stockoutHistoryId"></param>
        /// <returns></returns>
        Task<ResponseDto> CreateChkResultListFromStockoutHistoryAsync(int stockoutHistoryId);

        Task<ChkResultListDto> GetChkResultListByBarcodeAsync(string barcode);

        Task<PagedResultDto<ChkResultListDto>> GetPagedCheckInItemsAsync(PagedCheckItemQueryDto para);

        Task<GetChkByBarcodeDto> GetCheckByBarcodeAsync(string barcode,string chkNo);
    }
}
