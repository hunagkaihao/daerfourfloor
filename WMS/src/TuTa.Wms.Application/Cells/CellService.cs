using Abp.Events.Bus.Entities;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.BarcodeLists;
using TuTa.Wms.BarcodeLists.Aggregates;
using TuTa.Wms.Boxes;
using TuTa.Wms.Boxes.Aggregates;
using TuTa.Wms.Cells.Aggregates;
using TuTa.Wms.Cells.Dtos;
using TuTa.Wms.ChkResultLists;
using TuTa.Wms.ChkResultLists.Aggregates;
using TuTa.Wms.Erp;
using TuTa.Wms.Erp.Entities;
using TuTa.Wms.Materials;
using TuTa.Wms.Materials.Aggregates;
using TuTa.Wms.PickLists;
using TuTa.Wms.Skips;
using TuTa.Wms.Skips.Aggregates;
using TuTa.Wms.Skips.Dtos;
using TuTa.Wms.Stocks;
using TuTa.Wms.Stocks.Aggregates;
using TuTa.Wms.Stocks.Dtos;
using TuTa.Wms.Stocks.EventHandlers;
using TuTa.Wms.Warehouses;
using TuTa.Wms.Warehouses.Aggregates;
using TuTa.Wms.Warehouses.Entities;

using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Local;
using Wms.LogTool;

namespace TuTa.Wms.Cells
{
    public class CellService : WmsAppService, ICellService
    {
        private readonly ICellRepository _cellRepository;
        private readonly CellManager _cellManager;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IStockRepository _stockRepository;
        private readonly IBoxRepository _boxRepository;
        private readonly IBarcodeListRepository _barcodeListRepository;
        private readonly ISkipRepository _skipRepository;
        private readonly IPickListRepository _pickListRepository;
        private readonly IChkResultListRepository _chkResultListRepository;
        private readonly IErpDeptTypeDetailRepository _erpDeptTypeDetailRepository;
        private readonly IErpDeptTypeRepository _erpDeptTypeRepository;
        private readonly IErpWarehouseAreaPrdtRepository _erpWarehouseAreaPrdtRepository;
        private readonly IMaterialRepository _materialRepository;
        private readonly ILocalEventBus _localEventBus;
        private readonly ILogger<CellService> _logger;

        public CellService(
            ICellRepository cellRepository, 
            CellManager cellManager,
            IWarehouseRepository warehouseRepository,
            IStockRepository stockRepository,
            IBoxRepository boxRepository,
            IBarcodeListRepository barcodeListRepository,
            ISkipRepository skipRepository,
            IPickListRepository pickListRepository,
            IChkResultListRepository chkResultListRepository,
            IErpDeptTypeDetailRepository erpDeptTypeDetailRepository,
            IErpDeptTypeRepository erpDeptTypeRepository,
            IErpWarehouseAreaPrdtRepository erpWarehouseAreaPrdtRepository,
            IMaterialRepository materialRepository,
            ILocalEventBus localEventBus,
            ILogger<CellService> logger)
        {
            _cellRepository = cellRepository;
            _cellManager = cellManager;
            _warehouseRepository = warehouseRepository;
            _stockRepository = stockRepository;
            _boxRepository = boxRepository;
            _skipRepository = skipRepository;
            _barcodeListRepository = barcodeListRepository;
            _pickListRepository = pickListRepository;
            _chkResultListRepository = chkResultListRepository;
            _erpDeptTypeDetailRepository = erpDeptTypeDetailRepository;
            _erpDeptTypeRepository = erpDeptTypeRepository;
            _erpWarehouseAreaPrdtRepository = erpWarehouseAreaPrdtRepository;
            _materialRepository = materialRepository;
            _localEventBus = localEventBus;
            _logger = logger;
        }

        public async Task<ResponseDto> AddCellAsync(CellAddDto para)
        {
            try
            {
                var warehouseExist = await _warehouseRepository.FindByNameAsync(para.WarehouseName).ConfigureAwait(false);
                if (warehouseExist == null)
                    return new ResponseDto() { success = false, message = $"仓库名为{para.WarehouseName}的仓库不存在" };

                int? warehouseAreaId = null;
                if (para.WarehouseAreaName != null)
                {
                    var area = warehouseExist.GetAreaByAreaName(para.WarehouseAreaName);
                    if (area == null)
                        return new ResponseDto() { success = false, message = $"库区名为{para.WarehouseAreaName}的库区不存在" };
                    warehouseAreaId = area.Id;
                }

                Cell cell = await _cellManager.CreateCellAsync( 
                    warehouseExist.Id,
                    warehouseAreaId,
                    para.ShelfName,
                    para.CellCode,
                    para.CellName,
                    para.CellType,
                    para.AvailableBoxSpecsNames,
                    para.AvailableSkipSpecsNames);
                cell.LaneToColumn = para.LaneToColumn;
                cell.LanePosition = para.LanePosition;

                await _cellRepository.InsertAsync(cell).ConfigureAwait(false);
                return new ResponseDto() { success = true, message = "创建库位成功" };

            }
            catch(Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<ResponseDto> AddCellsAsync(List<CellAddDto> paras)
        {
            try
            {
                if (paras == null || paras.Count == 0)
                    throw new ArgumentNullException(nameof(paras));

                foreach (CellAddDto para in paras)
                {
                    var warehouseExist = await _warehouseRepository.FindByNameAsync(para.WarehouseName).ConfigureAwait(false);
                    if (warehouseExist == null)
                        return new ResponseDto() { success = false, message = $"仓库名为{para.WarehouseName}的仓库不存在" };

                    int? warehouseAreaId = null;
                    if (para.WarehouseAreaName != null)
                    {
                        var area = warehouseExist.GetAreaByAreaName(para.WarehouseAreaName);
                        if (area == null)
                            return new ResponseDto() { success = false, message = $"库区名为{para.WarehouseAreaName}的库区不存在" };
                        warehouseAreaId = area.Id;
                    }

                    Cell cell = await _cellManager.CreateCellAsync(
                        warehouseExist.Id,
                        warehouseAreaId,
                        para.ShelfName,
                        para.CellCode,
                        para.CellName,
                        para.CellType,
                        para.AvailableBoxSpecsNames,
                        para.AvailableSkipSpecsNames);
                    cell.LaneToColumn = para.LaneToColumn;
                    cell.LanePosition = para.LanePosition;

                    await _cellRepository.InsertAsync(cell).ConfigureAwait(false);
                }
                
                return new ResponseDto() { success = true, message = "创建库位成功" };

            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<ResponseDto> DelCellAsync(Guid cellId)
        {
            try
            {
                var cellExist = await _cellRepository.FindByIdAsync(cellId).ConfigureAwait(false);
                if (cellExist == null)
                    return new ResponseDto() { success = true, message = $"Id为{cellId}的库位不存在，默认删除成功" };

                if (cellExist.CellBoxes.Count > 0)
                    throw new Exception($"Id为{cellId}的仓位已经绑定容器，无法删除");

                await _cellRepository.DeleteAsync(cellExist).ConfigureAwait(false);
                return new ResponseDto() { success = true, message = "删除成功" };               

            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<ResponseDto> CellsBindToAreaAsync(CellsBindAreaDto para)
        {
            try
            {
                if (para.CellIds == null || para.CellIds.Count == 0)
                    return new ResponseDto() { success = false, message = $"未指定待绑定的库位" };

                var warehouse = await _warehouseRepository.FindByIdAsync(para.WarehouseId).ConfigureAwait(false);
                if (warehouse == null)
                    return new ResponseDto() { success = false, message = $"Id为{para.WarehouseId}的仓库不存在" };

                var area = warehouse.GetAreaByAreaId(para.WarehouseAreaId);
                if (area == null)
                    return new ResponseDto() { success = false, message = $"Id为{para.WarehouseAreaId}的库区不存在" };

                List<Cell> cells = new List<Cell>();
                foreach (var cellId in para.CellIds)
                {
                    var cell = await _cellRepository.FindAsync(cellId).ConfigureAwait(false);

                    if (cell == null)
                        return new ResponseDto() { success = false, message = $"Id为{cellId}的待绑定库位不存在" };

                    if (cell.WarehouseId != warehouse.Id)
                        return new ResponseDto() { success = false, message =  $"Id为{cellId}的待绑定库位不在仓库{para.WarehouseId}中" };

                    cells.Add(cell);
                }

                foreach (var cell in cells)
                {
                    if (cell.WarehouseAreaId != null)
                        throw new Exception($"Id为{cell.Id}的库位已经绑定库区，请勿重复绑定");

                    cell.BindToWarehouseArea(area.Id);
                    await _cellRepository.UpdateAsync(cell).ConfigureAwait(false);
                }

                return new ResponseDto() { success = true, message = "绑定库区成功" };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<ResponseDto> CellsDisBindToAreaAsync(CellsDisBindFromAreaDto para)
        {
            try
            {
                if (para.CellIds == null || para.CellIds.Count == 0)
                    return new ResponseDto() { success = false, message = $"未指定待解绑的库位" };

                List<Cell> cells = new List<Cell>();
                foreach (var cellId in para.CellIds)
                {
                    var cell = await _cellRepository.FindAsync(cellId).ConfigureAwait(false);

                    if (cell == null)
                        return new ResponseDto() { success = false, message = $"Id为{cellId}的待解绑库位不存在" };

                    cells.Add(cell);
                }

                foreach (var cell in cells)
                {
                    cell.DisBindFromWarehouseArea();
                    await _cellRepository.UpdateAsync(cell).ConfigureAwait(false);
                }

                return new ResponseDto() { success = true, message = "绑定库区成功" };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<PagedResultDto<CellDto>> GetPagedCellsAsync(PagedCellsQueryDto para)
        {
            try
            {
                var warehouses = await _warehouseRepository.GetAllWarehousesAsync().ConfigureAwait(false);
                var house = warehouses.FirstOrDefault(o => o.WarehouseName == para.WarehouseName);
                Guid? houseId = null;
                if(para.WarehouseName != null)
                {
                    if (house == null)
                        houseId = Guid.Empty;
                    else
                        houseId = house.Id;
                }
                int? areaId = null;
                if (house == null)
                {
                    if (null != para.WarehouseAreaName)
                    {
                        foreach(var h in warehouses) //从所有的仓库中寻找指定名称的库区
                        {
                            if(h.GetAreaByAreaName(para.WarehouseAreaName) != null)
                            {
                                areaId = h.GetAreaByAreaName(para.WarehouseAreaName).Id;
                                break;
                            }
                        }
                        if (areaId == null)
                            areaId = -1;
                    }
                }
                else
                {
                    if (null != para.WarehouseAreaName)
                    {
                        var area = house.GetAreaByAreaName(para.WarehouseAreaName);
                        if (area == null)
                            areaId = -1;
                        else
                            areaId = area.Id;
                    }
                }

                CellStatus? cellStatus = null;
                if (para.CellStatus != null)
                {
                    if (!Enum.IsDefined(typeof(CellStatus), para.CellStatus))
                        return new PagedResultDto<CellDto>() { TotalCount = 0, Items = new List<CellDto>() };
                    if (!Enum.TryParse<CellStatus>(para.CellStatus, out var status)) 
                        return new PagedResultDto<CellDto>() { TotalCount = 0, Items = new List<CellDto>() };
                    cellStatus = status;
                }

                CellRunStatus? cellRunStatus = null;
                if(para.RunStatus != null)
                {
                    if (!Enum.IsDefined(typeof(CellRunStatus), para.RunStatus))
                        return new PagedResultDto<CellDto>() { TotalCount = 0, Items = new List<CellDto>() };

                    if (!Enum.TryParse<CellRunStatus>(para.RunStatus, out var runStatus))
                        return new PagedResultDto<CellDto>() { TotalCount = 0, Items = new List<CellDto>() };

                    cellRunStatus = runStatus;
                }

                CellType? cellType = null;
                if (para.CellType != null)
                {
                    if (!Enum.IsDefined(typeof(CellType), para.CellType))
                        return new PagedResultDto<CellDto>() { TotalCount = 0, Items = new List<CellDto>() };

                    if (!Enum.TryParse<CellType>(para.CellType, out CellType type))
                        return new PagedResultDto<CellDto>() { TotalCount = 0, Items = new List<CellDto>() };

                    cellType = type;
                }

                var cells = await _cellRepository.GetPagedCellsAsync(
                    houseId,
                    areaId,
                    para.ShelfName,
                    cellStatus,
                    cellRunStatus,
                    cellType,
                    para.AvailableBoxSpecsNamesTip,
                    para.CellCodeTip,
                    para.CellNameTip,
                    false,
                    para.SkipCount,
                    para.MaxResultCount);

                PagedResultDto<CellDto> result = new PagedResultDto<CellDto>()
                {
                    TotalCount = cells.TotalCount
                };

                List<CellDto> items = new List<CellDto>();
                foreach(var item in cells.Items)
                {
                    CellDto cellDto = new CellDto();
                    cellDto.Id = item.Id;
                    var whouse = warehouses.FirstOrDefault(o => o.Id == item.WarehouseId);
                    var warea = whouse?.GetAreaByAreaId(item.WarehouseAreaId ?? -1);
                    cellDto.WarehouseName = whouse?.WarehouseName;
                    cellDto.WarehouseAreaName = warea?.WarehouseAreaName;
                    cellDto.CellCode = item.CellCode;
                    cellDto.CellName = item.CellName;
                    cellDto.ShelfName = item.ShelfName;
                    cellDto.CellStatus = item.CellStatus.ToString();
                    cellDto.RunStatus = item.RunStatus.ToString();
                    cellDto.CellType = item.CellType.ToString();
                    cellDto.AvailableBoxSpecsNames = item.AvailableBoxSpecsNames;
                    cellDto.LaneToColumn = item.LaneToColumn;
                    cellDto.LanePosition = item.LanePosition;
                    items.Add(cellDto);
                }

                result.Items = items;
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<PagedResultDto<CellDto>> GetPagedCellsByAreaAsync(PagedCellsAreaDto para)
        {
            try
            {
                string heigh = "small";
                string weigh = "small";
                if (para.isHeigh) heigh = "heigh";
                if (para.isWeight) weigh = "big";

                CellType cellType;

                if(!Enum.TryParse<CellType>(para.cellType,out cellType))
                    throw new Exception($"点位类型{para.cellType}无法识别");

                var cells = await _cellRepository.GetPagedCellsByAreaAsync(
                    para.areaId,
                    heigh,
                    weigh,
                    cellType,
                    para.cellCode,
                    false,
                    para.SkipCount,
                    para.MaxResultCount);

                PagedResultDto<CellDto> result = new PagedResultDto<CellDto>()
                {
                    TotalCount = cells.TotalCount
                };

                List<Warehouse> warehouse = await _warehouseRepository.GetAllWarehousesAsync();
                if (warehouse == null || warehouse.Count == 0)
                    throw new Exception($"获取仓库失败");
                var warea = warehouse[0].GetAreaByAreaId(para.areaId);
                if (warea == null)
                    throw new Exception($"获取库区失败");

                List<CellDto> items = new List<CellDto>();
                foreach (var item in cells.Items)
                {
                    CellDto cellDto = new CellDto();
                    cellDto.Id = item.Id;
                    cellDto.WarehouseName = warehouse[0].WarehouseName;
                    cellDto.WarehouseAreaName = warea?.WarehouseAreaName;
                    cellDto.CellCode = item.CellCode;
                    cellDto.CellName = item.CellName;
                    cellDto.ShelfName = item.ShelfName;
                    cellDto.CellStatus = item.CellStatus.ToString();
                    cellDto.RunStatus = item.RunStatus.ToString();
                    cellDto.CellType = item.CellType.ToString();
                    cellDto.AvailableBoxSpecsNames = item.AvailableBoxSpecsNames;
                    cellDto.LaneToColumn = item.LaneToColumn;
                    cellDto.LanePosition = item.LanePosition;
                    items.Add(cellDto);
                }

                result.Items = items;
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }


        public async Task<CellDto> GetCellByStock(string barcode,string boxCode)
        {
            try
            {
                int area = 1;
                string height = "small";
                string weight = "small";
                string ava = null;

                Box box = await _boxRepository.FindByBoxCodeAsync(boxCode);
                if(box == null)
                    throw new Exception($"获取料箱{boxCode}失败");


                if (barcode.IsNullOrEmpty() && box.Status != BoxStatus.NoHave)
                {
                    Material material = null;
                    List<Stock> stocks = await _stockRepository.GetByBoxIdAsync(box.Id);
                    decimal proportion = 0;
                    Stock mainstock = null;
                    foreach (Stock stock in stocks)
                    {
                        material = await _materialRepository.FindByMaterialCodeAsync(stock.Material.MaterialCode);

                        if(material.FullBoxCount == null)
                            throw new Exception($"该物料没有满箱数据");

                        decimal nowproportion = stock.TotalCountInTime / material.FullBoxCount.GetValueOrDefault();
                        if(nowproportion > proportion)
                        {
                            mainstock = stock;
                        }
                    }

                    //if (mainstock.Status == StockStatus.Waiting && mainstock.RunStatus == RunStatus.In)
                    //{
                    //}

                    BarcodeList barcodeList = await _barcodeListRepository.FindByBarcodeAsync(mainstock.Barcode);
                    if (barcodeList.Warehouse.TargetWarehouseCode == "01")
                    {
                        area = 1;
                    }
                    else if (barcodeList.Warehouse.TargetWarehouseCode == "26")
                    {
                        area = 3;
                    }
                    else
                    {
                        area = 2;
                    }
                    if (mainstock.CheckData != null && mainstock.CheckData.CheckResult != null && mainstock.CheckData.CheckResult != EnumCheckResult.Pass)
                    {
                        area = 2;
                    }
                    if (mainstock.Status == StockStatus.Freezing)
                    {
                        area = 2;
                    }
                    material = await _materialRepository.FindByMaterialCodeAsync(mainstock.Material.MaterialCode);
                    if (material == null)
                        throw new Exception($"获取物料{mainstock.Material.MaterialCode}失败");

                    try
                    {
                        if (box.BoxTypeName == "1")
                        {
                            ava = await _erpWarehouseAreaPrdtRepository.GetAreaByPrdtName(material.BindType, "B020401");
                        }
                        else
                        {
                            ava = await _erpWarehouseAreaPrdtRepository.GetAreaByPrdtName(material.BindType, "B020501");
                        }
                    }
                    catch (Exception)
                    {
                        throw new Exception($"获取ERP物料区域{material.BindType}失败");
                    }
                }
                else if (barcode.IsNullOrEmpty() && box.Status == BoxStatus.NoHave)
                {

                }
                else
                {
                    BarcodeList barcodeList = await _barcodeListRepository.FindByBarcodeAsync(barcode);
                    if (barcodeList == null)
                        throw new Exception($"获取物料{barcode}失败");

                    if (barcodeList.Warehouse.TargetWarehouseCode == "01")
                    {
                        area = 1;
                    }
                    else if (barcodeList.Warehouse.TargetWarehouseCode == "26")
                    {
                        area = 3;
                    }
                    else
                    {
                        area = 2;
                    }
                }

                if(area == 3)
                {
                    if (box.PickOutAreaId == "1")
                        area = 1;
                }


                //if (box.Height != 0 && box.Height == (decimal)1.8) height = "heigh";
                //if (box.Weight != 0 && box.Weight > 500) weight = "big";

                CellType cellType;
                if(box.BoxTypeName == "1")
                {
                    cellType = CellType.CTUCell;
                    if (area == 2) area = 1;
                }
                else
                {
                    cellType = CellType.Cell;
                }

                Cell cell = null;
                var cellList = await _cellRepository.GetListAsync(t => t.WarehouseAreaId == area && t.CellHeight == height && t.CellWeight == weight && t.CellType==cellType
                && t.RunStatus == CellRunStatus.Enable && t.CellStatus == CellStatus.Nohave && t.AvailableBoxSpecsNames == ava);
                if (cellType == CellType.Cell)
                    cell = cellList.OrderBy(t => t.CellCode).FirstOrDefault();
                else
                    cell = cellList.OrderByDescending(t => t.CellCode).FirstOrDefault();

                if (cell == null && ava != "B020501-2")
                {
                    Console.WriteLine("绑定区域无位置，查询综合区");
                    cellList = await _cellRepository.GetListAsync(t => t.WarehouseAreaId == area && t.CellHeight == height && t.CellWeight == weight && t.CellType == cellType
                    && t.RunStatus == CellRunStatus.Enable && t.CellStatus == CellStatus.Nohave && t.AvailableBoxSpecsNames == null);
                    if (cellType == CellType.Cell)
                        cell = cellList.OrderBy(t => t.CellCode).FirstOrDefault();
                    else
                        cell = cellList.OrderByDescending(t => t.CellCode).FirstOrDefault();
                }

                if (cell == null)
                    throw new Exception($"没有能分配的位置");

                Warehouse warehouse = await _warehouseRepository.FindByIdAsync(cell.WarehouseId);
                if (warehouse == null)
                    throw new Exception($"库位未绑定仓库");
                var warea = warehouse.GetAreaByAreaId(area);
                if (warea == null)
                    throw new Exception($"库位未绑定库区");

                CellDto cellDto = new CellDto();
                cellDto.Id = cell.Id;
                cellDto.WarehouseName = warehouse.WarehouseName;
                cellDto.WarehouseAreaName = warea.WarehouseAreaName;
                cellDto.CellCode = cell.CellCode;
                cellDto.CellName = cell.CellName;
                cellDto.ShelfName = cell.ShelfName;
                cellDto.CellStatus = cell.CellStatus.ToString();
                cellDto.RunStatus = cell.RunStatus.ToString();
                cellDto.CellType = cell.CellType.ToString();
                cellDto.AvailableBoxSpecsNames = cell.AvailableBoxSpecsNames;
                cellDto.LaneToColumn = cell.LaneToColumn;
                cellDto.LanePosition = cell.LanePosition;
                cellDto.isHeigh = height == "small" ? "0" : "1";
                cellDto.isWeight = weight == "small" ? "0" : "1";


                return cellDto;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<CellDto> GetCellByWorkShop(string skipCode, int areaId)
        {
            try
            {
                Skip skip = await _skipRepository.FindBySkipCodeAsync(skipCode);
                if (skip == null)
                    throw new Exception($"获取料车{skipCode}失败");


                List<Warehouse> warehouse = await _warehouseRepository.GetAllWarehousesAsync();
                if (warehouse == null || warehouse.Count == 0)
                    throw new Exception($"获取仓库失败");
                var warea = warehouse[0].GetAreaByAreaId(areaId);
                if (warea == null)
                    throw new Exception($"获取库区失败");

                if (warea.WarehouseAreaGroup != "车间" && warea.WarehouseAreaName != "周转区" && warea.WarehouseAreaName !="入库区") 
                    throw new Exception($"库区类型错误，不是车间库区或周转区");

                List<Cell> cells = await _cellRepository.FindByWorkSendAsync(areaId,skip.TargetCellType);
                Console.WriteLine(JsonConvert.SerializeObject(cells));

                Cell cell = null;
                if (areaId != 4)
                {
                    if(skip.TargetCellType == null)
                    {
                        cell = cells.Where(t => (t.AvailableSkipSpecsNames != null && int.Parse(t.AvailableSkipSpecsNames) >= skip.Type))
                            .OrderBy(t => t.AvailableSkipSpecsNames).FirstOrDefault();
                    }
                    else
                    {
                        cell = cells.Where(t => t.AvailableSkipSpecsNames == skip.Type.ToString()).FirstOrDefault();
                    }
                }
                else
                {
                    cell = cells.Where(t => t.AvailableSkipSpecsNames == skip.Type.ToString()).FirstOrDefault();
                }


                Console.WriteLine(JsonConvert.SerializeObject(cell));

                if (cell == null)
                    throw new Exception($"没有剩余可分配库位");

                CellDto cellDto = new CellDto();
                cellDto.Id = cell.Id;
                cellDto.WarehouseName = warehouse[0].WarehouseName;
                cellDto.WarehouseAreaName = warea.WarehouseAreaName;
                cellDto.CellCode = cell.CellCode;
                cellDto.CellName = cell.CellName;
                cellDto.ShelfName = cell.ShelfName;
                cellDto.CellStatus = cell.CellStatus.ToString();
                cellDto.RunStatus = cell.RunStatus.ToString();
                cellDto.CellType = cell.CellType.ToString();
                cellDto.AvailableBoxSpecsNames = cell.AvailableBoxSpecsNames;
                cellDto.LaneToColumn = cell.LaneToColumn;
                cellDto.LanePosition = cell.LanePosition;


                return cellDto;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<ResultGetBySkipDto> GetCellsBySkip(string skipCode)
        {
            try
            {
                int area = 1;
                //string height = "small";
                //string weight = "small";



                var skip = await _skipRepository.FindBySkipCodeAsync(skipCode).ConfigureAwait(false);

                if (skip == null)
                    throw new Exception($"读取料车信息失败");

                var skipCell = await _cellRepository.FindByIdAsync(skip.CellId.GetValueOrDefault()).ConfigureAwait(false);
                if (skipCell == null)
                    throw new Exception($"读取料车库位失败，该料车未绑定库位");

                if (skipCell.WarehouseAreaId != 4)
                    throw new Exception($"起始料车不在周转区，无法入库");

                if (skip.SkipRunStatus != SkipRunStatus.In)
                    throw new Exception($"起始料车不是入库料车，无法整车入库");

                if (skip.Type != 1 && skip.Type != 2)
                    throw new Exception($"料车不是运送料箱料车");


                List<Cell> cells = await _cellRepository.FindBySkipCellAsync(skipCode);
                if (cells.Count == 0)
                    throw new Exception($"查询料车库位失败");

                List<Box> boxs = await _boxRepository.GetByCellsIdAsync(cells.Select(t=>t.Id).ToList()).ConfigureAwait(false);


                ResultGetBySkipDto dto = new ResultGetBySkipDto();
                List<GetBySkipDto> getBySkipDtos = new List<GetBySkipDto>();
                List<StockDto> stockDtos = new List<StockDto>();


                string ava = null;
                List<string> useCells = new List<string>();

                foreach (Box box in boxs)
                {
                    Material material = null;
                    List<Stock> stocks = await _stockRepository.GetByBoxIdAsync(box.Id);
                    decimal proportion = 0;
                    Stock mainstock = null;
                    foreach (Stock stock in stocks)
                    {
                        material = await _materialRepository.FindByMaterialCodeAsync(stock.Material.MaterialCode);

                        if (material.FullBoxCount == null)
                            throw new Exception($"该物料没有满箱数据");

                        decimal nowproportion = stock.TotalCountInTime / material.FullBoxCount.GetValueOrDefault();
                        if (nowproportion > proportion)
                        {
                            mainstock = stock;
                        }

                        StockDto stockDto = new StockDto()
                        {
                            Id = stock.Id,
                            Barcode = stock.Barcode,
                            BoxId = stock.BoxData.BoxId,
                            BoxCode = stock.BoxData.BoxCode,
                            BoxName = stock.BoxData.BoxName,
                            CellId = stock.CellData.CellId,
                            CellCode = stock.CellData.CellCode,
                            CellName = stock.CellData.CellName,
                            HouseId = stock.Warehouse.HouseId,
                            HouseCode = stock.Warehouse.HouseCode,
                            HouseName = stock.Warehouse.HouseName,
                            AreaId = stock.Warehouse.AreaId,
                            AreaCode = stock.Warehouse.AreaCode,
                            AreaName = stock.Warehouse.AreaName,
                            TotalCountInTime = stock.TotalCountInTime,
                            Status = stock.Status.ToString(),
                            StockInType = StockInTypeHelper.StockInTypeToChinese(stock.StockInType),
                            BatchCode = stock.BatchCode,
                            BLCode = stock.BLCode,
                            BHCode = stock.BHCode,
                            StockInDate = stock.StockInDate.ToString("yyyy-MM-dd"),
                            MaterialCode = stock.Material.MaterialCode,
                            MaterialName = stock.Material.MaterialName,
                            Specs = stock.Material.Specs,
                            Unit = stock.Material.Unit,
                            ReceiveTotalCount = stock.ReceiveCount.ReceiveTotalCount,
                            ReceivePkgOrBoxCount = stock.ReceiveCount.ReceivePkgOrBoxCount,
                            CountInOnePkgOrBox = stock.ReceiveCount.CountInOnePkgOrBox,
                            CheckOrderCode = stock.CheckData.CheckOrderCode,
                            CheckDate = stock.CheckData.CheckDate?.ToString("yyyy-MM-dd"),
                            CheckNo = stock.CheckData.CheckNo,
                            CheckNoBeforeReCheck = stock.CheckData.CheckNoBeforeReCheck,
                            CheckType = stock.CheckData.CheckTypeInChs(),
                            CheckResult = stock.CheckData.CheckResultInChs(),
                            PassCnt = stock.CheckData.PassCnt,
                            SupplierCode = stock.Supplier.SupplierCode,
                            SupplierName = stock.Supplier.SupplierName
                        };
                        stockDtos.Add(stockDto);
                    }

                    if (mainstock.Status == StockStatus.Waiting && mainstock.RunStatus == RunStatus.In)
                    {
                        BarcodeList barcodeList = await _barcodeListRepository.FindByBarcodeAsync(mainstock.Barcode);
                        if (barcodeList.Warehouse.TargetWarehouseCode == "01")
                        {
                            area = 1;
                        }
                        else if (barcodeList.Warehouse.TargetWarehouseCode == "26")
                        {
                            area = 3;
                        }
                        else
                        {
                            area = 2;
                        }
                    }
                    if (mainstock.CheckData != null && mainstock.CheckData.CheckResult != null && mainstock.CheckData.CheckResult != EnumCheckResult.Pass)
                    {
                        area = 2;
                    }
                    if (mainstock.Status == StockStatus.Freezing)
                    {
                        area = 2;
                    }
                    material = await _materialRepository.FindByMaterialCodeAsync(mainstock.Material.MaterialCode);
                    if (material == null)
                        throw new Exception($"获取物料{mainstock.Material.MaterialCode}失败");
                    try
                    {
                        //ava = await _erpWarehouseAreaPrdtRepository.GetAreaByPrdtName(material.BindType,"B020401");
                        ava = "B020401-1";
                    }
                    catch (Exception)
                    {
                        throw new Exception($"获取ERP物料区域{material.BindType}失败");
                    }



                    /*
                    area = 1;
                    foreach(Stock stock in stocks)
                    {
                        if (stock.Status == StockStatus.Waiting && stock.RunStatus == RunStatus.In)
                        {
                            BarcodeList barcodeList = await _barcodeListRepository.FindByBarcodeAsync(stock.Barcode);
                            if (barcodeList.Warehouse.TargetWarehouseCode == "01")
                            {
                                area = 1;
                            }
                            else if (barcodeList.Warehouse.TargetWarehouseCode == "26")
                            {
                                area = 3;
                                break;
                            }
                            else
                            {
                                area = 2;
                                break;
                            }
                        }
                        if (stock.CheckData.CheckResult != null && stock.CheckData.CheckResult != EnumCheckResult.Pass)
                        {
                            area = 2;
                            break;
                        }

                        if (area == 2) area = 1;

                        StockDto stockDto = new StockDto()
                        {
                            Id = stock.Id,
                            Barcode = stock.Barcode,
                            BoxId = stock.BoxData.BoxId,
                            BoxCode = stock.BoxData.BoxCode,
                            BoxName = stock.BoxData.BoxName,
                            CellId = stock.CellData.CellId,
                            CellCode = stock.CellData.CellCode,
                            CellName = stock.CellData.CellName,
                            HouseId = stock.Warehouse.HouseId,
                            HouseCode = stock.Warehouse.HouseCode,
                            HouseName = stock.Warehouse.HouseName,
                            AreaId = stock.Warehouse.AreaId,
                            AreaCode = stock.Warehouse.AreaCode,
                            AreaName = stock.Warehouse.AreaName,
                            TotalCountInTime = stock.TotalCountInTime,
                            Status = stock.Status.ToString(),
                            StockInType = StockInTypeHelper.StockInTypeToChinese(stock.StockInType),
                            BatchCode = stock.BatchCode,
                            BLCode = stock.BLCode,
                            BHCode = stock.BHCode,
                            StockInDate = stock.StockInDate.ToString("yyyy-MM-dd"),
                            MaterialCode = stock.Material.MaterialCode,
                            MaterialName = stock.Material.MaterialName,
                            Specs = stock.Material.Specs,
                            Unit = stock.Material.Unit,
                            ReceiveTotalCount = stock.ReceiveCount.ReceiveTotalCount,
                            ReceivePkgOrBoxCount = stock.ReceiveCount.ReceivePkgOrBoxCount,
                            CountInOnePkgOrBox = stock.ReceiveCount.CountInOnePkgOrBox,
                            CheckOrderCode = stock.CheckData.CheckOrderCode,
                            CheckDate = stock.CheckData.CheckDate?.ToString("yyyy-MM-dd"),
                            CheckNo = stock.CheckData.CheckNo,
                            CheckNoBeforeReCheck = stock.CheckData.CheckNoBeforeReCheck,
                            CheckType = stock.CheckData.CheckTypeInChs(),
                            CheckResult = stock.CheckData.CheckResultInChs(),
                            PassCnt = stock.CheckData.PassCnt,
                            SupplierCode = stock.Supplier.SupplierCode,
                            SupplierName = stock.Supplier.SupplierName
                        };
                        stockDtos.Add(stockDto);


                        Material material = await _materialRepository.FindByMaterialCodeAsync(stock.Material.MaterialCode);

                        if (material.FullBoxCount == null)
                            throw new Exception($"该物料没有满箱数据");

                        decimal nowproportion = stock.TotalCountInTime / material.FullBoxCount.GetValueOrDefault();
                        if (nowproportion > proportion)
                        {
                            mainstock = stock;
                        }
                    }


                    Material mainmaterial = await _materialRepository.FindByMaterialCodeAsync(mainstock.Material.MaterialCode);
                    try
                    {
                        ava = await _erpWarehouseAreaPrdtRepository.GetAreaByPrdtName(mainmaterial.BindType);
                    }
                    catch (Exception)
                    {
                        throw new Exception($"获取ERP物料区域{mainmaterial.BindType}失败");
                    }
                    */

                    Console.WriteLine("ava=" + ava);

                    var cellList = await _cellRepository.GetListAsync(t => t.WarehouseAreaId == area && t.CellType == CellType.CTUCell
                    && t.RunStatus == CellRunStatus.Enable && t.CellStatus == CellStatus.Nohave && t.AvailableBoxSpecsNames == ava && !useCells.Contains(t.CellCode));
                    Cell cell = cellList.OrderByDescending(t => t.CellCode).FirstOrDefault();

                    if (cell == null)
                    {
                        Console.WriteLine("绑定区域无位置，查询综合区");
                        cellList = await _cellRepository.GetListAsync(t => t.WarehouseAreaId == area && t.CellType == CellType.CTUCell
                        && t.RunStatus == CellRunStatus.Enable && t.CellStatus == CellStatus.Nohave && t.AvailableBoxSpecsNames == null && !useCells.Contains(t.CellCode));
                        cell = cellList.OrderByDescending(t => t.CellCode).FirstOrDefault();
                    }


                    if (cell == null)
                        throw new Exception($"没有能分配的位置");

                    useCells.Add(cell.CellCode);



                    Warehouse warehouse = await _warehouseRepository.FindByIdAsync(cell.WarehouseId);
                    if (warehouse == null)
                        throw new Exception($"库位未绑定仓库");
                    var warehouseArea = warehouse.GetAreaByAreaId(area);
                    if (warehouseArea == null)
                        throw new Exception($"库位未绑定库区");

                    CellDto cellDto = new CellDto();
                    cellDto.Id = cell.Id;
                    cellDto.WarehouseName = warehouse.WarehouseName;
                    cellDto.WarehouseAreaName = warehouseArea.WarehouseAreaName;
                    cellDto.CellCode = cell.CellCode;
                    cellDto.CellName = cell.CellName;
                    cellDto.ShelfName = cell.ShelfName;
                    cellDto.CellStatus = cell.CellStatus.ToString();
                    cellDto.RunStatus = cell.RunStatus.ToString();
                    cellDto.CellType = cell.CellType.ToString();
                    cellDto.AvailableBoxSpecsNames = cell.AvailableBoxSpecsNames;
                    cellDto.LaneToColumn = cell.LaneToColumn;
                    cellDto.LanePosition = cell.LanePosition;
                    cellDto.isHeigh = "0";
                    cellDto.isWeight = "0";

                    GetBySkipDto getBySkip = new GetBySkipDto();
                    getBySkip.boxCode = box.BoxCode;
                    getBySkip.endArea = area;
                    getBySkip.startCode = box.CellData.CellCode;
                    getBySkip.endCell = cellDto;
                    getBySkipDtos.Add(getBySkip);
                }

                dto.stocks = stockDtos;
                dto.getBySkip = getBySkipDtos;
                return dto;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }
        public async Task<CellDto> GetCellByPickOut(string barcode, string boxCode, string pickListCode, string uniqueCode)
        {
            try
            {

                Box box = await _boxRepository.FindByBoxCodeAsync(boxCode);
                if (box == null)
                    throw new Exception($"获取料箱{boxCode}失败");


                var pickList = await _pickListRepository.FindByPickListCodeAsync(pickListCode);
                if (pickListCode == null)
                    throw new Exception($"领料单{pickListCode}不存在");

                var pickItem = pickList.GetPickItemByUniqueCode(uniqueCode);
                if (pickItem == null)
                    throw new Exception($"领料单{pickListCode}中不存在领料项{uniqueCode}");

                Stock stock = await _stockRepository.FindByBoxIdAndBarcodeAsync(box.Id, barcode);
                if (stock == null)
                    throw new Exception($"获取料箱{boxCode}中的物料{barcode}失败");

                ErpDeptTypeDetail deptdetail = await _erpDeptTypeDetailRepository.FindByDeptMaterial(pickList.Picker.DeptCode, stock.Material.MaterialCode);
                string materialType = null;
                if (deptdetail != null)
                    materialType = deptdetail.CLCHKLB_ID;

                Console.WriteLine(materialType);
                CellType cellType;
                if (box.BoxTypeName == "1")
                {
                    cellType = CellType.CTUCell;
                }
                else
                {
                    cellType = CellType.Cell;
                }

                List<Stock> inBoxs = await _stockRepository.GetByBoxIdAsync(box.Id);

                int type;

                if (cellType == CellType.CTUCell)
                {
                    if(stock.TotalCountInTime <= (pickItem.CountToPick - pickItem.PickedCount) && inBoxs.Count == 1)
                        type = 1;
                    else
                        type = 2;

                    //Cell workCell = await _cellRepository.FindByAreaTypeAvailableAsync()
                    //if ( )
                }
                else if (cellType == CellType.Cell)
                {
                    if (stock.TotalCountInTime <= (pickItem.CountToPick - pickItem.PickedCount) && inBoxs.Count == 1)
                        type = 3;
                    else
                        type = 4;
                    //type = 3;
                }
                else
                {
                    type = 0;
                }

                //if (materialType == null)
                //{
                //    type = 2;
                //}


                Cell cell = null;
                if(type ==1)
                {


                    SkipRunStatus status = SkipRunStatus.OutByWare;
                    if (pickList.Type == 1 || pickList.Type == 15)
                    {
                        status = SkipRunStatus.OutByWork;
                    }

                    //if (pickList.Type == 11)
                    if (status == SkipRunStatus.OutByWare)
                    {
                        cell = await _cellRepository.FirstOrDefaultAsync(
                            t => t.RunStatus == CellRunStatus.Enable && t.CellStatus == CellStatus.Nohave && t.WarehouseAreaId == 4 && t.CellType == CellType.WallCell);
                    }
                    else
                    {
                        cell = await _cellManager.GetCellBySkipType(status, pickList.Picker.DeptName, materialType);
                    }
                }
                else if (type == 2)
                {
                    cell = await _cellRepository.FirstOrDefaultAsync(
                        t => t.RunStatus == CellRunStatus.Enable && t.CellStatus == CellStatus.Nohave && t.WarehouseAreaId == 4 && t.CellType == CellType.WallCell);
                }
                else if (type == 3 || type == 4)
                {
                    SkipRunStatus status = SkipRunStatus.OutByWare;
                    if (pickList.Type == 1 || pickList.Type == 15)
                    {
                        status = SkipRunStatus.OutByWork;
                    }

                    if (status == SkipRunStatus.OutByWare)
                    {
                        cell = await _cellRepository.FirstOrDefaultAsync(
                            t => t.RunStatus == CellRunStatus.Enable && t.CellStatus == CellStatus.Nohave && t.WarehouseAreaId == 4 && t.CellType == CellType.Cell);
                    }
                    else
                    {
                        var zzSkipCell = await _cellRepository.FindByZhouZhuanAsync();

                        var skips = await _skipRepository.FindInZhouZhuanAsync(zzSkipCell.Select(o => o.Id).ToList(), 3);

                        if (skips.Where(t => t.SkipRunStatus == SkipRunStatus.Enable && t.SkipStatus == SkipStatus.NoHave).Count() > 0)
                        {
                            Skip skip = skips.Where(t => t.SkipRunStatus == SkipRunStatus.Enable && t.SkipStatus == SkipStatus.NoHave).FirstOrDefault();
                            cell = await _cellRepository.FindByCellCodeAsync(skip.CellCode);
                        }
                    }
                    //cell = await _cellRepository.FirstOrDefaultAsync(
                    //    t => t.RunStatus == CellRunStatus.Enable && t.CellStatus == CellStatus.Nohave && t.WarehouseAreaId == 4 && t.CellType == CellType.Cell);
                }
                //else if (type == 4)
                //{
                //    cell = await _cellRepository.FirstOrDefaultAsync(
                //        t => t.RunStatus == CellRunStatus.Enable && t.CellStatus == CellStatus.Nohave && t.WarehouseAreaId == 4 && t.CellType == CellType.Cell);
                //}
                else
                {

                }
                  

                if(cell ==null)
                    throw new Exception($"没有可分配的下架库位");

                Warehouse warehouse = await _warehouseRepository.FindByIdAsync(cell.WarehouseId);
                if (warehouse == null)
                    throw new Exception($"库位未绑定仓库");
                var warea = warehouse.GetAreaByAreaId(4);
                if (warea == null)
                    throw new Exception($"库位未绑定库区");

                CellDto cellDto = new CellDto();
                cellDto.Id = cell.Id;
                cellDto.WarehouseName = warehouse.WarehouseName;
                cellDto.WarehouseAreaName = warea.WarehouseAreaName;
                cellDto.CellCode = cell.CellCode;
                cellDto.CellName = cell.CellName;
                cellDto.ShelfName = cell.ShelfName;
                cellDto.CellStatus = cell.CellStatus.ToString();
                cellDto.RunStatus = cell.RunStatus.ToString();
                cellDto.CellType = cell.CellType.ToString();
                cellDto.AvailableBoxSpecsNames = cell.AvailableBoxSpecsNames;
                cellDto.LaneToColumn = cell.LaneToColumn;
                cellDto.LanePosition = cell.LanePosition;
                cellDto.isHeigh = "0";
                cellDto.isWeight = "0";


                return cellDto;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }
        public async Task<CellDto> GetCellByWall()
        {
            try
            {
                Cell cell = await _cellRepository.FirstOrDefaultAsync(
                        t => t.RunStatus == CellRunStatus.Enable && t.CellStatus == CellStatus.Nohave && t.WarehouseAreaId == 4 && t.CellType == CellType.WallCell);

                if (cell == null)
                    throw new Exception($"没有可分配的下架库位");

                Warehouse warehouse = await _warehouseRepository.FindByIdAsync(cell.WarehouseId);
                if (warehouse == null)
                    throw new Exception($"库位未绑定仓库");
                var warea = warehouse.GetAreaByAreaId(4);
                if (warea == null)
                    throw new Exception($"库位未绑定库区");

                CellDto cellDto = new CellDto();
                cellDto.Id = cell.Id;
                cellDto.WarehouseName = warehouse.WarehouseName;
                cellDto.WarehouseAreaName = warea.WarehouseAreaName;
                cellDto.CellCode = cell.CellCode;
                cellDto.CellName = cell.CellName;
                cellDto.ShelfName = cell.ShelfName;
                cellDto.CellStatus = cell.CellStatus.ToString();
                cellDto.RunStatus = cell.RunStatus.ToString();
                cellDto.CellType = cell.CellType.ToString();
                cellDto.AvailableBoxSpecsNames = cell.AvailableBoxSpecsNames;
                cellDto.LaneToColumn = cell.LaneToColumn;
                cellDto.LanePosition = cell.LanePosition;
                cellDto.isHeigh = "0";
                cellDto.isWeight = "0";


                return cellDto;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }
        public async Task<CellDto> GetCellByCheck(string barcode, string boxCode)
        {
            try
            {

                Box box = await _boxRepository.FindByBoxCodeAsync(boxCode);
                if (box == null)
                    throw new Exception($"获取料箱{boxCode}失败");

                //ChkResultList chkResult = await _chkResultListRepository.FindByBarcodeAndCheckTypeAsync(barcode, CheckTypeHelper.ChineseToCheckType(checkType));
                //if(chkResult==null)
                //    throw new Exception($"获取领料单{barcode}{1}失败");

                Stock stock = await _stockRepository.FindByBoxIdAndBarcodeAsync(box.Id, barcode);
                if (stock == null)
                    throw new Exception($"获取料箱{boxCode}中的物料{barcode}失败");

                CellType cellType;
                if (box.BoxTypeName == "1")
                {
                    cellType = CellType.CTUCell;
                }
                else
                {
                    cellType = CellType.Cell;
                }

                List<Stock> inBoxs = await _stockRepository.GetByBoxIdAsync(box.Id);

                Cell cell = null;
                cell = await _cellRepository.FirstOrDefaultAsync(
                    t => t.RunStatus == CellRunStatus.Enable && t.CellStatus == CellStatus.Nohave && t.WarehouseAreaId == 4 && t.CellType == CellType.WallCell);

                if (cell == null)
                    throw new Exception($"没有可分配的下架库位");

                Warehouse warehouse = await _warehouseRepository.FindByIdAsync(cell.WarehouseId);
                if (warehouse == null)
                    throw new Exception($"库位未绑定仓库");
                var warea = warehouse.GetAreaByAreaId(4);
                if (warea == null)
                    throw new Exception($"库位未绑定库区");

                CellDto cellDto = new CellDto();
                cellDto.Id = cell.Id;
                cellDto.WarehouseName = warehouse.WarehouseName;
                cellDto.WarehouseAreaName = warea.WarehouseAreaName;
                cellDto.CellCode = cell.CellCode;
                cellDto.CellName = cell.CellName;
                cellDto.ShelfName = cell.ShelfName;
                cellDto.CellStatus = cell.CellStatus.ToString();
                cellDto.RunStatus = cell.RunStatus.ToString();
                cellDto.CellType = cell.CellType.ToString();
                cellDto.AvailableBoxSpecsNames = cell.AvailableBoxSpecsNames;
                cellDto.LaneToColumn = cell.LaneToColumn;
                cellDto.LanePosition = cell.LanePosition;
                cellDto.isHeigh = "0";
                cellDto.isWeight = "0";


                return cellDto;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<List<string>> GetCtuArea()
        {
            List<string> avas = await _cellRepository.GetCTUAreaAvaAsync();
            var depttypes = await _erpDeptTypeRepository.ToListAsync();
            Console.WriteLine(JsonConvert.SerializeObject(depttypes));
            avas.Remove(null);
            List<string> result = new List<string>();
            result.Add("综合");
            foreach (var ava in avas)
            {
                result.Add(depttypes.Where(t=>t.CLCHKLB_ID == ava).Select(t=>t.CLCHKLB_NAME).FirstOrDefault());
            }
            return result;
        }

        public async Task<List<CellLaneStatusDto>> GetLaneCellStatusByCellCodeAsync(string cellCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cellCode))
                    throw new Exception("库位编码不能为空");

                var cell = await _cellRepository.FindByCellCodeAsync(cellCode, includeDetails: false).ConfigureAwait(false);
                if (cell == null)
                    throw new Exception($"库位{cellCode}不存在");

                if (string.IsNullOrWhiteSpace(cell.LaneToColumn))
                    throw new Exception($"库位{cellCode}未配置巷道列");

                var laneCells = await _cellRepository.GetListAsync(c =>
                    c.LaneToColumn == cell.LaneToColumn).ConfigureAwait(false);

                return laneCells
                    .OrderBy(c => c.LanePosition)
                    .Select(c => new CellLaneStatusDto
                    {
                        CellCode = c.CellCode,
                        LanePosition = c.LanePosition,
                        Status = c.CellStatus.ToString(),
                        RunStatus = c.RunStatus.ToString()
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }
    }
}
