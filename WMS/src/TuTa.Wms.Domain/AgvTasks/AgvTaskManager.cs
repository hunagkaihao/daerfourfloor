using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TuTa.Wms.AgvTasks.Aggregaes;
using TuTa.Wms.Cells.Aggregates;
using TuTa.Wms.Cells;
using TuTa.Wms.Cells.Entities;
using Volo.Abp.Uow;
using Volo.Abp;
using Microsoft.Extensions.Logging;
using Wms.LogTool;
using TuTa.Wms.Boxes;
using TuTa.Wms.Warehouses;
using TuTa.Wms.Boxes.Aggregates;
using TuTa.Wms.Warehouses.Aggregates;
using TuTa.Wms.Warehouses.Entities;
using TuTa.Wms.Stocks;
using TuTa.Wms.Stocks.Aggregates;
using TuTa.Wms.ChkResultLists;
using TuTa.Wms.ChkResultLists.Aggregates;
using TuTa.Wms.Stocks.ValueObjects;
using TuTa.Wms.Skips.Aggregates;
using TuTa.Wms.Skips;
using TuTa.Wms.PickLists;
using TuTa.Wms.BarcodeLists;
using Volo.Abp.EventBus.Local;
using TuTa.Wms.Stocks.Events;
using Microsoft.AspNetCore.Identity;
using Volo.Abp.Identity;
using TuTa.Wms.PickLists.Events;
using TuTa.Wms.PickLists.Aggregates;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace TuTa.Wms.AgvTasks
{
    public class AgvTaskManager : WmsDomainService
    {
        private readonly IAgvTaskRepository _agvTaskRepository;
        private readonly RcsApiManager _rcsApiManager;
        private readonly IBoxRepository _boxRepository;
        private readonly ICellRepository _cellRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IChkResultListRepository _chkResultListRepository;
        private readonly IBarcodeCheckRepository _barcodeCheckRepository;
        private readonly ISkipRepository _skipRepository;
        private readonly IPickListRepository _pickListRepository;
        private readonly IStockRepository _stockRepository;
        private readonly IdentityUserManager _userManager;
        private readonly PickListManager _pickListManager;
        private readonly AGVOptions _aGVOptions;
        private readonly LocalEventBus _localEventBus;
        private readonly ILogger<AgvTaskManager> _logger;
        public AgvTaskManager(IAgvTaskRepository agvTaskRepository,
            RcsApiManager rcsApiManager
            , ICellRepository cellRepository
            , IWarehouseRepository warehouseRepository
            , IBoxRepository boxRepository
            , IStockRepository stockRepository
            , IChkResultListRepository chkResultListRepository
            , IBarcodeCheckRepository barcodeCheckRepository
            , ISkipRepository skipRepository
            ,IPickListRepository pickListRepository
            , IOptionsSnapshot<AGVOptions> aGVOptions
            , IUnitOfWorkManager unitofWorkManager
            , IdentityUserManager userManager
            , PickListManager pickListManager
            , LocalEventBus localEventBus
            , ILogger<AgvTaskManager> logger

            )
        {
            _agvTaskRepository = agvTaskRepository;
            _cellRepository = cellRepository;
            _rcsApiManager = rcsApiManager;
            _boxRepository = boxRepository;
            _warehouseRepository = warehouseRepository;
            _stockRepository = stockRepository;
            _chkResultListRepository = chkResultListRepository;
            _barcodeCheckRepository = barcodeCheckRepository;
            _skipRepository = skipRepository;
            _pickListRepository = pickListRepository;
            _userManager = userManager;
            _pickListManager = pickListManager;
            _aGVOptions = aGVOptions.Value;
            _localEventBus = localEventBus;
            _logger = logger;
        }
        
        [UnitOfWork]
        public async Task<AgvTask> CreateCtuStockInByStockTaskAsync(string boxCode, string ctnrTyp, string startCellName, string endCellName, string podCode, ManageType type, bool dispatchToRcs = true, string taskTypOverride = null)
        {
            var reqCode = Guid.NewGuid().ToString("N");
            var taskTyp = taskTypOverride ?? _aGVOptions.CTUTaskType;
            string[] userCallCodePath = new string[2];
            userCallCodePath[0] = startCellName ;//按照仓位下达任务
            userCallCodePath[1] = endCellName ;//按照仓位下达任务

            var entity = new AgvTask(reqCode, taskTyp, podCode, userCallCodePath, boxCode,
            startCellName, endCellName, ctnrTyp, type);
            var result = await _agvTaskRepository.InsertAsync(entity);

            if (dispatchToRcs)
            {
                await SetAsExecutingAsync(entity);
            }

            return result;
        }

        /// <summary>
        /// 创建库存整理专用AGV任务。
        /// 起点和终点在WMS任务中保存业务库位编码；真正下发RCS时，
        /// <see cref="SetAsExecutingAsync(AgvTask)"/> 会重新读取库位CellName，
        /// 因此能够同时支持“仓库位${05}”和“4B暂存位”两种RCS点位格式。
        /// </summary>
        [UnitOfWork]
        public async Task<AgvTask> CreateStockConsolidationTaskAsync(
            string boxCode,
            string ctnrTyp,
            string startCellCode,
            string endCellCode,
            string taskType,
            bool dispatchToRcs = true)
        {
            var reqCode = Guid.NewGuid().ToString("N");
            var userCallCodePath = new[] { startCellCode, endCellCode };

            // WMS业务类型必须明确记录为StockConsolidation。
            // 不能因为RCS模板执行了类似入库/出库的物理动作，就复用普通入库或出库业务类型。
            var entity = new AgvTask(
                reqCode,
                taskType,
                null,
                userCallCodePath,
                boxCode,
                startCellCode,
                endCellCode,
                ctnrTyp,
                ManageType.StockConsolidation);

            var result = await _agvTaskRepository.InsertAsync(entity, true).ConfigureAwait(false);
            if (dispatchToRcs)
            {
                await SetAsExecutingAsync(entity).ConfigureAwait(false);
            }

            return result;
        }

        [UnitOfWork]
        public async Task<AgvTask> CreateCtuSSXTaskAsync(string boxCode, string ctnrTyp, string startCellName, string endCellName, string podCode, ManageType type)
        {
            var reqCode = Guid.NewGuid().ToString("N");
            var taskTyp = _aGVOptions.CTUTaskXianType;
            string[] userCallCodePath = new string[2];
            userCallCodePath[0] = startCellName + "${05}";//按照仓位下达任务
            userCallCodePath[1] = endCellName + "${05}";//按照仓位下达任务

            var entity = new AgvTask(reqCode, taskTyp, podCode, userCallCodePath, boxCode,
            startCellName, endCellName, ctnrTyp, type);
            var result = await _agvTaskRepository.InsertAsync(entity);


            await SetAsExecutingAsync(entity);

            return result;
        }

        [UnitOfWork]
        public async Task<AgvTask> CreateLiftStockInByStockTaskAsync(string boxCode, string ctnrTyp, string startCellName, string endCellName, string podCode, ManageType type)
        {
            var reqCode = Guid.NewGuid().ToString("N");
            var taskTyp = _aGVOptions.LiftTaskType;
            string[] userCallCodePath = new string[2];
            userCallCodePath[0] = startCellName + "${05}";//按照仓位下达任务
            userCallCodePath[1] = endCellName + "${05}";//按照仓位下达任务

            var entity = new AgvTask(reqCode, taskTyp, podCode, userCallCodePath, boxCode,
            startCellName, endCellName, ctnrTyp, type);
            var result = await _agvTaskRepository.InsertAsync(entity);

            await SetAsExecutingAsync(entity);

            return result;
        }

        [UnitOfWork]
        public async Task<AgvTask> CreateLiftSSXTaskAsync(string boxCode, string ctnrTyp, string startCellName, string endCellName, string podCode, ManageType type)
        {
            var reqCode = Guid.NewGuid().ToString("N");
            var taskTyp = _aGVOptions.LiftTaskXianType;
            string[] userCallCodePath = new string[2];
            userCallCodePath[0] = startCellName + "${05}";//按照仓位下达任务
            userCallCodePath[1] = endCellName + "${05}";//按照仓位下达任务

            var entity = new AgvTask(reqCode, taskTyp, podCode, userCallCodePath, boxCode,
            startCellName, endCellName, ctnrTyp, type);
            var result = await _agvTaskRepository.InsertAsync(entity);

            await SetAsExecutingAsync(entity);

            return result;
        }

        [UnitOfWork]
        public async Task<AgvTask> CreateSkipTaskAsync(string startCellName, string endCellName, string podCode, ManageType type)
        {
            var reqCode = Guid.NewGuid().ToString("N");
            string taskTyp = "";
            string[] userCallCodePath = null;
            if(type == ManageType.SkipSend)
            {
                taskTyp = _aGVOptions.SkipSendType;
                userCallCodePath = new string[3];
                userCallCodePath[0] = startCellName;//按照仓位下达任务
                userCallCodePath[1] = endCellName;//按照仓位下达任务
                userCallCodePath[2] = "1";
            }
            else if (type == ManageType.SkipCall)
            {
                taskTyp = _aGVOptions.SkipCallType;
                userCallCodePath = new string[3];
                userCallCodePath[0] = "1";
                userCallCodePath[1] = startCellName;//按照仓位下达任务
                userCallCodePath[2] = endCellName;//按照仓位下达任务
            }
            else
            {
                taskTyp = _aGVOptions.SkipTaskType;
                userCallCodePath = new string[2];
                userCallCodePath[0] = startCellName;//按照仓位下达任务
                userCallCodePath[1] = endCellName;//按照仓位下达任务
            }

            var entity = new AgvTask(reqCode, taskTyp, podCode, userCallCodePath, null,
            startCellName, endCellName, null, type);
            var result = await _agvTaskRepository.InsertAsync(entity);

            await SetAsExecutingAsync(entity);

            return result;
        }



        [UnitOfWork]
        public async Task<AgvTask> CreateSkipCallTaskAsync(string startCellName, string podCode, ManageType type)
        {
            var reqCode = Guid.NewGuid().ToString("N");
            string taskTyp = "";
            string[] userCallCodePath = null;
            taskTyp = _aGVOptions.SkipCallType;
            userCallCodePath = new string[3];
            userCallCodePath[0] = "1";
            userCallCodePath[1] = startCellName;//按照仓位下达任务
            userCallCodePath[2] = "";//按照仓位下达任务

            var entity = new AgvTask(reqCode, taskTyp, podCode, userCallCodePath, null,
            startCellName, null, null, type);
            var result = await _agvTaskRepository.InsertAsync(entity);

            await SetAsExecutingAsync(entity);

            return result;
        }

        [UnitOfWork]
        public async Task<AgvTask> CreateSkipMoveAsync(string startCellName, string endCellName, string podCode , ManageType type)
        {
            var reqCode = Guid.NewGuid().ToString("N");
            var taskTyp = _aGVOptions.SkipTaskType;
            string[] userCallCodePath = new string[2];
            userCallCodePath[0] = startCellName ;//按照仓位下达任务
            userCallCodePath[1] = endCellName;//按照仓位下达任务

            var entity = new AgvTask(reqCode, taskTyp, podCode, userCallCodePath,
            startCellName, endCellName,type);
            var result = await _agvTaskRepository.InsertAsync(entity);

            await SetAsExecutingAsync(entity);

            return result;
        }

        
        [UnitOfWork]
        public async Task<AgvTask> CreateLiftStockOutByStockTaskAsync(string boxCode, string ctnrTyp, string startCellName, string endCellName , ManageType type, bool dispatchToRcs = true, string taskTypOverride = null)
        {
            var reqCode = Guid.NewGuid().ToString("N");
            var taskTyp = taskTypOverride ?? _aGVOptions.LiftTaskType;
            string[] userCallCodePath = new string[2];
            userCallCodePath[0] = startCellName + "${05}";//按照仓位下达任务
            userCallCodePath[1] = endCellName + "${05}";//按照仓位下达任务

            var entity = new AgvTask(reqCode, taskTyp, null, userCallCodePath, boxCode,
            startCellName, endCellName, ctnrTyp,type);
            var result = await _agvTaskRepository.InsertAsync(entity);

            if (dispatchToRcs)
            {
                await SetAsExecutingAsync(entity);
            }

            return result;
        }

        
        [UnitOfWork]
        public async Task<AgvTask> CreateCTUStockOutByStockTaskAsync(string boxCode, string ctnrTyp, string startCellName, string endCellName,string podCode,ManageType type,string picklist,string unique, bool dispatchToRcs = true, string taskTypOverride = null)
        {
            var reqCode = Guid.NewGuid().ToString("N");
            var taskTyp = taskTypOverride ?? _aGVOptions.CTUTaskType;
            string[] userCallCodePath = new string[2];
            userCallCodePath[0] = startCellName + "${05}";//按照仓位下达任务
            userCallCodePath[1] = endCellName + "${05}";//按照仓位下达任务

            var entity = new AgvTask(reqCode, taskTyp, podCode, userCallCodePath, boxCode,
            startCellName, endCellName, ctnrTyp, type, picklist, unique);

            var result = await _agvTaskRepository.InsertAsync(entity, true);

            if (dispatchToRcs)
            {
                await SetAsExecutingAsync(entity);
            }

            return result;
        }


        public async Task DeleteAsync(int agvTaskId)
        {
            var entity = await _agvTaskRepository.FindByIdAsync(agvTaskId);
            if (entity == null)
                throw new UserFriendlyException(message: "物料盒不存在");
            await _agvTaskRepository.DeleteAsync(entity);
        }
        public async Task<AgvTask> UpdateAsync(int id, string reqCode, string clientCode, string taskTyp,
            string wbCode, string podCode, string materialLot)
        {
            var entity = await _agvTaskRepository.FindByIdAsync(id);
            if (entity == null)
                throw new UserFriendlyException(message: "物料盒不存在");
            entity.Update(reqCode, clientCode, taskTyp, wbCode, podCode, materialLot);
            return await _agvTaskRepository.UpdateAsync(entity);
        }

        [UnitOfWork]
        public async Task<AgvTask> SetAsCompletedAsync(string reqcode)
        {
            try
            {
                var entity = await _agvTaskRepository.FindByReqCodeAsync(reqcode);
                if (entity == null)
                    throw new UserFriendlyException(message: "AGV任务不存在");
                // RCS可能因网络重试重复发送taskFinish。库存整理任务完成后再次收到回调时直接返回，
                // 避免把已经迁移到终点的同一容器再次从原起点解绑。
                if (entity.StockTyp == ManageType.StockConsolidation &&
                    entity.AgvTaskStatus == AgvTaskStatus.Complete)
                {
                    _logger.LogInformation($"库存整理任务{reqcode}已完成，忽略重复完成回调");
                    return entity;
                }
                //if (entity.AgvTaskStatus == AgvTaskStatus.Complete)
                //{
                //    _logger.Error(reqcode + "AgvTask任务重复完成");
                //    return entity;
                //}
                entity.SetAsCompleted();

                string endCode =entity.EndPositionCode; 
                Cell endCell = null;
                //设置库位状态
                if (entity.EndPositionCode == "300015A1501013" || entity.EndPositionCode == "300016A1501013" || 
                    entity.EndPositionCode == "300017A1501013" || entity.EndPositionCode == "300018A1501013" || entity.EndPositionCode == "300019A1501013")
                {
                    endCell = await _cellRepository.FindByCellCode2Async(entity.EndPositionCode);
                }
                else
                {
                    endCell = await _cellRepository.FindByCellCodeAsync(entity.EndPositionCode);
                    endCell.SetCellStatus(CellStatus.Have);
                }

                endCell.SetEnable();
                _logger.Info($"设置目标库位{endCell.CellCode}为Enable");
                await _cellRepository.UpdateAsync(endCell);

                string startCode = entity.StartPositionCode;
                Cell startCell = null;
                //设置库位状态
                if (entity.StartPositionCode == "300015A1501013" || entity.StartPositionCode == "300016A1501013" ||
                    entity.StartPositionCode == "300017A1501013" || entity.StartPositionCode == "300018A1501013" || entity.StartPositionCode == "300019A1501013")
                {
                    startCell = await _cellRepository.FindByCellCode2Async(entity.StartPositionCode);
                }
                else
                {
                    startCell = await _cellRepository.FindByCellCodeAsync(entity.StartPositionCode);
                    startCell.SetCellStatus(CellStatus.Nohave);
                }
                startCell.SetEnable();
                _logger.Info($"设置开始库位{startCell.CellCode}为Enable");
                await _cellRepository.UpdateAsync(startCell);


                Box box = null;
                Box endBox = null;
                if (!(entity.StockTyp == ManageType.SkipMove || entity.StockTyp == ManageType.SkipCall || entity.StockTyp == ManageType.SkipSend 
                    ))
                {
                    box = await _boxRepository.FindByBoxCodeAsync(entity.BoxCode);
                    if (box == null)
                        throw new UserFriendlyException(message: "料箱不存在");

                    if (entity.StockTyp == ManageType.StockConsolidation)
                    {
                        // 整理业务移动的是整个实体容器：原容器从起点解除关联后绑定终点，
                        // 容器中的库存仍属于原容器，只更新库存所在库位、仓库和库区信息。
                        endBox = await MoveContainerForStockConsolidationAsync(box, startCell, endCell);
                    }
                    else
                    {
                        var isSsxOut = entity.StockTyp == ManageType.CTUSSXOut || entity.StockTyp == ManageType.LiftSSXOut;
                        if (!isSsxOut)
                        {
                            // 原有入库、出库行为保持不变：起点/终点使用固定容器，仅迁移库存。
                            endBox = await MoveStockFromStartBoxToEndBoxAsync(box, startCell, endCell);
                        }
                    }
                }



                if (entity.StockTyp == ManageType.CTUStockIn)
                {

                    List<Stock> stocks = await _stockRepository.GetByBoxIdAsync(endBox.Id);
                    foreach (Stock stock in stocks)
                    {
                        if (stock.RunStatus == RunStatus.In)
                        {
                            ChkResultList chk = null;

                            List<ChkResultList> chkList = await _chkResultListRepository.FindByBarcodeAsync(stock.Barcode);
                            chk = chkList.FirstOrDefault(t => t.CheckData.CheckType == EnumCheckType.ReCheck);

                            if(chk == null)
                            {
                                if (stock.StockInType == StockInType.GoodsReturn)
                                {
                                    chk = await _chkResultListRepository.FindByBarcodeAndCheckTypeAsync(stock.Barcode, EnumCheckType.GoodsReturnCheck);
                                }
                                else
                                {
                                    chk = await _chkResultListRepository.FindByBarcodeAndCheckTypeAsync(stock.Barcode, EnumCheckType.StockInCheck);
                                }
                            }

                            if (chk != null)
                            {
                                stock.SetCheck(
                                            new CheckInfoOfStock(chk.CheckData.CheckOrderCode, chk.CheckData.CheckDate, chk.CheckData.CheckNo, chk.CheckData.CheckNoBeforeReCheck, chk.CheckData.CheckType, chk.CheckData.CheckResult, chk.CheckData.PassCnt));

                                if (chk.CheckData.CheckResult == EnumCheckResult.Pass) //检验结果是合格的，解冻结
                                    stock.ReturnToAvailable();
                                else if (chk.CheckData.CheckResult == EnumCheckResult.NoPass) //检验结果是不合格的，冻结
                                    stock.FreezeStock();
                                else if (chk.CheckData.CheckResult == EnumCheckResult.Filter) //检验结果是不合格的，冻结
                                    stock.SetStatus(StockStatus.Filtrate);
                            }
                            stock.StockInDate = DateTime.Now;

                            stock.SetRunStatus(RunStatus.Enable);
                            await _stockRepository.UpdateAsync(stock);

                            //创建入库记录，通知erp
                            Warehouse house = await _warehouseRepository.FindByIdAsync(endCell.WarehouseId).ConfigureAwait(false);
                            WarehouseArea area = house.GetAreaByAreaId((int)endCell.WarehouseAreaId);
                            Cell cell = endCell;
                            string operatorName = null;
                            if(stock.CreatorId != null)
                            {
                                var user = await _userManager.GetByIdAsync(stock.CreatorId.GetValueOrDefault());
                                //Console.WriteLine(JsonConvert.SerializeObject(user));
                                operatorName = user.Name;
                            }
                            //通知检验结论对象更新数据
                            await _localEventBus.PublishAsync(new StockBindBoxAndCellEvent()
                            {
                                StockBarcode = stock.Barcode,
                                BoxId = endBox.Id,
                                BoxCode = endBox.BoxCode,
                                BoxName = endBox.BoxName,
                                CellId = cell.Id,
                                CellCode = cell.CellCode,
                                CellName = cell.CellName,
                                AreaId = area.Id,
                                AreaCode = area.WarehouseAreaCode,
                                AreaName = area.WarehouseAreaName,
                                HouseId = house.Id,
                                HouseCode = house.WarehouseCode,
                                HouseName = house.WarehouseName,
                                StockCount = stock.TotalCountInTime,

                                MaterialCode = stock.Material.MaterialCode,
                                MaterialName = stock.Material.MaterialName,
                                Specs = stock.Material.Specs,
                                Unit = stock.Material.Unit,

                                SupplierCode = stock.Supplier.SupplierCode,
                                SupplierName = stock.Supplier.SupplierName,

                                StockInDate = stock.StockInDate,
                                StockInType = StockInTypeHelper.StockInTypeToChinese(stock.StockInType),
                                BatchCode = stock.BatchCode,
                                BLCode = stock.BLCode,
                                BHCode = stock.BHCode,
                                Operator = operatorName
                            });
                        }
                    }

                }
                else if (entity.StockTyp == ManageType.LiftStockIn)
                {
                    List<Stock> stocks = await _stockRepository.GetByBoxIdAsync(endBox.Id);
                    foreach (Stock stock in stocks)
                    {
                        if (stock.RunStatus == RunStatus.In)
                        {
                            ChkResultList chk = await _chkResultListRepository.FindByBarcodeAndCheckTypeAsync(stock.Barcode, EnumCheckType.StockInCheck);

                            if (chk != null && stock.Status==StockStatus.Waiting)
                            {
                                stock.SetCheck(
                                            new CheckInfoOfStock(chk.CheckData.CheckOrderCode, chk.CheckData.CheckDate, chk.CheckData.CheckNo, chk.CheckData.CheckNoBeforeReCheck, chk.CheckData.CheckType, chk.CheckData.CheckResult, chk.CheckData.PassCnt));

                                if (chk.CheckData.CheckResult == EnumCheckResult.Pass) //检验结果是合格的，解冻结
                                    stock.ReturnToAvailable();
                                else if (chk.CheckData.CheckResult == EnumCheckResult.NoPass) //检验结果是不合格的，冻结
                                    stock.FreezeStock();
                                else if (chk.CheckData.CheckResult == EnumCheckResult.Filter) //检验结果是不合格的，冻结
                                    stock.SetStatus(StockStatus.Filtrate);
                            }
                            stock.StockInDate = DateTime.Now;

                            stock.SetRunStatus(RunStatus.Enable);
                            await _stockRepository.UpdateAsync(stock);


                            //创建入库记录，通知erp
                            Warehouse house = await _warehouseRepository.FindByIdAsync(endCell.WarehouseId).ConfigureAwait(false);
                            WarehouseArea area = house.GetAreaByAreaId((int)endCell.WarehouseAreaId);
                            Cell cell = endCell;
                            string operatorName = null;
                            if (stock.CreatorId != null)
                            {
                                var user = await _userManager.GetByIdAsync(stock.CreatorId.GetValueOrDefault());
                                operatorName = user.Name;
                            }
                            //通知检验结论对象更新数据
                            await _localEventBus.PublishAsync(new StockBindBoxAndCellEvent()
                            {
                                StockBarcode = stock.Barcode,
                                BoxId = endBox.Id,
                                BoxCode = endBox.BoxCode,
                                BoxName = endBox.BoxName,
                                CellId = cell.Id,
                                CellCode = cell.CellCode,
                                CellName = cell.CellName,
                                AreaId = area.Id,
                                AreaCode = area.WarehouseAreaCode,
                                AreaName = area.WarehouseAreaName,
                                HouseId = house.Id,
                                HouseCode = house.WarehouseCode,
                                HouseName = house.WarehouseName,
                                StockCount = stock.TotalCountInTime,

                                MaterialCode = stock.Material.MaterialCode,
                                MaterialName = stock.Material.MaterialName,
                                Specs = stock.Material.Specs,
                                Unit = stock.Material.Unit,

                                SupplierCode = stock.Supplier.SupplierCode,
                                SupplierName = stock.Supplier.SupplierName,

                                StockInDate = stock.StockInDate,
                                StockInType = StockInTypeHelper.StockInTypeToChinese(stock.StockInType),
                                BatchCode = stock.BatchCode,
                                BLCode = stock.BLCode,
                                BHCode = stock.BHCode,
                                Operator = operatorName
                            });
                        }
                    }

                    Cell skipCell = await _cellRepository.FindByCellCode2Async(entity.StartPositionCode);
                    if(skipCell!= null)
                    {
                        Skip skip = await _skipRepository.FindByCellIdAsync(skipCell.Id);
                        if (skip != null && skip.SkipStatus != SkipStatus.NoHave)
                        {
                            skip.SkipStatus = SkipStatus.NoHave;
                            skip.SkipRunStatus = SkipRunStatus.Enable;
                            await _skipRepository.UpdateAsync(skip);
                        }
                    }

                }
                else if (entity.StockTyp == ManageType.CTUStockOut)
                {
                    List<Stock> stocks = await _stockRepository.GetByBoxIdAsync(endBox.Id);
                    if (endCell.CellType == CellType.SkipCell)
                    {
                        PickList pickList = await _pickListRepository.FindByPickListCodeAsync(entity.PickListCode);

                        foreach (Stock stock in stocks)
                        {
                            stock.SetRunStatus(RunStatus.Out);
                            await _stockRepository.UpdateAsync(stock);

                            Skip skip = await _skipRepository.FindBySkipCodeAsync(entity.PodCode);
                            if (skip.SkipStatus != SkipStatus.Have)
                            {
                                skip.SkipStatus = SkipStatus.Have;
                                await _skipRepository.UpdateAsync(skip);
                            }



                            //通知出库
                            //领料完成时，发出领料完成事件，库存相应修改
                            string operatorName = null;
                            if (entity.CreatorId != null)
                            {
                                var user = await _userManager.GetByIdAsync(entity.CreatorId.GetValueOrDefault());
                                operatorName = user.Name;
                            }


                            if (pickList != null)
                            {
                                Warehouse warehouse = await _warehouseRepository.FindByIdAsync(startCell.WarehouseId).ConfigureAwait(false);
                                WarehouseArea warehouseArea = warehouse.GetAreaByAreaId(startCell.WarehouseAreaId.GetValueOrDefault());
                                _pickListManager.StockPickOut(stock, pickList, entity.UniqueCode, operatorName, warehouseArea,stock.TotalCountInTime);
                            }
                        }

                    }
                    else if (endCell.CellType == CellType.WallCell)
                    {

                    }
                }
                else if (entity.StockTyp == ManageType.LiftStockOut)
                {
                    //if (endCell.CellType == CellType.Skip)
                    //{
                    //    var stocks = await _stockRepository.GetByBoxIdAsync(box.Id);

                    //    var pickList = await _pickListRepository.FindByPickListCodeAsync(box.PickListCode);
                    //    if (pickList == null)
                    //        throw new UserFriendlyException(message: $"领料单{box.PickListCode}不存在");

                    //    var pickItem = pickList.GetPickItemByUniqueCode(box.UniqueCode);
                    //    if (pickItem == null)
                    //        throw new UserFriendlyException(message: $"领料单{box.PickListCode}中不存在领料项{box.UniqueCode}");

                    //    if (stocks.Count == 1 && (stocks.FirstOrDefault().TotalCountInTime < pickItem.CountToPick - pickItem.PickedCount))
                    //    {
                    //        foreach (Stock stock in stocks)
                    //        {
                    //            stock.SetRunStatus(RunStatus.Out);
                    //            await _stockRepository.UpdateAsync(stock);

                    //            Skip skip = await _skipRepository.FindBySkipCodeAsync(entity.PodCode);
                    //            if (skip.SkipStatus != SkipStatus.Have)
                    //            {
                    //                skip.SkipStatus = SkipStatus.Have;
                    //                await _skipRepository.UpdateAsync(skip);
                    //            }
                    //        }
                    //    }
                    //}

                }
                else if (entity.StockTyp == ManageType.SkipMove || entity.StockTyp == ManageType.SkipCall || entity.StockTyp == ManageType.SkipSend)
                {
                    Skip skip = await _skipRepository.FindBySkipCodeAsync(entity.PodCode);
                    skip.CellId = endCell.Id;
                    skip.CellCode = endCell.CellCode;
                    skip.AreaId = (int)endCell.WarehouseAreaId;
                    await _skipRepository.UpdateAsync(skip);
                }
                else if(entity.StockTyp == ManageType.CTUSSXOut || entity.StockTyp == ManageType.LiftSSXOut)
                {
                    box = await _boxRepository.FindByBoxCodeAsync(entity.BoxCode);
                    if (box == null)
                        throw new UserFriendlyException(message: "料箱不存在");


                    var stocks = await _stockRepository.GetByBoxIdAsync(box.Id);

                    if(stocks.Count > 0)
                    {
                        var checks = await _barcodeCheckRepository.GetByBoxAsync(box.Id);

                        foreach (var check in checks)
                        {
                            await _barcodeCheckRepository.DeleteAsync(check);
                        }

                        foreach (Stock stock in stocks)
                        {
                            box.RemoveStock(stock.Id);
                            await _stockRepository.DeleteAsync(stock);
                        }

                        box.SetNoHave();
                        box.DisBindCell();
                        box.PickOutType = null;
                        await _boxRepository.UpdateAsync(box);
                    }
                }
                else if (entity.StockTyp == ManageType.CTUSSXIn)
                {
                    Skip skip = await _skipRepository.FindBySkipCodeAsync(entity.PodCode);
                    if (skip.SkipRunStatus != SkipRunStatus.In)
                    {
                        skip.SkipRunStatus = SkipRunStatus.In;
                        skip.SkipStatus = SkipStatus.Have;
                        await _skipRepository.UpdateAsync(skip).ConfigureAwait(false);
                    }
                }
                _logger.Info($"AGVTask:{reqcode} SetAsCompleted is end");
                await TryDispatchWaiting4ALaneTaskAsync(entity).ConfigureAwait(false);
                return await _agvTaskRepository.UpdateAsync(entity, true);
            }
            catch (Exception e)
            {

                throw new UserFriendlyException(message: e.Message);
            }
        }


        public async Task<AgvTask> SetAsCancelAsync(string reqcode)
        {
            try
            {
                var entity = await _agvTaskRepository.FindByReqCodeAsync(reqcode);
                if (entity == null)
                    throw new UserFriendlyException(message: "AGV任务不存在");
                if (entity.AgvTaskStatus == AgvTaskStatus.Complete || entity.AgvTaskStatus == AgvTaskStatus.Cancel)
                    throw new UserFriendlyException(message: "AGV任务已完成或取消");
                entity.SetAsCancel();


                //设置库位状态

                string endCode = entity.EndPositionCode;
                Cell endCell = null;
                //设置库位状态
                if (entity.EndPositionCode == "300015A1501013" || entity.EndPositionCode == "300016A1501013" ||
                    entity.EndPositionCode == "300017A1501013" || entity.EndPositionCode == "300018A1501013" || entity.EndPositionCode == "300019A1501013")
                {
                    endCell = await _cellRepository.FindByCellCode2Async(entity.EndPositionCode);
                }
                else
                {
                    endCell = await _cellRepository.FindByCellCodeAsync(entity.EndPositionCode);
                }
                endCell.SetEnable();
                _logger.Info($"设置目标库位{endCell.CellCode}为Enable");
                await _cellRepository.UpdateAsync(endCell);
                //var endCell = await _cellRepository.FindByCellCodeAsync(entity.EndPositionCode);
                //endCell.SetEnable();
                //_logger.Info($"设置目标库位{endCell.CellCode}为Enable");
                //await _cellRepository.UpdateAsync(endCell, true);

                var startCell = await _cellRepository.FindByCellCodeAsync(entity.StartPositionCode);
                startCell.SetEnable();
                _logger.Info($"设置开始库位{startCell.CellCode}为Enable");
                await _cellRepository.UpdateAsync(startCell, true);


                if (entity.StockTyp == ManageType.LiftStockOut)
                {
                    Skip skip = await _skipRepository.FindByCellIdAsync(endCell.Id);
                    if (skip!=null)
                    {
                        skip.SkipStatus = SkipStatus.NoHave;
                        skip.SkipRunStatus = SkipRunStatus.Enable;
                        skip.TargetLocation = null;
                        skip.TargetCellType = null;
                        await _skipRepository.UpdateAsync(skip);
                    }
                }

                return await _agvTaskRepository.UpdateAsync(entity, true);
            }
            catch (Exception e)
            {

                throw new UserFriendlyException(message: e.Message);
            }
        }

        [UnitOfWork]
        public async Task<bool> CreatePreAsync(string position,string nextTask,string agvTyp)
        {
            //设置AGV执行任务
            var response = await _rcsApiManager.CreateCTUPre(position,nextTask,agvTyp);
            if (response.Code != "0")
            {
                throw new UserFriendlyException(message: response.Message);
            }
            return true;
        }

        [UnitOfWork]
        public async Task<AgvTask> SetAsExecutingAsync(AgvTask entity)
        {
            try
            {
                entity.SetAsExecuting();
                
                // 记录任务执行参数
                _logger.LogInformation($"AGVTask SetAsExecuting: ReqCode={entity.ReqCode}, TaskTyp={entity.TaskTyp}, StockTyp={entity.StockTyp}, " +
                    $"StartPosition={entity.StartPositionCode}, EndPosition={entity.EndPositionCode}, " +
                    $"PodCode={entity.PodCode}, BoxCode={entity.BoxCode}, CtnrTyp={entity.CtnrTyp}");
                var startCell = await _cellRepository.FindByCellCodeAsync(entity.StartPositionCode).ConfigureAwait(false);
                var endCell = await _cellRepository.FindByCellCodeAsync(entity.EndPositionCode).ConfigureAwait(false);

                string[] userCallCodePath = new string[2];
                    userCallCodePath[0] = startCell.CellName;
                    userCallCodePath[1] = endCell.CellName;
                    //设置AGV执行任务
                    _logger.LogInformation($"创建StockTask任务: userCallCodePath={string.Join(',', userCallCodePath)}, CtnrCode=(empty, WMS BoxCode={entity.BoxCode})");
                    var response = await _rcsApiManager.CreateStockTaskAsync(entity.ReqCode, entity.TaskTyp, entity.CtnrTyp, userCallCodePath
                        , entity.ReqCode, "", "");
                    if (response.Code != "0")
                    {
                        _logger.LogError($"创建StockTask任务失败: {response.Message}");
                        throw new UserFriendlyException(message: response.Message);
                    }
                    _logger.LogInformation($"创建StockTask任务成功: {response.Message}");
                

                _logger.LogInformation($"AGVTask SetAsExecuting完成: ReqCode={entity.ReqCode}");
                return await _agvTaskRepository.UpdateAsync(entity);
            }
            catch (Exception e)
            {
                _logger.LogError($"AGVTask SetAsExecuting失败: ReqCode={entity?.ReqCode}, ErrorMsg={e.Message}");
                throw new UserFriendlyException(message: e.Message);
            }
        }

        [UnitOfWork]
        public async Task<AgvTask> SetAsExecutingSkipCallAsync(AgvTask entity)
        {
            try
            {
                List<Cell> cells = await _cellRepository.FindByAreaTypeAvailableAsync(12, CellType.Skip, "1");

                entity.SetAsExecuting();

                string[] userCallCodePath = new string[3];
                userCallCodePath[0] = "1";
                userCallCodePath[1] = entity.StartPositionCode;
                userCallCodePath[2] = entity.EndPositionCode;
                //设置AGV执行任务
                var response = await _rcsApiManager.CreateTaskAsync(entity.ReqCode, entity.TaskTyp, userCallCodePath
                    , entity.ReqCode, entity.PodCode);
                if (response.Code != "0")
                {
                    throw new UserFriendlyException(message: response.Message);
                }

                return await _agvTaskRepository.UpdateAsync(entity);
            }
            catch (Exception e)
            {
                //Log.Error($"AGVTask:{agvTaskId.ToString()} SetAsExecuting is fail ErrorMsg:{e.Message}。");
                throw new UserFriendlyException(message: e.Message);
            }
        }
        /// <summary>
        /// 任务设置为执行，
        /// </summary>
        /// <param name="agvTaskId"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        [UnitOfWork]
        public async Task<AgvTask> SetAsTaskStart(string rqecode)
        {
            try
            {
                var entity = await _agvTaskRepository.FindByReqCodeAsync(rqecode);
                if (entity == null)
                    throw new UserFriendlyException(message: "AGV任务不存在");
                entity.SetAsTaskStart();
                return await _agvTaskRepository.UpdateAsync(entity);
            }
            catch (Exception e)
            {

                throw new UserFriendlyException(message: e.Message);
            }
        }
        [UnitOfWork]
        public async Task<AgvTask> SetAsCellOut(string reqcode)
        {
            try
            {
                var entity = await _agvTaskRepository.FindByReqCodeAsync(reqcode);
                if (entity == null)
                    throw new UserFriendlyException(message: "AGV任务不存在");
                entity.SetAsCellOut();

                if (entity.StockTyp == ManageType.SkipMove || entity.StockTyp == ManageType.SkipCall || entity.StockTyp == ManageType.SkipSend)
                {
                    var startCell = await _cellRepository.FindByCellCodeAsync(entity.StartPositionCode);
                    startCell.SetEnable();
                    startCell.SetCellStatus(CellStatus.Nohave);
                    _logger.Info($"设置开始库位{startCell.CellCode}为Enable");
                    await _cellRepository.UpdateAsync(startCell);
                }

                await TryDispatchWaiting4ALaneTaskAsync(entity).ConfigureAwait(false);
                return await _agvTaskRepository.UpdateAsync(entity);
            }
            catch (Exception e)
            {

                throw new UserFriendlyException(message: e.Message);
            }
        }

        /// <summary>
        /// 任务完成时：起点/终点容器与库位绑定关系不变，起点容器设无货，仅迁移库存到终点容器。
        /// </summary>
        private async Task<Box> MoveStockFromStartBoxToEndBoxAsync(Box startBox, Cell startCell, Cell endCell)
        {
            var endBox = await _boxRepository.FindByCellIdAsync(endCell.Id);
            if (endBox == null)
                endBox = await _boxRepository.FindByBoxCodeAsync(endCell.CellCode);
            if (endBox == null)
                throw new UserFriendlyException(message: $"终点库位{endCell.CellCode}未找到容器");

            if (endBox.Id == startBox.Id)
                throw new UserFriendlyException(message: "起点容器与终点容器相同，无法迁移库存");

            var warehouse = await _warehouseRepository.FindByIdAsync(endCell.WarehouseId).ConfigureAwait(false);
            var warehouseArea = warehouse.GetAreaByAreaId((int)endCell.WarehouseAreaId);

            var stocks = await _stockRepository.GetByBoxIdAsync(startBox.Id);

            foreach (var stock in stocks)
            {
                startBox.RemoveStock(stock.Id);
                stock.BindBox(endBox.Id, endBox.BoxCode, endBox.BoxName);
                stock.BindCell(endCell, warehouse, warehouseArea);
                await _stockRepository.UpdateAsync(stock);
            }

            startBox.SetNoHave();
            await _boxRepository.UpdateAsync(startBox);

            if (startCell != null)
            {
                startCell.SetCellStatus(CellStatus.Nohave);
                startCell.SetEnable();
                await _cellRepository.UpdateAsync(startCell);
                _logger.Info($"起点库位{startCell.CellCode}设置为无货（容器绑定不变）");
            }

            if (stocks.Count > 0)
            {
                endBox.SetHave();
                await _boxRepository.UpdateAsync(endBox);
            }

            _logger.Info($"库存从起点容器{startBox.BoxCode}迁至终点容器{endBox.BoxCode}，共{stocks.Count}条");
            return endBox;
        }

        /// <summary>
        /// 完成库存整理任务时，将同一个实体容器从起点库位整体迁移到终点库位。
        /// 与普通入库、出库的“固定库位容器之间迁移库存”不同，本方法保证：
        /// 1. 起点库位移除原容器，终点库位新增的仍是同一个容器；
        /// 2. 库存的BoxId、BoxCode保持不变，只更新库位、仓库和库区；
        /// 3. 一个数据库工作单元内同时更新容器、库位和库存，失败时整体回滚；
        /// 4. 终点必须没有其他容器，防止实物与WMS绑定关系发生覆盖。
        /// </summary>
        private async Task<Box> MoveContainerForStockConsolidationAsync(
            Box movingBox,
            Cell startCell,
            Cell endCell)
        {
            if (movingBox == null)
                throw new UserFriendlyException(message: "库存整理任务未找到待搬运容器");
            if (startCell == null)
                throw new UserFriendlyException(message: "库存整理任务未找到起点库位");
            if (endCell == null)
                throw new UserFriendlyException(message: "库存整理任务未找到终点库位");

            // WMS以容器当前CellData为权威位置。若它已不是任务起点，说明整理期间发生了
            // 人工改绑或其他业务搬运，此时必须停止，不能根据过期任务继续覆盖位置。
            if (movingBox.CellData?.CellId != startCell.Id)
            {
                throw new UserFriendlyException(
                    message: $"容器{movingBox.BoxCode}当前不在任务起点{startCell.CellCode}，停止完成库存整理任务");
            }

            var startBindings = startCell.CellBoxes ?? new List<CellBox>();
            if (startBindings.Count != 1 || startBindings[0].BoxId != movingBox.Id)
            {
                throw new UserFriendlyException(
                    message: $"起点库位{startCell.CellCode}的容器绑定与任务容器{movingBox.BoxCode}不一致，停止完成库存整理任务");
            }

            var endBindings = endCell.CellBoxes ?? new List<CellBox>();
            if (endBindings.Count > 0)
            {
                var occupiedBoxes = string.Join("、", endBindings.Select(item => item.BoxCode));
                throw new UserFriendlyException(
                    message: $"终点库位{endCell.CellCode}已绑定容器{occupiedBoxes}，停止完成库存整理任务");
            }

            var warehouse = await _warehouseRepository
                .FindByIdAsync(endCell.WarehouseId)
                .ConfigureAwait(false);
            if (warehouse == null)
            {
                throw new UserFriendlyException(message: $"终点库位{endCell.CellCode}所属仓库不存在");
            }

            if (!endCell.WarehouseAreaId.HasValue)
            {
                throw new UserFriendlyException(message: $"终点库位{endCell.CellCode}未配置所属库区");
            }

            var warehouseArea = warehouse.GetAreaByAreaId(endCell.WarehouseAreaId.Value);
            if (warehouseArea == null)
            {
                throw new UserFriendlyException(message: $"终点库位{endCell.CellCode}所属库区不存在");
            }

            var stocks = await _stockRepository.GetByBoxIdAsync(movingBox.Id).ConfigureAwait(false);
            if (stocks == null || stocks.Count == 0)
            {
                throw new UserFriendlyException(
                    message: $"容器{movingBox.BoxCode}没有库存，停止完成库存整理任务");
            }

            // 先直接维护CellBox集合，使当前工作单元中的库位占用状态立即准确。
            // 随后调用Box.BindCell覆盖容器位置并产生绑定事件，事件处理器会再次做幂等检查，
            // 同步其他依赖容器绑定事件的既有业务。这里不先调用Box.DisBindCell，避免库存
            // 在同一完成事务中经历一次没有库位的中间状态。
            startCell.RemoveBox(movingBox.Id);
            endCell.AddBox(new CellBox(
                endCell.Id,
                movingBox.Id,
                movingBox.BoxCode,
                movingBox.BoxName,
                movingBox.BoxTypeName,
                movingBox.BoxSpecs?.SpecsName,
                movingBox.BoxSpecs?.Length,
                movingBox.BoxSpecs?.Width,
                movingBox.BoxSpecs?.Height));

            movingBox.BindCell(endCell, warehouse, warehouseArea);
            movingBox.SetHave();

            // 库存仍绑定movingBox，不调用Stock.BindBox；只把冗余的库位、仓库和库区信息
            // 更新成终点，确保“一个容器内多条库存”能够随容器整体到达同一库位。
            foreach (var stock in stocks)
            {
                stock.BindCell(endCell, warehouse, warehouseArea);
                await _stockRepository.UpdateAsync(stock).ConfigureAwait(false);
            }

            startCell.SetCellStatus(CellStatus.Nohave);
            startCell.SetEnable();
            endCell.SetCellStatus(CellStatus.Have);
            endCell.SetEnable();

            await _cellRepository.UpdateAsync(startCell).ConfigureAwait(false);
            await _cellRepository.UpdateAsync(endCell).ConfigureAwait(false);
            await _boxRepository.UpdateAsync(movingBox).ConfigureAwait(false);

            _logger.LogInformation(
                $"库存整理容器{movingBox.BoxCode}已从{startCell.CellCode}整体迁移到{endCell.CellCode}，容器内共{stocks.Count}条库存");
            return movingBox;
        }

        /// <summary>
        /// 4A巷道：当前任务出库/完成后，尝试下发后一位（LanePosition-1）处于Created状态的排队任务。
        /// 若因终点库位容器未解绑而失败，自动间隔重试（最多20次，每次间隔10秒）。
        /// </summary>
        private async Task TryDispatchWaiting4ALaneTaskAsync(AgvTask currentTask)
        {
            if (currentTask == null || string.IsNullOrWhiteSpace(currentTask.StartPositionCode))
                return;

            if (!currentTask.StartPositionCode.StartsWith("4A", StringComparison.OrdinalIgnoreCase))
                return;
            //var nextPos = (Cell)null;
            try
            {
                var startCell = await _cellRepository.FindByCellCodeAsync(currentTask.StartPositionCode).ConfigureAwait(false);
                if (startCell == null)
                {
                    _logger.Info($"4A巷道下发：起点库位{currentTask.StartPositionCode}不存在，跳过");
                    return;
                }

                if (string.IsNullOrWhiteSpace(startCell.LaneToColumn) || !startCell.LanePosition.HasValue)
                {
                    _logger.Info($"4A巷道下发：库位{startCell.CellCode}未配置LaneToColumn/LanePosition，跳过");
                    return;
                }

                var nextLanePosition = startCell.LanePosition.Value - 1;
                var nextCells = await _cellRepository.GetListAsync(c =>
                    c.LaneToColumn == startCell.LaneToColumn && c.LanePosition == nextLanePosition).ConfigureAwait(false);
                var nextCell = nextCells.FirstOrDefault();
                //nextPos = nextCell;
                if (nextCell == null)
                {
                    _logger.Info($"4A巷道下发：巷道列{startCell.LaneToColumn}不存在LanePosition={nextLanePosition}的库位，跳过");
                    return;
                }

                var waitingTasks = await _agvTaskRepository.GetListAsync(t =>
                    t.StartPositionCode == nextCell.CellCode && t.AgvTaskStatus == AgvTaskStatus.Created).ConfigureAwait(false);

                var waitingTask = waitingTasks
                    .OrderBy(t => t.CreationTime)
                    .ThenBy(t => t.Id)
                    .FirstOrDefault();

                if (waitingTask == null)
                {
                    _logger.Info($"4A巷道下发：库位{nextCell.CellCode}无Created状态待下发任务，跳过");
                    return;
                }

                _logger.Info($"4A巷道下发：库位{nextCell.CellCode}发现Created任务{waitingTask.ReqCode}，开始下发RCS");
                await SetAsExecutingAsync(waitingTask).ConfigureAwait(false);
                _logger.Info($"4A巷道下发：任务{waitingTask.ReqCode}下发成功，状态已更新为Executing");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"4A巷道排队任务下发失败，当前任务={currentTask.ReqCode}");
                //// 若为"已绑定容器"类错误，自动重试
                //if (ex.Message.Contains("已绑定容器"))
                //{
                //    _logger.LogInformation($"4A巷道下发：库位{nextPos.CellCode}任务：终点库位容器未解绑，60秒后重试...");
                //    _ = RetryDispatchWaiting4ALaneTaskAsync(nextPos.CellCode, 1);
                //}
            }
        }

        /// <summary>
        /// 4A巷道下发失败重试（最多20次，每次间隔10秒）
        /// </summary>
        private async Task RetryDispatchWaiting4ALaneTaskAsync(string startPositionCode, int retryCount)
        {
            if (retryCount > 3)
            {
                _logger.LogWarning($"4A巷道下发库位{startPositionCode}任务重试{retryCount}次均失败，放弃重试，startPositionCode={startPositionCode}");
                return;
            }

            await Task.Delay(60000).ConfigureAwait(false);

            try
            {
                var waitingTasks = await _agvTaskRepository.GetListAsync(t =>
                    t.StartPositionCode == startPositionCode && t.AgvTaskStatus == AgvTaskStatus.Created).ConfigureAwait(false);

                var waitingTask = waitingTasks
                    .OrderBy(t => t.CreationTime)
                    .ThenBy(t => t.Id)
                    .FirstOrDefault();

                if (waitingTask == null)
                {
                    _logger.Info($"4A巷道下发重试{retryCount}：库位{startPositionCode}已无Created任务，跳过");
                    return;
                }

                _logger.Info($"4A巷道下发重试{retryCount}：尝试下发任务{waitingTask.ReqCode}");
                await SetAsExecutingAsync(waitingTask).ConfigureAwait(false);
                _logger.Info($"4A巷道下发重试{retryCount}成功：任务{waitingTask.ReqCode}已下发RCS");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"4A巷道下发重试{retryCount}仍失败: {ex.Message}");
                if (ex.Message.Contains("已绑定容器"))
                {
                    _ = RetryDispatchWaiting4ALaneTaskAsync(startPositionCode, retryCount + 1);
                }
            }
        }

        /// <summary>
        /// 获取指定起点库位最近一条AGV任务
        /// </summary>
        public async Task<AgvTask> GetLatestTaskByStartPositionCodeAsync(string startPositionCode)
        {
            if (string.IsNullOrWhiteSpace(startPositionCode))
                return null;

            var tasks = await _agvTaskRepository.GetListAsync(t => t.StartPositionCode == startPositionCode).ConfigureAwait(false);
            return tasks
                .OrderByDescending(t => t.CreationTime)
                .ThenByDescending(t => t.Id)
                .FirstOrDefault();
        }

        /// <summary>
        /// 前序任务是否尚未出储位（Created=0, WaitingExecuting=1, Executing=2, TaskStart=3）
        /// </summary>
        public static bool IsPreviousTaskInWaitingQueue(AgvTask task)
        {
            if (task == null)
                return false;

            return task.AgvTaskStatus == AgvTaskStatus.Created
                || task.AgvTaskStatus == AgvTaskStatus.WaitingExecuting
                || task.AgvTaskStatus == AgvTaskStatus.Executing
                || task.AgvTaskStatus == AgvTaskStatus.TaskStart;
        }

        /// <summary>
        /// 是否存在重复任务
        /// </summary>
        /// <param name="boxCode"></param>
        /// <param name="taskTyp"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        public async Task<bool> IsExistBoxTask(string boxCode)
        {
            if (boxCode != null)
            {
                var agvTask = await _agvTaskRepository.GetListAsync(f => f.BoxCode == boxCode
                & (f.AgvTaskStatus != AgvTaskStatus.Complete & f.AgvTaskStatus != AgvTaskStatus.Cancel));
                if (agvTask.Count > 0)
                { return true; }
                else
                { return false; }
            }
            else
            {
                throw new UserFriendlyException(message: "料箱编码为空");
            }
        }

        /// <summary>
        /// 获取所有已有未完成任务（未完成且未取消）的容器编码
        /// </summary>
        public async Task<List<string>> GetBoxCodesWithUnfinishedTaskAsync()
        {
            var agvTasks = await _agvTaskRepository.GetListAsync(f =>
                f.AgvTaskStatus != AgvTaskStatus.Complete
                & f.AgvTaskStatus != AgvTaskStatus.Cancel).ConfigureAwait(false);
            return agvTasks.Select(t => t.BoxCode).Distinct().ToList();
        }



        /// <summary>
        /// 是否存在重复任务
        /// </summary>
        /// <param name="boxCode"></param>
        /// <param name="taskTyp"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        public async Task<bool> IsExistSkipTask(string skipCode)
        {
            if (skipCode != null)
            {
                var agvTask = await _agvTaskRepository.GetListAsync(f => f.PodCode == skipCode
                & (f.AgvTaskStatus != AgvTaskStatus.Complete & f.AgvTaskStatus != AgvTaskStatus.Cancel) && f.TaskTyp != "B10");
                if (agvTask.Count > 0)
                { return true; }
                else
                { return false; }
            }
            else
            {
                throw new UserFriendlyException(message: "料箱编码为空");
            }
        }

        public async Task<bool> BindCtnrAndBinAsync(string stgBinCode, string ctnrTyp, string ctnrCode, string indBind)
        {
            try
            {
                var response = await _rcsApiManager.BindCtnrAndBinAsync(null, stgBinCode, ctnrTyp, ctnrCode, indBind);
                if (response.Code == "1")
                {
                    throw new UserFriendlyException(message: response.Message);
                }
                else
                {
                    return true;
                    //response.
                }

            }
            catch (Exception e)
            {
                throw new UserFriendlyException(message: e.Message);
            }
        }
        public async Task<bool> BindPodAndBerthAsync(string stgBinCode, string ctnrCode, string indBind, string podDir)
        {
            try
            {
                var response = await _rcsApiManager.BindPodAndBerthAsync(null, stgBinCode, ctnrCode, indBind, podDir);
                if (response.Code == "1")
                {
                    throw new UserFriendlyException(message: response.Message);
                }
                else
                {
                    return true;
                }

            }
            catch (Exception e)
            {
                throw new UserFriendlyException(message: e.Message);
            }
        }
    }
}
