using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using System;
using System.Threading.Tasks;
using TuTa.Wms.Erp.Entities;
using TuTa.Wms.StockInHistories.Aggregates;
using TuTa.Wms.StockInHistories.ValueObjects;
using TuTa.Wms.Stocks;
using TuTa.Wms.Stocks.Aggregates;
using TuTa.Wms.Stocks.Events;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Uow;
using Wms.LogTool;

namespace TuTa.Wms.Erp.EventHandlers
{
    public class Erp_StockEventHandler
         : ILocalEventHandler<StockBindBoxAndCellEvent>, //物料人工入库
           ILocalEventHandler<StockPickOutEvent>, //正常领用
           ILocalEventHandler<StockRecheckOutEvent>, //复检领用
           ILocalEventHandler<StockMoveEvent>, //复检领用
           ILocalEventHandler<StockCheckEvent>, //复检领用
           ITransientDependency
    {
        private readonly IRepository<ErpStockInReturn, int> _stockInReturnRepository;
        private readonly IRepository<ErpStockOutReturn, int> _stockOutReturnRepository;
        private readonly IRepository<ErpStockMoveReturn, int> _stockMoveReturnRepository;
        private readonly IRepository<ErpStockCheck, int> _stockCheckRepository;
        private readonly UnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger<Erp_StockEventHandler> _logger;
        private readonly LocalEventBus _localEventBus;

        private static readonly object _locker = new object();

        public Erp_StockEventHandler(
            IRepository<ErpStockInReturn, int> stockInReturnRepository,
            IRepository<ErpStockOutReturn, int> stockOutReturnRepository,
            IRepository<ErpStockMoveReturn, int> stockMoveReturnRepository,
            IRepository<ErpStockCheck, int> stockCheckRepository,
        UnitOfWorkManager unitOfWorkManager,
            ILogger<Erp_StockEventHandler> logger,
            LocalEventBus localEventBus)
        {
            _stockInReturnRepository = stockInReturnRepository;
            _stockOutReturnRepository = stockOutReturnRepository;
            _stockMoveReturnRepository = stockMoveReturnRepository;
            _stockCheckRepository = stockCheckRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _logger = logger;
            _localEventBus = localEventBus;
        }

        public async Task HandleEventAsync(StockBindBoxAndCellEvent eventData)
        {
            //TODO: your code that does somthing on the event
            using (IUnitOfWork uow = _unitOfWorkManager.Begin())
            {
                try
                {
                    if (eventData == null)
                        throw new ArgumentNullException(nameof(eventData));

                    if (eventData.StockInType == "盘点入库") //盘点入库不进入ERP
                        return;

                    //WMS的暂存区与ERP的仓库的对应关系
                    string stockInHouseCode = null, stockInHouseName = null;
                    switch(eventData.AreaCode)
                    {
                        case "001":
                            stockInHouseCode = "01";
                            stockInHouseName = "东方综合库";
                            break;
                        case "002":
                            stockInHouseCode = "04";
                            stockInHouseName = "待处理库";
                            break;
                        case "003":
                            stockInHouseCode = "26";
                            stockInHouseName = "采购暂存库";
                            break;
                        default:
                            break;                        
                    }

                    ErpStockInReturn stockInReturn = new ErpStockInReturn(
                        eventData.StockInDate,
                        eventData.SupplierCode, eventData.SupplierName,
                        StockInTypeHelper.ChineseToStockInType(eventData.StockInType), 
                        eventData.MaterialCode, eventData.MaterialName, eventData.Specs, eventData.Unit,
                        stockInHouseCode, stockInHouseName,
                        eventData.CheckNo, 
                        eventData.StockBarcode,
                        eventData.CheckOrderCode,
                        eventData.StockCount,
                        eventData.Operator,
                        eventData.AreaCode,
                        eventData.AreaName);

                    await _stockInReturnRepository.InsertAsync(stockInReturn).ConfigureAwait(false);

                    await uow.SaveChangesAsync().ConfigureAwait(false);
                    await uow.CompleteAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    //await uow.RollbackAsync().ConfigureAwait(false);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

        public async Task HandleEventAsync(StockPickOutEvent eventData)
        {
            //TODO: your code that does somthing on the event
            using (IUnitOfWork uow = _unitOfWorkManager.Begin())
            {
                try
                {
                    if (eventData == null)
                        throw new ArgumentNullException(nameof(eventData));
                    
                    ErpStockOutReturn stockOutReturn = new ErpStockOutReturn(
                        eventData.StockOutTime,
                        eventData.GysCode, eventData.GysName, eventData.DeptCode, eventData.DeptName,
                        eventData.StockOutType, eventData.PickBatch, 
                        eventData.GoodsCode, eventData.GoodsName, eventData.GoodsSpecs,
                        eventData.UniqueCode, 
                        eventData.Material.MaterialCode, eventData.Material.MaterialName, eventData.Material.Specs, eventData.Material.Unit,
                        eventData.CheckData.CheckNo, eventData.Barcode, eventData.StockOutCount, eventData.OperatorName,
                        eventData.AreaCode,eventData.AreaName, eventData.PickManName);

                    await _stockOutReturnRepository.InsertAsync(stockOutReturn).ConfigureAwait(false);

                    await uow.SaveChangesAsync().ConfigureAwait(false);
                    await uow.CompleteAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    //await uow.RollbackAsync().ConfigureAwait(false);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

        public async Task HandleEventAsync(StockRecheckOutEvent eventData)
        {
            //TODO: your code that does somthing on the event
            using (IUnitOfWork uow = _unitOfWorkManager.Begin())
            {
                try
                {
                    if (eventData == null)
                        throw new ArgumentNullException(nameof(eventData));

                    ErpStockOutReturn stockOutReturn = new ErpStockOutReturn(
                        eventData.StockOutTime,
                        eventData.GysCode, eventData.GysName, eventData.DeptCode, eventData.DeptName,
                        eventData.StockOutType, eventData.PickBatch,
                        eventData.GoodsCode, eventData.GoodsName, eventData.GoodsSpecs,
                        eventData.UniqueCode,
                        eventData.Material.MaterialCode, eventData.Material.MaterialName, eventData.Material.Specs, eventData.Material.Unit,
                        eventData.CheckData.CheckNo, eventData.Barcode, eventData.StockOutCount, eventData.OperatorName,null,null);

                    await _stockOutReturnRepository.InsertAsync(stockOutReturn).ConfigureAwait(false);

                    await uow.SaveChangesAsync().ConfigureAwait(false);
                    await uow.CompleteAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    //await uow.RollbackAsync().ConfigureAwait(false);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

        public async Task HandleEventAsync(StockMoveEvent eventData)
        {
            //TODO: your code that does somthing on the event
            using (IUnitOfWork uow = _unitOfWorkManager.Begin())
            {
                try
                {
                    if (eventData == null)
                        throw new ArgumentNullException(nameof(eventData));

                    ErpStockMoveReturn stockMoveReturn = new ErpStockMoveReturn(
                        eventData.MoveDate,
                        eventData.SupplierCode, eventData.SupplierName,
                        eventData.MaterialCode, eventData.MaterialName, eventData.MaterialSpecs, eventData.MaterialUnit, 
                        eventData.CheckNo, eventData.Barcode, eventData.MoveCount, eventData.MoveType, eventData.OperatorName);

                    await _stockMoveReturnRepository.InsertAsync(stockMoveReturn).ConfigureAwait(false);

                    await uow.SaveChangesAsync().ConfigureAwait(false);
                    await uow.CompleteAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    //await uow.RollbackAsync().ConfigureAwait(false);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }

        public async Task HandleEventAsync(StockCheckEvent eventData)
        {
            //TODO: your code that does somthing on the event
            using (IUnitOfWork uow = _unitOfWorkManager.Begin())
            {
                try
                {
                    if (eventData == null)
                        throw new ArgumentNullException(nameof(eventData));

                    var list = await _stockCheckRepository.ToListAsync();

                    Console.WriteLine(JsonConvert.SerializeObject(list));

                    ErpStockCheck stockCheck = new ErpStockCheck(
                        eventData.Barcode,eventData.Boxcode,eventData.Count);

                    await _stockCheckRepository.InsertAsync(stockCheck).ConfigureAwait(false);

                    await uow.SaveChangesAsync().ConfigureAwait(false);
                    await uow.CompleteAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    //await uow.RollbackAsync().ConfigureAwait(false);
                    throw new UserFriendlyException(ex.Message);
                }
            }
        }
    }
}
