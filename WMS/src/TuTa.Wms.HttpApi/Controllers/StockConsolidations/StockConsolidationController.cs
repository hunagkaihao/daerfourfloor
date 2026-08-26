using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.StockConsolidations;
using TuTa.Wms.StockConsolidations.Dtos;

namespace TuTa.Wms.Controllers.StockConsolidations
{
    /// <summary>
    /// 四楼库存整理线程调度接口。
    /// Controller只负责接收前端请求，实际线程和业务逻辑由Application服务处理。
    /// </summary>
    [AllowAnonymous]
    [Route("wms/stock-consolidation")]
    [ApiController]
    public class StockConsolidationController : WmsController
    {
        private readonly IStockConsolidationService _stockConsolidationService;

        public StockConsolidationController(IStockConsolidationService stockConsolidationService)
        {
            _stockConsolidationService = stockConsolidationService;
        }

        /// <summary>
        /// 启动库存整理线程。
        /// </summary>
        [HttpPost("start")]
        [SwaggerOperation(summary: "启动四楼库存整理线程", Tags = new[] { "StockConsolidation" })]
        public Task<ResponseDto> StartAsync()
        {
            return _stockConsolidationService.StartAsync();
        }

        /// <summary>
        /// 安全停止库存整理线程。
        /// </summary>
        [HttpPost("stop")]
        [SwaggerOperation(summary: "停止四楼库存整理线程", Tags = new[] { "StockConsolidation" })]
        public Task<ResponseDto> StopAsync()
        {
            return _stockConsolidationService.StopAsync();
        }

        /// <summary>
        /// 查询库存整理线程状态。
        /// </summary>
        [HttpGet("status")]
        [SwaggerOperation(summary: "查询四楼库存整理线程状态", Tags = new[] { "StockConsolidation" })]
        public Task<StockConsolidationStatusDto> GetStatusAsync()
        {
            return _stockConsolidationService.GetStatusAsync();
        }
    }
}
