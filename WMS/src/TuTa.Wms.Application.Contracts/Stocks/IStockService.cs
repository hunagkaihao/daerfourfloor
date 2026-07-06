using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.Skips.Dtos;
using TuTa.Wms.Stocks.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TuTa.Wms.Stocks
{
    public interface IStockService : IApplicationService
    {
        /// <summary>
        /// 直接创建仓储任务
        /// </summary>
        /// <param name="stockInfo">库存信息</param>
        /// <param name="boxCode">容器编码</param>
        /// <returns></returns>
        //Task<ResponseDto> CreateStockTask(string boxCode,string startCode,string endCode);

        Task<ResponseDto> CreateStockAndBindBoxAsync(List<StockCreateDto> paras, string boxCode);

        /// <summary>
        /// 物料绑定容器（组盘入库），并更新ASN已入库数量
        /// </summary>
        Task<ResponseDto> CreateStockAndBindBoxWithAsnAsync(List<StockCreateDto> paras, string boxCode, string orderCode);
        //创建托盘搬运任务
        Task<ResponseDto> CreateStockTask(string boxCode, string startCellCode, string endCellCode);

        /// <summary>
        /// 创建容器搬运任务（无4A巷道校验，统一使用De02模板）
        /// </summary>
        Task<ResponseDto> CreateStockTaskV2(string boxCode, string startCellCode, string endCellCode);

        /// <summary>
        /// 直接创建库存并绑定到容器（无需收料条码）
        /// </summary>
        /// <param name="stockInfo">库存信息</param>
        /// <param name="boxCode">容器编码</param>
        /// <returns></returns>
        Task<ResponseDto> CreateStockDirectAndBindBoxAsync(StockDirectCreateDto stockInfo, string boxCode);
        
        /// <summary>
        /// 组盘清空
        /// </summary>
        /// <param name="boxCode"></param>
        /// <returns></returns>
        Task<ResponseDto> StockDisBindBox(string boxCode);

        /// <summary>
        /// 绑定库位
        /// </summary>
        /// <param name="paras"></param>
        /// <param name="cellCode"></param>
        /// <returns></returns>
        Task<ResponseDto> BindCellAsync(string boxCode, string cellCode);

        /// <summary>
        /// 解绑库位
        /// </summary>
        /// <param name="paras"></param>
        /// <param name="cellCode"></param>
        /// <returns></returns>
        Task<ResponseDto> DisBindCellAsync(string boxCode, string cellCode);

        /// <summary>
        /// 输送线呼叫
        /// </summary>
        /// <param name="boxCode"></param>
        /// <param name="height"></param>
        /// <param name="weight"></param>
        /// <param name="plpeCode"></param>
        /// <returns></returns>
        Task<ResponseDto> CreatePipelineInAsync(string boxCode, decimal height, decimal weight, string plpeCode);

        /// <summary>
        /// 料箱入库
        /// </summary>
        /// <param name="paras"></param>
        /// <param name="cellCode"></param>
        /// <returns></returns>
        Task<ResponseDto> CreateCTUBasicInAsync(string boxCode, string cellCode);

        /// <summary>
        /// 整车入库
        /// </summary>
        /// <param name="paras"></param>
        /// <param name="cellCode"></param>
        /// <returns></returns>
        Task<ResponseDto> CreateCTUSkipStockInAsync(SkipStockCtuInDto dto);

        /// <summary>
        /// 检验/复检入库
        /// </summary>
        /// <param name="barcode"></param>
        /// <param name="boxCode"></param>
        /// <param name="startCellCode"></param>
        /// <param name="endCellCode"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        Task<ResponseDto> CreateCTUCheckInAsync(string barcode, string boxCode, string startCellCode, string endCellCode, decimal count);

        /// <summary>
        /// 检查收料码是否已入库
        /// </summary>
        /// <param name="receivingMaterialBarcode">收料码</param>
        /// <returns>是否已入库</returns>
        Task<bool> CheckReceivingMaterialBarcodeExistsAsync(string receivingMaterialBarcode);

        /// <summary>
        /// 托盘入库
        /// </summary>
        /// <param name="paras"></param>
        /// <param name="boxCode"></param>
        /// <param name="startCellCode"></param>
        /// <param name="endCellCode"></param>
        /// <returns></returns>
        Task<ResponseDto> CreateLiftInAsync(List<StockCreateDto> paras, string boxCode, string startCellCode, string endCellCode);

        /// <summary>
        /// 创建库存并入库到库位，用于常规人工入库区
        /// </summary>
        /// <param name="paras"></param>
        /// <param name="cellCode"></param>
        /// <returns></returns>
        Task<ResponseDto> CreateStockAndBindCellAsync(string barcode, string cellCode, string boxCode, decimal pkgCount, decimal partsCount);

        /// <summary>
        /// 创建库存并入库到库位，用于常规人工入库，并且没用容器的场景
        /// </summary>
        /// <param name="paras"></param>
        /// <param name="cellCode"></param>
        /// <returns></returns>
        Task<ResponseDto> CreateStockAndBindCellNormalAsync(List<StockCreateDto> paras, string cellCode, string operatorName = null);

        /// <summary>
        /// 创建库存并入库到库位，用于复检后人工入库，并且没用容器的场景
        /// </summary>
        /// <param name="paras"></param>
        /// <param name="cellCode"></param>
        /// <returns></returns>
        Task<ResponseDto> CreateStockAndInBoundAfterReCheckAsync(List<StockCreateDto> paras, string cellCode, string operatorName = null);

        /// <summary>
        /// 库存分页查询
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        Task<PagedResultDto<StockDto>> GetPagedStocksAsync(PagedStockQueryDto para);

        /// <summary>
        /// 调拨库存查询
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        Task<PagedResultDto<StockDto>> GetPagedStocksByMoveAsync(PagedStockMoveQueryDto para);
        /// <summary>
        /// 托盘周转区料车入库物料查询
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        Task<PagedResultDto<StockDto>> GetCtuInStocksAsync(PagedStockQueryDto para);

        /// <summary>
        /// 托盘周转区料车查询
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        Task<PagedResultDto<SkipInDto>> GetCtuInSkipAsync();

        /// <summary>
        /// 库存查询
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        Task<List<StockDto>> GetStocksAsync(PagedStockQueryDto para);
        //Task<List<StockDto>> GetStocksAsync(StockQueryDto para);

        /// <summary>
        /// 查询库位中的所有库存信息
        /// </summary>
        /// <param name="cellCode"></param>
        /// <returns></returns>
        Task<List<StockDto>> GetStocksInCellAsync(string cellCode);

        /// <summary>
        /// 查询容器中的所有库存信息
        /// </summary>
        /// <param name="boxCode"></param>
        /// <returns></returns>
        Task<List<StockDto>> GetStocksInBoxAsync(string boxCode);

        /// <summary>
        /// 查询容器中的所有库存信息
        /// </summary>
        /// <param name="boxCode"></param>
        /// <returns></returns>
        Task<List<StockDto>> GetStocksAndCheckInBoxAsync(string boxCode);

        /// <summary>
        /// 查询料车中的所有库存信息
        /// </summary>
        /// <param name="boxCode"></param>
        /// <returns></returns>
        Task<List<StockDto>> GetStocksInSkipAsync(string skipCode);

        /// <summary>
        /// 查询料车中的所有料箱信息
        /// </summary>
        /// <param name="boxCode"></param>
        /// <returns></returns>
        Task<List<StockDto>> GetBoxsInSkipAsync(string skipCode);

        /// <summary>
        /// 车间收料确认
        /// </summary>
        /// <param name="boxCode"></param>
        /// <returns></returns>
        Task<ResponseDto> StockReceiptAsync(string boxCode);

        /// <summary>
        /// 查询库位中指定收料码的库存
        /// </summary>
        /// <param name="cellCode"></param>
        /// <param name="barcode"></param>
        /// <returns></returns>
        Task<StockDto> GetStockInCellWithBarcodeAsync(string cellCode, string barcode);

        /// <summary>
        /// 查询包含某种物料库存的所有库位
        /// </summary>
        /// <param name="materialCode"></param>
        /// <returns></returns>
        Task<List<CellWithMaterialDto>> GetCellsWithMaterialAsync(string materialCode ,string uniqueCode);

        /// <summary>
        /// 查询包含某种条码库存的所有库位
        /// </summary>
        /// <param name="materialCode"></param>
        /// <returns></returns>
        Task<List<CellWithMaterialDto>> GetCellsWithBarcodeAsync(string barcode);

        /// <summary>
        /// 库存移库
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        Task<ResponseDto> MoveStockAsync(StockMoveDto para);

        /// <summary>
        /// 库存移库
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        Task<ResponseDto> MoveStockAgvAsync(string boxCode,string barcode,int areaId);

        /// <summary>
        /// 批量调拨下架
        /// </summary>
        /// <param name="boxCode"></param>
        /// <returns></returns>
        Task<ResponseDto> MoveStocksWallAsync(List<string> boxCode);

        /// <summary>
        /// 物料拆箱
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        Task<ResponseDto> StockDevanningAsync(string boxCode, string barcode, string nextBoxCode, string cellCode, int count);

        /// <summary>
        /// 物料并箱
        /// </summary>
        /// <param name="boxCode"></param>
        /// <param name="nextBoxCode"></param>
        /// <param name="cellCode"></param>
        /// <returns></returns>
        Task<ResponseDto> StockMergeAsync(string boxCode, string nextBoxCode, string cellCode);

        /// <summary>
        /// 抽检分页查询
        /// </summary>
        /// <returns></returns>
        Task<List<StockCheckDto>> GetChecksByBox(string booxcode);

        /// <summary>
        /// 物料抽检
        /// </summary>
        /// <param name="barcode"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        Task<ResponseDto> StockCheckAsync(string barcode,string boxcode, int count);

        /// <summary>
        /// 直接删除库存，谨慎使用
        /// </summary>
        /// <param name="cellCode"></param>
        /// <param name="barcode"></param>
        /// <returns></returns>
        Task<ResponseDto> RemoveStockDirectAsync(Guid stockId);

        /// <summary>
        /// 直接出库，谨慎处理
        /// </summary>
        /// <param name="stockId"></param>
        /// <param name="outBoundCount"></param>
        /// <returns></returns>
        Task<ResponseDto> OutBountStockDirectAsync(Guid stockId, decimal outBoundCount, int? pagOrBoxCount = null);


    }
}
