using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using Swashbuckle.AspNetCore.Annotations;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.Skips.Dtos;
using TuTa.Wms.Stocks;
using TuTa.Wms.Stocks.Dtos;
using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.Controllers.Stocks
{
    [AllowAnonymous]
    [Route("wms/stock")]
    [ApiController]
    public class StockController : WmsController, IStockService
    {
        private readonly IStockService _stockService;

        private static readonly object _lock = new object();

        public StockController(IStockService stockService)
        {
            _stockService = stockService;
        }

        [HttpPost("pagedStocksQuery")]
        public async Task<PagedResultDto<StockDto>> GetPagedStocksAsync(PagedStockQueryDto para)
        {
            return await _stockService.GetPagedStocksAsync(para).ConfigureAwait(false);
        }

        [HttpPost("pagedMoveStocksQuery")]
        public async Task<PagedResultDto<StockDto>> GetPagedStocksByMoveAsync(PagedStockMoveQueryDto para)
        {
            return await _stockService.GetPagedStocksByMoveAsync(para).ConfigureAwait(false);
        }

        [HttpPost("pagedCtuInStocksQuery")]
        [SwaggerOperation(summary: "查询托盘周转区可入库物料", Tags = new[] { "Stock" })]
        public async Task<PagedResultDto<StockDto>> GetCtuInStocksAsync(PagedStockQueryDto para)
        {
            return await _stockService.GetCtuInStocksAsync(para).ConfigureAwait(false);
        }

        [HttpPost("pagedCtuInSkipsQuery")]
        [SwaggerOperation(summary: "查询托盘周转区可入库料车", Tags = new[] { "Stock" })]
        public async Task<PagedResultDto<SkipInDto>> GetCtuInSkipAsync()
        {
            return await _stockService.GetCtuInSkipAsync().ConfigureAwait(false);
        }

        [HttpPost("stocksQuery")]
        public async Task<List<StockDto>> GetStocksAsync(PagedStockQueryDto para)
        {
            return await _stockService.GetStocksAsync(para).ConfigureAwait(false);
        }

        //[HttpPost("stockCreateAndBindBox")]
        //[SwaggerOperation(summary: "物料绑定容器", Tags = new[] { "Stock" })]
        //public async Task<ResponseDto> CreateStockAndBindBoxAsync(string barcode,string boxCode,decimal pkgCount,decimal partsCount)
        //{
        //    await Task.Delay(1);
        //    lock (_lock)
        //    {
        //        return _stockService.CreateStockAndBindBoxAsync(barcode, boxCode, pkgCount, partsCount).GetAwaiter().GetResult();
        //    }
        //}

        /// <summary>
        /// 物料绑定容器（组盘入库）
        /// </summary>
        /// <param name="paras">库存创建参数</param>
        /// <param name="boxCode">容器编号</param>
        /// <returns>操作结果</returns>
        [HttpPost("stockCreateAndBindBox")]
        [SwaggerOperation(summary: "物料绑定容器", Tags = new[] { "Stock" })]
        public async Task<ResponseDto> CreateStockAndBindBoxAsync(List<StockCreateDto> paras, string boxCode)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _stockService.CreateStockAndBindBoxAsync(paras, boxCode).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// 物料绑定容器（组盘入库，关联ASN订单号）
        /// </summary>
        /// <param name="paras">库存创建参数</param>
        /// <param name="boxCode">容器编号</param>
        /// <param name="orderCode">订单号</param>
        /// <returns>操作结果</returns>
        [HttpPost("stockCreateAndBindBoxWithAsn")]
        [SwaggerOperation(summary: "物料绑定容器(ASN)", Tags = new[] { "Stock" })]
        public async Task<ResponseDto> CreateStockAndBindBoxWithAsnAsync(List<StockCreateDto> paras, string boxCode, string orderCode)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _stockService.CreateStockAndBindBoxWithAsnAsync(paras, boxCode, orderCode).GetAwaiter().GetResult();
            }
        }

        [HttpGet("checkReceivingMaterialBarcodeExists")]
        [SwaggerOperation(summary: "检查收料码是否已入库", Tags = new[] { "Stock" })]
        public async Task<bool> CheckReceivingMaterialBarcodeExistsAsync(string receivingMaterialBarcode)
        {
            return await _stockService.CheckReceivingMaterialBarcodeExistsAsync(receivingMaterialBarcode).ConfigureAwait(false);
        }
        /// <summary>
        /// 创建容器搬运任务
        /// </summary>
        /// <param name="paras">创建参数</param>
        /// <param name="boxCode">容器编码</param>
        /// <returns>操作结果</returns>
        [HttpPost("createStockTask")]
        [SwaggerOperation(summary: "创建容器搬运任务", Tags = new[] { "Stock" })]
        public async Task<ResponseDto> CreateStockTask(string boxCode, string startCellCode, string endCellCode)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _stockService.CreateStockTask(boxCode,startCellCode,endCellCode   ).GetAwaiter().GetResult();
            }   
        }

        /// <summary>
        /// 创建容器搬运任务（无4A巷道校验，统一使用De02模板）
        /// </summary>
        [HttpPost("createStockTaskV2")]
        [SwaggerOperation(summary: "创建容器搬运任务V2", Tags = new[] { "Stock" })]
        public async Task<ResponseDto> CreateStockTaskV2(string boxCode, string startCellCode, string endCellCode)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _stockService.CreateStockTaskV2(boxCode, startCellCode, endCellCode).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// 直接创建库存并绑定容器（无需收料条码）
        /// </summary>
        /// <param name="stockInfo">库存信息</param>
        /// <param name="boxCode">容器编码</param>
        /// <returns>操作结果</returns>
        [HttpPost("createStockDirectAndBindBox")]
        [SwaggerOperation(summary: "直接创建库存并绑定容器", Tags = new[] { "Stock" })]
        public async Task<ResponseDto> CreateStockDirectAndBindBoxAsync(StockDirectCreateDto stockInfo, string boxCode)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _stockService.CreateStockDirectAndBindBoxAsync(stockInfo, boxCode).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// 组盘取消
        /// </summary>
        /// <param name="boxCode">容器编码</param>
        /// <returns>操作结果</returns>
        [HttpPost("stocksDisBindBox")]
        [SwaggerOperation(summary: "组盘取消", Tags = new[] { "Stock" })]
        public async Task<ResponseDto> StockDisBindBox(string boxCode)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _stockService.StockDisBindBox(boxCode).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// 容器绑定库位
        /// </summary>
        /// <param name="boxCode">容器编码</param>
        /// <param name="cellCode">库位编码</param>
        /// <returns>操作结果</returns>
        [HttpPost("BoxBindCell")]
        [SwaggerOperation(summary: "容器绑定库位", Tags = new[] { "Stock" })]
        public async Task<ResponseDto> BindCellAsync(string boxCode,string cellCode)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _stockService.BindCellAsync(boxCode, cellCode).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// 容器解绑库位
        /// </summary>
        /// <param name="boxCode">容器编码</param>
        /// <param name="cellCode">库位编码</param>
        /// <returns>操作结果</returns>
        [HttpPost("BoxDisBindCell")]
        [SwaggerOperation(summary: "容器解绑库位", Tags = new[] { "Stock" })]
        public async Task<ResponseDto> DisBindCellAsync(string boxCode, string cellCode)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _stockService.DisBindCellAsync(boxCode, cellCode).GetAwaiter().GetResult();
            }
        }


        [HttpPost("CreatePipelineIn")]
        [SwaggerOperation(summary: "输送线下料", Tags = new[] { "Stock" })]
        public async Task<ResponseDto> CreatePipelineInAsync(string boxCode, decimal height, decimal weight, string plpeCode)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _stockService.CreatePipelineInAsync(boxCode, height, weight, plpeCode).GetAwaiter().GetResult();
            }
        }

        [HttpPost("CreateCtuBasicIn")]
        [SwaggerOperation(summary: "料箱入库", Tags = new[] { "Stock" })]
        public async Task<ResponseDto> CreateCTUBasicInAsync(string boxCode, string cellCode)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _stockService.CreateCTUBasicInAsync(boxCode, cellCode).GetAwaiter().GetResult();
            }
        }

        [HttpPost("CreateCtuSkipIn")]
        [SwaggerOperation(summary: "料箱整车入库", Tags = new[] { "Stock" })]
        public async Task<ResponseDto> CreateCTUSkipStockInAsync(SkipStockCtuInDto dto)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _stockService.CreateCTUSkipStockInAsync(dto).GetAwaiter().GetResult();
            }
        }

        [HttpPost("CreateCtuCheckIn")]
        [SwaggerOperation(summary: "料箱检验入库入库", Tags = new[] { "Stock" })]
        public async Task<ResponseDto> CreateCTUCheckInAsync(string barcode, string boxCode, string startCellCode, string endCellCode, decimal count)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _stockService.CreateCTUCheckInAsync(barcode, boxCode, startCellCode, endCellCode, count).GetAwaiter().GetResult();
            }
        }

        [HttpPost("CreateLiftIn")]
        [SwaggerOperation(summary: "托盘组盘入库", Tags = new[] { "Stock" })]
        public async Task<ResponseDto> CreateLiftInAsync(List<StockCreateDto> paras, string boxCode, string startCellCode, string endCellCode)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _stockService.CreateLiftInAsync(paras, boxCode, startCellCode, endCellCode).GetAwaiter().GetResult();
            }
        }

        [HttpPost("stockCreateAndBindCellIn")]
        [SwaggerOperation(summary: "手工入库", Tags = new[] { "Stock" })]
        public async Task<ResponseDto> CreateStockAndBindCellAsync(string barcode, string cellCode, string boxCode, decimal pkgCount, decimal partsCount)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _stockService.CreateStockAndBindCellAsync(barcode, cellCode, boxCode, pkgCount, partsCount).GetAwaiter().GetResult();
            }
        }

        [HttpPost("stockCreateAndBindCellInNormal")]
        public async Task<ResponseDto> CreateStockAndBindCellNormalAsync(List<StockCreateDto> paras, string cellCode, string operatorName = null)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _stockService.CreateStockAndBindCellNormalAsync(paras, cellCode, operatorName).GetAwaiter().GetResult();
            }            
        }

        [HttpPost("stockCreateAndBindCellAftReCheck")]
        public async Task<ResponseDto> CreateStockAndInBoundAfterReCheckAsync(List<StockCreateDto> paras, string cellCode, string operatorName = null)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _stockService.CreateStockAndInBoundAfterReCheckAsync(paras, cellCode).GetAwaiter().GetResult();
            }
        }

        [HttpPost("stockMove")]
        public async Task<ResponseDto> MoveStockAsync(StockMoveDto para)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _stockService.MoveStockAsync(para).GetAwaiter().GetResult();
            }
        }

        [HttpPost("stockMoveAgv")]
        [SwaggerOperation(summary: "agv移库", Tags = new[] { "Stock" })]
        public async Task<ResponseDto> MoveStockAgvAsync(string boxCode,string barcode,int areaId)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _stockService.MoveStockAgvAsync(boxCode, barcode, areaId).GetAwaiter().GetResult();
            }
        }

        [HttpPost("stocksMoveWall")]
        [SwaggerOperation(summary: "批量调拨下架", Tags = new[] { "Stock" })]
        public async Task<ResponseDto> MoveStocksWallAsync(List<string> boxCode)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _stockService.MoveStocksWallAsync(boxCode).GetAwaiter().GetResult();
            }
        }

        [HttpPost("stockDevanning")]
        [SwaggerOperation(summary: "物料拆箱", Tags = new[] { "Stock" })]
        public async Task<ResponseDto> StockDevanningAsync(string boxCode,string barcode,string nextBoxCode,string cellCode,int count)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _stockService.StockDevanningAsync(boxCode,barcode,nextBoxCode,cellCode,count).GetAwaiter().GetResult();
            }
        }

        [HttpPost("stockMerge")]
        [SwaggerOperation(summary: "物料合箱", Tags = new[] { "Stock" })]
        public async Task<ResponseDto> StockMergeAsync(string boxCode, string nextBoxCode, string cellCode)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _stockService.StockMergeAsync(boxCode, nextBoxCode, cellCode).GetAwaiter().GetResult();
            }
        }

        [HttpPost("GetStockChecksByBox")]
        [SwaggerOperation(summary: "料箱抽检数据查询", Tags = new[] { "Stock" })]
        public async Task<List<StockCheckDto>> GetChecksByBox(string boxcode)
        {
            return await _stockService.GetChecksByBox(boxcode);
        }

        // 其他项目的物料抽检
        [HttpPost("stockCheck")]
        [SwaggerOperation(summary: "物料抽检", Tags = new[] { "Stock" })]
        public async Task<ResponseDto> StockCheckAsync(string barcode,string boxcode, int count)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _stockService.StockCheckAsync(barcode, boxcode, count).GetAwaiter().GetResult();
            }
        }

        [HttpGet("stocksGetInCell")]
        public async Task<List<StockDto>> GetStocksInCellAsync(string cellCode)
        {
            return await _stockService.GetStocksInCellAsync(cellCode).ConfigureAwait(false);
        }

        [HttpGet("stocksAndCheckGetInBox")]
        [SwaggerOperation(summary: "查询容器中的库存和检验信息", Tags = new[] { "Stock" })]
        public async Task<List<StockDto>> GetStocksAndCheckInBoxAsync(string boxCode)
        {
            return await _stockService.GetStocksAndCheckInBoxAsync(boxCode).ConfigureAwait(false);
        }

        [HttpGet("stocksGetInBox")]
        [SwaggerOperation(summary: "查询容器中的库存", Tags = new[] { "Stock" })]
        public async Task<List<StockDto>> GetStocksInBoxAsync(string boxCode)
        {
            return await _stockService.GetStocksInBoxAsync(boxCode).ConfigureAwait(false);
        }

        [HttpGet("stocksGetInSkip")]
        [SwaggerOperation(summary: "查询料车中的库存", Tags = new[] { "Stock" })]
        public async Task<List<StockDto>> GetStocksInSkipAsync(string skipCode)
        {
            return await _stockService.GetStocksInSkipAsync(skipCode).ConfigureAwait(false);
        }

        [HttpGet("boxsGetInSkip")]
        [SwaggerOperation(summary: "查询料车中的料箱", Tags = new[] { "Stock" })]
        public async Task<List<StockDto>> GetBoxsInSkipAsync(string skipCode)
        {
            return await _stockService.GetBoxsInSkipAsync(skipCode).ConfigureAwait(false);
        }

        [HttpPost("stockReceipt")]
        [SwaggerOperation(summary: "车间领料", Tags = new[] { "Stock" })]
        public async Task<ResponseDto> StockReceiptAsync(string boxCode)
        {
            await Task.Delay(1);
            lock (_lock)
            {
                return _stockService.StockReceiptAsync(boxCode).GetAwaiter().GetResult();
            }
        }

        [HttpGet("stockWithBarcodeGetInCell")]
        public async Task<StockDto> GetStockInCellWithBarcodeAsync(string cellCode, string barcode)
        {
            return await _stockService.GetStockInCellWithBarcodeAsync(cellCode, barcode).ConfigureAwait(false);
        }

        [HttpGet("cellsWithMaterial")]
        public async Task<List<CellWithMaterialDto>> GetCellsWithMaterialAsync(string materialCode, string uniqueCode)
        {
            return await _stockService.GetCellsWithMaterialAsync(materialCode,uniqueCode).ConfigureAwait(false);
        }

        [HttpGet("cellsWithBarcode")]
        public async Task<List<CellWithMaterialDto>> GetCellsWithBarcodeAsync(string barcode)
        {
            return await _stockService.GetCellsWithBarcodeAsync(barcode).ConfigureAwait(false);
        }

        [HttpPost("stockRemoveDirect")]
        public async Task<ResponseDto> RemoveStockDirectAsync(Guid stockId)
        {
            return await _stockService.RemoveStockDirectAsync(stockId).ConfigureAwait(false);
        }

        [HttpPost("pushInspectionReport")]
        [SwaggerOperation(summary: "推送来料报检单", Tags = new[] { "Stock" })]
        public async Task<ResponseDto> PushInspectionReportAsync(List<Guid> stockIds)
        {
            return await _stockService.PushInspectionReportAsync(stockIds).ConfigureAwait(false);
        }

        [HttpPost("stockOutboundDirect")]
        public async Task<ResponseDto> OutBountStockDirectAsync(Guid stockId, decimal outBoundCount, int? pagOrBoxCount = null)
        {
            return await _stockService.OutBountStockDirectAsync(stockId, outBoundCount, pagOrBoxCount).ConfigureAwait(false);
        }

        [HttpPost("stockInspection")]
        public async Task<ResponseDto> StockInspectionAsync(Guid stockId, decimal outBoundCount, int? pagOrBoxCount = null)
        {
            return await _stockService.StockInspectionAsync(stockId, outBoundCount, pagOrBoxCount).ConfigureAwait(false);
        }

        [HttpPost("setInspectionCompleted")]
        public async Task<ResponseDto> SetInspectionCompletedAsync(Guid stockId)
        {
            return await _stockService.SetInspectionCompletedAsync(stockId).ConfigureAwait(false);
        }

        //[HttpPost("StocksCreateAndBindToCell")]
        //public async Task<ResponseDto> CreateStockAndBindToCellAsync(List<StockCreateDto> paras, string cellCode)
        //{
        //    await Task.Delay(1);
        //    lock(_lock)
        //    {
        //        return _stockService.CreateStockAndBindToCellAsync(paras, cellCode).GetAwaiter().GetResult();
        //    }
        //}
    }
}

