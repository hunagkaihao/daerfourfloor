using Abp.UI;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using TuTa.Wms.AgvTasks;
using TuTa.Wms.AgvTasks.Aggregaes;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.BarcodeLists;
using TuTa.Wms.Boxes;
using TuTa.Wms.Boxes.Aggregates;
using TuTa.Wms.Cells;
using TuTa.Wms.Cells.Aggregates;
using TuTa.Wms.Erp;
using TuTa.Wms.Erp.Entities;
using TuTa.Wms.Skips.Aggregates;
using TuTa.Wms.Skips.Dtos;
using TuTa.Wms.Stocks;
using TuTa.Wms.Stocks.Aggregates;
using TuTa.Wms.Warehouses;
using TuTa.Wms.Warehouses.Aggregates;
using TuTa.Wms.Warehouses.Entities;

using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Uow;

using Wms.LogTool;

namespace TuTa.Wms.Skips
{
    public class SkipService:WmsAppService,ISkipService
    {
        private readonly ISkipRepository _skipRepository;
        private readonly ICellRepository _cellRepository;
        private readonly IStockRepository _stockRepository;
        private readonly IBoxRepository _boxRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IBarcodeCheckRepository _barcodeCheckRepository;
        private readonly IErpDeptTypeRepository _erpDeptTypeRepository;
        private readonly SkipManager _skipManager;
        private readonly AgvTaskManager _agvTaskManager;
        private readonly ILocalEventBus _localEventBus;
        private readonly ILogger<SkipService> _logger;

        public SkipService(ISkipRepository skipRepository,
            ICellRepository cellRepository,
            IStockRepository stockRepository,
            IBoxRepository boxRepository,
            IWarehouseRepository warehouseRepository,
            IErpDeptTypeRepository erpDeptTypeRepository,
            IBarcodeCheckRepository barcodeCheckRepository,
            SkipManager skipManager,
            AgvTaskManager agvTaskManager,
            ILocalEventBus localEventBus,
            ILogger<SkipService> logger)
        {
            _skipRepository = skipRepository;
            _cellRepository = cellRepository;
            _stockRepository = stockRepository;
            _boxRepository = boxRepository;
            _warehouseRepository = warehouseRepository;
            _barcodeCheckRepository = barcodeCheckRepository;
            _erpDeptTypeRepository = erpDeptTypeRepository;
            _skipManager = skipManager;
            _agvTaskManager = agvTaskManager;
            _localEventBus = localEventBus;
            _logger = logger;
        }

        public async Task<ResponseDto> AddSkipAsync(SkipAddDto para)
        {
            try
            {
                Skip skip = await _skipManager.CreateSkipAsync(para.SkipCode, para.SkipName, para.Type);
                await _skipRepository.InsertAsync(skip).ConfigureAwait(false);
                return new ResponseDto() { success = true, message = "创建料车成功" };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }


        public async Task<ResponseDto> SkipBindCellAsync(string skipCode,string cellCode,string podDir,string isBind)
        {
            try
            {
                Skip skip = await _skipRepository.FindBySkipCodeAsync(skipCode);
                if(skip == null)
                    return new ResponseDto() { success = false, message = $"料车码为{skipCode}的料车不存在" };

                Cell cell = await _cellRepository.FindByCellCodeAsync(cellCode);
                if(cell == null)
                    return new ResponseDto() { success = false, message = $"库位码为{skipCode}的库位不存在" };

                if(cell.CellType!=CellType.Skip)
                    return new ResponseDto() { success = false, message = $"库位不是料车库位" };

                if (isBind == "0")
                {
                    if (cell.RunStatus != CellRunStatus.Enable && cell.CellStatus == CellStatus.Nohave)
                        return new ResponseDto() { success = false, message = $"库位状态不是有料车库位" };

                    skip.CellId = null;
                    skip.CellCode = null;
                    skip.AreaId = 0;
                    cell.SetCellStatus(CellStatus.Nohave);
                }
                else
                {
                    if (cell.RunStatus != CellRunStatus.Enable && cell.CellStatus != CellStatus.Nohave)
                        return new ResponseDto() { success = false, message = $"库位状态不是待使用空库位" };

                    skip.CellId = cell.Id;
                    skip.CellCode = cell.CellCode;
                    skip.AreaId = cell.WarehouseAreaId.GetValueOrDefault();

                    cell.SetCellStatus(CellStatus.Have);
                }

                await _agvTaskManager.BindPodAndBerthAsync(cell.CellCode, skipCode, isBind, podDir);

                await _skipRepository.UpdateAsync(skip).ConfigureAwait(false);

                await _cellRepository.UpdateAsync(cell);

                return new ResponseDto() { success = true, message = "绑定料车成功" };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        [UnitOfWork]
        public async Task<ResponseDto> SendSkipAsync(string skipCode, string cellCode)
        {
            try
            {
                Skip skip = await _skipRepository.FindBySkipCodeAsync(skipCode);
                if (skip == null)
                    return new ResponseDto() { success = false, message = $"料车码为{skipCode}的料车不存在" };

                Cell cell = await _cellRepository.FindByCellCodeAsync(cellCode);
                if (cell == null)
                    return new ResponseDto() { success = false, message = $"库位码为{skipCode}的库位不存在" };

                if (cell.CellType != CellType.Skip)
                    return new ResponseDto() { success = false, message = $"库位不是料车库位" };

                if (cell.RunStatus != CellRunStatus.Enable && cell.CellStatus != CellStatus.Nohave)
                    return new ResponseDto() { success = false, message = $"库位状态不是待使用空库位" };

                Cell startCell = await _cellRepository.FindByIdAsync((Guid)skip.CellId);
                if(startCell == null)
                    return new ResponseDto() { success = false, message = $"料车未绑定库位" };

                if(await _skipRepository.FindByCellIdAsync(cell.Id) != null)
                    return new ResponseDto() { success = false, message = $"目标点位已有料车" };

                if (skip.Type == 1)
                {
                    List<Cell> skipCells = await _cellRepository.FindBySkipCellAsync(skip.SkipCode);
                    if (skipCells.Where(t => t.CellStatus != CellStatus.Nohave).ToList().Count() == 0)
                        return new ResponseDto() { success = false, message = $"空料车无法发送" };
                }

                if (await _agvTaskManager.IsExistSkipTask(skipCode))
                    return new ResponseDto() { success = false, message = $"料车已存在任务" };

                if (cell.WarehouseAreaId==4)
                {
                    if(startCell.WarehouseAreaId == 5)
                    {
                        if (skip.SkipRunStatus != SkipRunStatus.In)
                            return new ResponseDto() { success = false, message = $"料车不是入库料车" };

                        await SetAsExecutingAsync(startCell, cell, skip, ManageType.SkipMove);
                    }
                    else
                    {
                        return new ResponseDto() { success = false, message = $"料车不在入库区" };
                    }
                }
                else if(cell.WarehouseAreaId == 5)
                {
                    if(startCell.WarehouseAreaId == 4)
                    {
                        if (skip.SkipRunStatus != SkipRunStatus.OutByWare)
                            return new ResponseDto() { success = false, message = $"料车不是出仓库料车" };
                        await SetAsExecutingAsync(startCell, cell, skip, ManageType.SkipMove);
                    }
                    else
                    {
                        return new ResponseDto() { success = false, message = $"料车不在周转区" };
                    }
                }
                else
                {
                    Warehouse warehouse = await _warehouseRepository.FindByIdAsync(cell.WarehouseId).ConfigureAwait(false);
                    WarehouseArea warehouseArea = warehouse.GetAreaByAreaId((int)cell.WarehouseAreaId);

                    if (skip.TargetLocation != warehouseArea.WarehouseAreaName)
                    {
                        return new ResponseDto() { success = false, message = $"该料车中是{skip.TargetLocation}物料" };
                    }
                    if(skip.TargetCellType != cell.AvailableBoxSpecsNames)
                    {
                        return new ResponseDto() { success = false, message = $"该料车中是{skip.TargetCellType}类型物料与点位不匹配" };
                    }

                    if (skip.Type == 3)
                    {
                        await _agvTaskManager.BindCtnrAndBinAsync(startCell.CellCode2, "2", null, "0");
                        Box box = await _boxRepository.FindByCellIdAsync((Guid)startCell.Id);
                        Cell skipCell = (await _cellRepository.FindBySkipCellAsync(skip.SkipCode)).FirstOrDefault();
                        Warehouse warehousestart = await _warehouseRepository.FindByIdAsync(startCell.WarehouseId).ConfigureAwait(false);
                        WarehouseArea warehouseAreastart = warehouse.GetAreaByAreaId((int)startCell.WarehouseAreaId);
                        box.BindCell(skipCell, warehousestart, warehouseAreastart);
                        await _boxRepository.UpdateAsync(box);
                    }
                    await SetAsExecutingAsync(startCell, cell, skip, ManageType.SkipSend);
                }
                await CurrentUnitOfWork.SaveChangesAsync();


                cell.SetSelected();
                await _cellRepository.UpdateAsync(cell);
                await CurrentUnitOfWork.SaveChangesAsync();

                return new ResponseDto() { success = true, message = "料车发送成功" };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        [UnitOfWork]
        public async Task<ResponseDto> CallSkipAsync(string skipCode, int areaId)
        {
            try
            {
                Skip skip = await _skipRepository.FindBySkipCodeAsync(skipCode);
                if (skip == null)
                    return new ResponseDto() { success = false, message = $"料车码为{skipCode}的料车不存在" };

                List<Cell> cells = await _cellRepository.FindSkipCellByAreaTypeAsync(areaId, skip.Type);

                if (cells.Count == 0)
                    return new ResponseDto() { success = false, message = $"该区域没有对应类型的料车空位" };

                Cell startCell = await _cellRepository.FindByIdAsync((Guid)skip.CellId);
                if (startCell == null)
                    return new ResponseDto() { success = false, message = $"料车未绑定库位" };

                if (await _agvTaskManager.IsExistSkipTask(skipCode))
                    return new ResponseDto() { success = false, message = $"料车已存在任务" };

                Cell endCell = cells.FirstOrDefault();

                Console.WriteLine(JsonConvert.SerializeObject(endCell));

                if (await _skipRepository.FindByCellIdAsync(endCell.Id) != null)
                    return new ResponseDto() { success = false, message = $"目标点位已有料车" };

                if (areaId == 12)
                {
                    return new ResponseDto() { success = false, message = $"暂未完成" };
                    //await SetAsExecutingAsync(startCell, skip, ManageType.SkipCall);
                }

                if ((startCell.WarehouseAreaId==7 || startCell.WarehouseAreaId==8||startCell.WarehouseAreaId==9 || startCell.WarehouseAreaId == 10))
                {
                    if(areaId == 12)
                    {
                        return new ResponseDto() { success = false, message = $"暂未完成" };
                        //await SetAsExecutingAsync(startCell, skip, ManageType.SkipCall);
                    }
                    else
                    {
                        await SetAsExecutingAsync(startCell, endCell, skip, ManageType.SkipCall);
                    }
                }
                else
                {
                    await SetAsExecutingAsync(startCell, endCell, skip, ManageType.SkipMove);
                }
                await CurrentUnitOfWork.SaveChangesAsync();

                Cell endCell2 = await _cellRepository.FindByCellCodeAsync(endCell.CellCode);
                _logger.Info($"{endCell2.CellCode}锁定2");
                endCell2.SetSelected();
                await _cellRepository.UpdateAsync(endCell2);
                await CurrentUnitOfWork.SaveChangesAsync();

                return new ResponseDto() { success = true, message = "料车叫回成功" };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<PagedResultDto<SkipDto>> GetPagedSkips(PagedSkipDto para)
        {
            try
            {
                SkipStatus status;

                if (!Enum.TryParse<SkipStatus>(para.skipStatus, out status))
                    throw new Exception($"料车状态{para.skipStatus}无法识别");

                var Skips = await _skipRepository.GetPagedSkipsAsync(
                    para.areaId,
                    status,
                    false,
                    para.SkipCount,
                    para.MaxResultCount);

                _logger.Debug(JsonConvert.SerializeObject(Skips));

                PagedResultDto<SkipDto> result = new PagedResultDto<SkipDto>()
                {
                    TotalCount = Skips.TotalCount
                };

                List<SkipDto> items = new List<SkipDto>();
                foreach (var item in Skips.Items)
                {
                    if (item.CellId == null)
                    {
                        continue;
                    }
                    Cell cell = await _cellRepository.FindByIdAsync((Guid)item.CellId);
                    if(cell == null || cell.RunStatus != CellRunStatus.Enable)
                    {
                        continue;
                    }
                    List<Cell> skipcells = await _cellRepository.FindBySkipCellAsync(item.SkipCode);
                    if (skipcells.Where(t => t.RunStatus != CellRunStatus.Enable).Count() > 0)
                    {
                        continue;
                    }
                    SkipDto dto = new SkipDto();
                    dto.SkipCode = item.SkipCode;
                    dto.SkipName = item.SkipName;
                    dto.SkipCellCode = item.CellCode;
                    items.Add(dto);
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

        public async Task<PagedResultDto<SkipOutDto>> GetPagedSkipsOut()
        {
            try
            {
                var zzSkipCell = await _cellRepository.FindByZhouZhuanAsync();

                var skips = await _skipRepository.FindInZhouZhuanAsync(zzSkipCell.Select(o => o.Id).ToList(), 1);

                skips = skips.Where(t => t.SkipRunStatus == SkipRunStatus.OutByWork || t.SkipRunStatus == SkipRunStatus.OutByWare).ToList();

                PagedResultDto<SkipOutDto> result = new PagedResultDto<SkipOutDto>();

                List<SkipOutDto> items = new List<SkipOutDto>();
                foreach (var skip in skips)
                {
                    Cell skipCell = await _cellRepository.FindByCellCodeAsync(skip.CellCode);
                    if (skipCell.RunStatus == CellRunStatus.Selected)
                        continue;

                    List<Cell> cells = await _cellRepository.FindBySkipCellAsync(skip.SkipCode);
                    if(cells.Where(t=>t.RunStatus == CellRunStatus.Selected).Count() > 0)
                        continue;

                    ErpDeptType cellType = await _erpDeptTypeRepository.FirstOrDefaultAsync(t => t.CLCHKLB_ID == skip.TargetCellType);

                    SkipOutDto item = new SkipOutDto()
                    {
                        SkipCode = skip.SkipCode,
                        SkipName = skip.SkipName,
                        SkipCellCode = skip.CellCode,
                        BindCellCounts = cells.Where(t => t.CellStatus != CellStatus.Nohave).Count(),
                        SkipRunStatus = cellType == null ? skip.TargetLocation : cellType.CLCHKLB_NAME
                        //SkipRunStatus = SkipRunStatusHelper.SkipRunStatusToChinese(skip.SkipRunStatus),
                        //TargetCellType = cellType == null ? null : cellType.CLCHKLB_NAME
                    };
                    items.Add(item);
                }

                result.Items = items;
                result.TotalCount = items.Count;
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        [UnitOfWork]
        public async Task<ResponseDto> SetNoHaveStatus(string skipCode)
        {
            try
            {
                Skip skip = await _skipRepository.FindBySkipCodeAsync(skipCode);
                if (skip == null)
                    return new ResponseDto() { success = false, message = $"料车码为{skipCode}的料车不存在" };

                List<Cell> cells = await _cellRepository.FindBySkipCellAsync(skipCode);
                if (cells.Count == 0)
                    return new ResponseDto() { success = false, message = $"查询料车库位失败" };


                /*
                int isAuto = 2;
                //是否自动模式
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        HttpResponseMessage response = await client.GetAsync("http://localhost:327/ecs/GetWorkBinDevType");
                        if (response.IsSuccessStatusCode)
                        {
                            string responseBody = await response.Content.ReadAsStringAsync();

                            Console.WriteLine(responseBody);
                            //JSON数据中的结构有一层额外的嵌套。在result字段中，实际的数据是作为字符串存储的，
                            // 解析JSON数据
                            if (responseBody == "1")
                            {
                                isAuto = 1;
                            }
                            else if (responseBody == "2")
                            {
                                isAuto = 2;
                            }
                            else if (responseBody == "0")
                            {
                                isAuto = 2;
                            }
                            else
                            {
                                return new ResponseDto() { success = false, message = $"读取输送线状态失败" };
                            }
                        }
                        else
                        {
                            return new ResponseDto() { success = false, message = $"读取输送线状态失败" };
                        }
                    }
                }
                catch (Exception)
                {
                }
                */

                List<Box> boxs = null;
                var stocks = await _stockRepository.GetSkipCellStockAsync(cells.Select(t => t.CellCode).ToList());
                if (stocks == null || stocks.Count == 0)
                {
                }
                else
                {
                    boxs = await _boxRepository.GetByCellsIdAsync(cells.Select(t => t.Id).ToList());
                    foreach (Box box in boxs)
                    {
                        var checks = await _barcodeCheckRepository.GetByBoxAsync(box.Id);
                        //if (box.PickOutType == "out" && isAuto == 1)
                        //    continue;
                        foreach (var check in checks)
                        {
                            await _barcodeCheckRepository.DeleteAsync(check);
                        }

                        List<Stock> boxstocks= stocks.Where(t=>t.CellData.CellId==box.CellData.CellId).ToList();
                        foreach(Stock stock in boxstocks)
                        {
                            await _stockRepository.DeleteAsync(stock);
                        }
                    }

                    //return new ResponseDto() { success = false, message = $"该料车物料未全部收料" };
                }


                boxs = await _boxRepository.GetByCellsIdAsync(cells.Select(t => t.Id).ToList());
                foreach (Box box in boxs)
                {

                    Cell cell = await _cellRepository.FindByCellCodeAsync(box.CellData.CellCode).ConfigureAwait(false);

                    /*
                    if (box.PickOutType == "out" && isAuto==1)
                    {

                        if (cell.ShelfName.IsNullOrEmpty())
                            return new ResponseDto() { success = false, message = $"容器不在料车上，无法出库" };

                        Cell skipCell = await _cellRepository.FindByCellCodeAsync(skip.CellCode);

                        if (skipCell.WarehouseAreaId != 5 || skipCell.AvailableBoxSpecsNames != "ctuin" || skipCell.CellType != CellType.Skip)
                            return new ResponseDto() { success = false, message = $"所在料车位置错误" };

                        try
                        {
                            //是否出库模式
                            using (HttpClient client = new HttpClient())
                            {
                                HttpResponseMessage response = await client.GetAsync($"http://localhost:327/ecs/GetWorkbinRuntype");
                                if (response.IsSuccessStatusCode)
                                {

                                    //Console.WriteLine("POST 请求成功，响应内容：" + responseBody);


                                    string responseBody = await response.Content.ReadAsStringAsync();

                                    //JSON数据中的结构有一层额外的嵌套。在result字段中，实际的数据是作为字符串存储的，
                                    // 解析JSON数据
                                    if (responseBody == "1")
                                    {
                                        //入库
                                        return new ResponseDto() { success = false, message = $"输送线入库模式，无法出库" };
                                    }
                                    else if (responseBody == "2")
                                    {
                                        //出库
                                    }
                                    else
                                    {
                                        return new ResponseDto() { success = false, message = $"读取输送线状态失败" };
                                    }


                                }
                                else
                                {
                                    return new ResponseDto() { success = false, message = $"读取输送线状态失败" };
                                }
                            }
                        }
                        catch (Exception)
                        {
                            return new ResponseDto() { success = false, message = $"读取输送线状态失败" };
                        }
                        Cell endCell = await _cellRepository.FindByCellCodeAsync("700020A9501013");
                        await SetAsExecutingAsync(cell, endCell, endCell.ShelfName, box, ManageType.CTUSSXOut).ConfigureAwait(false);
                    }
                    else
                    {
                        cell.SetCellStatus(CellStatus.Nohave);
                        await _cellRepository.UpdateAsync(cell);

                        box.SetNoHave();
                        box.DisBindCell();
                        box.PickOutType = null;
                        await _boxRepository.UpdateAsync(box);
                    }
                    */

                    cell.SetCellStatus(CellStatus.Nohave);
                    await _cellRepository.UpdateAsync(cell);

                    if (box.BoxTypeName == "1")
                        await _agvTaskManager.BindCtnrAndBinAsync(box.CellData.CellCode, box.BoxTypeName, null, "0");


                    box.SetNoHave();
                    box.DisBindCell();
                    await _boxRepository.UpdateAsync(box);
                }
                skip.SkipStatus = SkipStatus.NoHave;
                skip.SkipRunStatus = SkipRunStatus.Enable;
                skip.TargetLocation = null;
                skip.TargetCellType = null;

                await _skipRepository.UpdateAsync(skip);


                return new ResponseDto() { success = true, message = "设置空料车成功" };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        [UnitOfWork]
        public async Task<ResponseDto> ClearWallNoHaveBox()
        {
            try
            {
                List<Cell> cells = await _cellRepository.GetNoHaveBoxWall();
                if (cells.Count == 0)
                    return new ResponseDto() { success = true, message = $"设置空分拨墙成功" };



                List<Box> boxs = await _boxRepository.GetNoHaveByCellsIdAsync(cells.Select(t => t.Id).ToList());

                foreach (Box box in boxs)
                {

                    Cell cell = await _cellRepository.FindByCellCodeAsync(box.CellData.CellCode).ConfigureAwait(false);


                    await _agvTaskManager.BindCtnrAndBinAsync(box.CellData.CellCode, box.BoxTypeName, box.BoxCode, "0");


                    cell.SetCellStatus(CellStatus.Nohave);
                    await _cellRepository.UpdateAsync(cell);

                    box.SetNoHave();
                    box.DisBindCell();
                    await _boxRepository.UpdateAsync(box);
                }




                return new ResponseDto() { success = true, message = "设置空分拨墙成功" };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        [UnitOfWork]
        public async Task<ResponseDto> SetReceipt(string skipCode)
        {
            try
            {
                return new ResponseDto() { success = true, message = "收料成功" };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }


        private async Task<AgvTask> SetAsExecutingAsync(Cell startCell, Cell endCell, Skip skip, ManageType type)
        {
            _logger.Info($"{startCell.CellCode}已锁定");
            startCell.SetSelected();
            await _cellRepository.UpdateAsync(startCell);
            _logger.Info($"{endCell.CellCode}已锁定");
            endCell.SetSelected();
            await _cellRepository.UpdateAsync(endCell);


            AgvTask agvtask = await _agvTaskManager.CreateSkipTaskAsync(startCell.CellName, endCell.CellName, skip.SkipCode,type);
            return agvtask;
        }


        private async Task<AgvTask> SetAsExecutingAsync(Cell startCell, Skip skip, ManageType type)
        {
            _logger.Info($"{startCell.CellCode}已锁定");
            startCell.SetSelected();
            await _cellRepository.UpdateAsync(startCell);


            AgvTask agvtask = await _agvTaskManager.CreateSkipCallTaskAsync(startCell.CellName, skip.SkipCode, type);
            return agvtask;
        }
    }
}
