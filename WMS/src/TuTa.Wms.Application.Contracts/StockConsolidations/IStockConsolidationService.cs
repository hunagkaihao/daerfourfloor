using System.Threading.Tasks;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.StockConsolidations.Dtos;
using Volo.Abp.Application.Services;

namespace TuTa.Wms.StockConsolidations
{
    /// <summary>
    /// 四楼库存整理应用服务契约。
    /// </summary>
    public interface IStockConsolidationService : IApplicationService
    {
        /// <summary>
        /// 启动库存整理后台线程。
        /// </summary>
        Task<ResponseDto> StartAsync();

        /// <summary>
        /// 请求安全停止库存整理后台线程。
        /// 已经下发的AGV任务继续执行，线程不会再创建下一条任务。
        /// </summary>
        Task<ResponseDto> StopAsync();

        /// <summary>
        /// 获取当前库存整理线程的运行状态。
        /// </summary>
        Task<StockConsolidationStatusDto> GetStatusAsync();
    }
}
