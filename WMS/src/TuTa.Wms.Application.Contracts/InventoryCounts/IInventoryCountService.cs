using System.Threading.Tasks;
using TuTa.Wms.Application.Contracts.Shared;
using Volo.Abp.Application.Services;

namespace TuTa.Wms.InventoryCounts;

/// <summary>独立实物盘点的保存和按日Excel导出。</summary>
public interface IInventoryCountService : IApplicationService
{
    /// <summary>保存一笔盘点明细，重复提交相同标识及数据时返回成功。</summary>
    Task<ResponseDto> CreateAsync(InventoryCountCreateDto input);
    /// <summary>按服务器本地保存日期导出全部明细，日期格式必须为yyyy-MM-dd。</summary>
    Task<byte[]> ExportAsync(string date);
}
