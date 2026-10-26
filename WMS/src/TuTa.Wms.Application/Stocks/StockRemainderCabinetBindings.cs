using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.Cells;
using TuTa.Wms.Stocks.Dtos;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Uow;

namespace TuTa.Wms.Stocks
{
    public partial class StockService
    {
        /// <summary>按固定物料查找柜位，独立于现有库存；同一物料可绑定多个实际格位。</summary>
        /// <param name="materialCode">箱码第1段物料编码，精确匹配，不传批次或等级。</param>
        /// <returns>所有已配置位置；等级在首笔寄存后固定，缺货格位仍保留。</returns>
        [UnitOfWork(IsDisabled = true)]
        public async Task<List<RemainderCabinetDto>> GetRemainderLocationsAsync(string materialCode)
        {
            if (string.IsNullOrWhiteSpace(materialCode)) return new List<RemainderCabinetDto>();
            using (var read = UnitOfWorkManager.Begin(true, false))
            {
                var code = materialCode.Trim();
                var cells = (await GetRemainderCellsAsync()).Where(x => x.RemainderMaterialCode == code).ToList();
                var warehouses = await _warehouseRepository.GetAllWarehousesAsync();
                var names = warehouses.ToDictionary(x => x.Id, x => x.WarehouseName);
                var result = cells.OrderBy(x => x.CellCode, StringComparer.OrdinalIgnoreCase).Select(x => new RemainderCabinetDto {
                    CellCode = NormalizeRemainderCellCode(x.CellCode), MaterialCode = code, Grade = x.RemainderGrade,
                    WarehouseName = names.GetValueOrDefault(x.WarehouseId),
                    CanStore = x.RunStatus == CellRunStatus.Enable && x.CellStatus != CellStatus.Full
                }).ToList();
                await read.CompleteAsync();
                return result;
            }
        }

        /// <summary>绑定物料，缺失的标准柜位在现有零头柜区内补建；已有其他物料时须先解绑。</summary>
        [UnitOfWork(IsDisabled = true)]
        public Task<ResponseDto> BindRemainderLocationAsync(RemainderCabinetBindingDto input)
            => ChangeRemainderBindingAsync(input, false);

        /// <summary>解绑固定物料及等级，禁止改动有库存、占用或正在作业的格位。</summary>
        [UnitOfWork(IsDisabled = true)]
        public Task<ResponseDto> UnbindRemainderLocationAsync(RemainderCabinetBindingDto input)
            => ChangeRemainderBindingAsync(input, true);

        /// <summary>与寄存、领用共用柜位锁和事务，防止读到空柜后又被并发寄存。</summary>
        /// <param name="input">绑定的新编码；解绑时为页面读取的旧编码，防止过期页面误解绑。</param>
        /// <param name="unbind">是否清除物料和等级；不清除历史流水或操作重试摘要。</param>
        private async Task<ResponseDto> ChangeRemainderBindingAsync(RemainderCabinetBindingDto input, bool unbind)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.MaterialCode) || input.MaterialCode.Trim().Length > 20)
                return new ResponseDto { success = false, message = "请扫描或输入有效的物料编码" };
            await Erp.ErpAsnAppService.LocalPushLock.WaitAsync();
            try
            {
                var cellCode = NormalizeRemainderCellCode(input.CellCode);
                var materialCode = input.MaterialCode.Trim();
                await using var handle = _u8GroupingLock == null ? null : await _u8GroupingLock.TryAcquireAsync("U8:REMAINDER:" + cellCode);
                if (_u8GroupingLock != null && handle == null)
                    return new ResponseDto { success = false, message = "该柜位正在操作，请稍后重试" };
                using (var uow = UnitOfWorkManager.Begin(true, true))
                {
                    var existing = await _cellRepository.GetListAsync(x => x.CellCode == cellCode);
                    var created = existing.Count == 0;
                    if (created && unbind)
                        return new ResponseDto { success = false, message = "柜位不存在，无需解绑" };
                    if (!unbind && await _materialRepository.FindByMaterialCodeAsync(materialCode) == null)
                        return new ResponseDto { success = false, message = "物料主数据不存在，请先同步物料" };
                    if (created)
                    {
                        var warehouses = await _warehouseRepository.GetAllWarehousesAsync();
                        var areas = warehouses.SelectMany(w => w.WarehouseAreas.Select(a => new { Warehouse = w, Area = a }))
                            .Where(x => x.Area.WarehouseAreaFlag == RemainderAreaFlag).ToList();
                        if (areas.Count != 1)
                            return new ResponseDto { success = false, message = "零头柜区未配置或存在多个，请先确认4楼叉车库的唯一零头柜区" };
                        var area = areas[0];
                        var cell = await _cellManager.CreateCellAsync(area.Warehouse.Id, area.Area.Id, cellCode.Substring(0, 1),
                            cellCode, $"零头柜{cellCode[0]}第{int.Parse(cellCode.Substring(1, 2))}列第{int.Parse(cellCode.Substring(4, 2))}层",
                            nameof(CellType.HandCell), null, null);
                        cell.ConfigureRemainderMaterial(materialCode);
                        cell.SetProperty("RemainderCabinet", "true");
                        cell.SetProperty("RemainderCabinetCode", cellCode.Substring(0, 1));
                        cell.SetProperty("RemainderColumn", int.Parse(cellCode.Substring(1, 2)));
                        cell.SetProperty("RemainderLayer", int.Parse(cellCode.Substring(4, 2)));
                        await _cellRepository.InsertAsync(cell);
                    }
                    else
                    {
                        var location = await ResolveRemainderCellAsync(cellCode);
                        var current = location.Cell.RemainderMaterialCode;
                        if (unbind && !string.IsNullOrEmpty(current) && current != materialCode)
                            return new ResponseDto { success = false, message = "柜位绑定已变化，请重新读取后再解绑" };
                        if (!unbind && !string.IsNullOrEmpty(current) && current != materialCode)
                            return new ResponseDto { success = false, message = $"柜位已绑定{current}，更换物料前请先解绑" };
                        if (!unbind && current == materialCode)
                            return new ResponseDto { success = true, message = $"{cellCode}已绑定该物料，无需重复绑定" };
                        var stocks = await _stockRepository.GetByCellIdAsync(location.Cell.Id);
                        if (stocks.Any(x => x.TotalCountInTime > 0 || (x.InspectionCount ?? 0) > 0) ||
                            location.Cell.CellStatus != CellStatus.Nohave || location.Cell.RunStatus != CellRunStatus.Enable)
                            return new ResponseDto { success = false, message = "柜位仍有库存、占用或未处于启用空柜状态，不能修改绑定" };
                        location.Cell.ConfigureRemainderMaterial(unbind ? null : materialCode);
                        await _cellRepository.UpdateAsync(location.Cell);
                    }
                    await uow.CompleteAsync();
                    return new ResponseDto { success = true, message = unbind ? $"{cellCode}已解绑物料及等级" : $"{cellCode}已绑定{materialCode}" };
                }
            }
            catch (UserFriendlyException error) { return new ResponseDto { success = false, message = error.Message }; }
            finally { Erp.ErpAsnAppService.LocalPushLock.Release(); }
        }
    }
}
