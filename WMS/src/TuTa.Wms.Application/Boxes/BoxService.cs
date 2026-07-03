using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.Boxes.Dtos;
using Wms.LogTool;
using TuTa.Wms.Boxes.ValueObjects;
using Volo.Abp.Application.Dtos;
using TuTa.Wms.Cells;
using TuTa.Wms.Warehouses;
using System.Reflection.Emit;
using TuTa.Wms.Stocks;

namespace TuTa.Wms.Boxes
{
    public class BoxService : WmsAppService, IBoxService
    {
        private readonly IBoxRepository _boxRepository;
        private readonly BoxManager _boxManager;
        private readonly ICellRepository _cellRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IStockRepository _stockRepository;
        private readonly ILogger<BoxService> _logger;

        public BoxService(
            IBoxRepository boxRepository, 
            BoxManager boxManager, 
            ICellRepository cellRepository,
            IWarehouseRepository warehouseRepository,
            IStockRepository stockRepository,
            ILogger<BoxService> logger)
        {
            _boxRepository = boxRepository;
            _boxManager = boxManager;
            _cellRepository = cellRepository;
            _warehouseRepository = warehouseRepository;
            _stockRepository = stockRepository;
            _logger = logger;
        }

        public async Task<ResponseDto> AddBoxAsync(BoxAddDto para)
        {
            try
            {
                BoxSpecsValObj specs = new BoxSpecsValObj(para.BoxSpecsName, para.BoxLength, para.BoxWidth, para.BoxHeight);
                var box = await _boxManager.CreateBoxAsync(para.BoxCode, para.BoxName, para.BoxTypeName, specs).ConfigureAwait(false);

                await _boxRepository.InsertAsync(box).ConfigureAwait(false);
                return new ResponseDto() { success = true, message = "添加容器成功" };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<ResponseDto> AddBoxListAsync(List<BoxAddDto> paras)
        {
            try
            {
                foreach(var para in paras)
                {
                    BoxSpecsValObj specs = new BoxSpecsValObj(para.BoxSpecsName, para.BoxLength, para.BoxWidth, para.BoxHeight);
                    if (paras.Where(o => o.BoxCode == para.BoxCode || o.BoxName == para.BoxName).Count() > 1)
                        throw new Exception($"容器码为{para.BoxCode},容器名为{para.BoxName}的容器重复");

                    var box = await _boxManager.CreateBoxAsync(para.BoxCode, para.BoxName, para.BoxTypeName, specs).ConfigureAwait(false);

                    await _boxRepository.InsertAsync(box).ConfigureAwait(false);
                }
                
                return new ResponseDto() { success = true, message = "添加容器成功" };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<ResponseDto> DelBoxAsync(string boxCode)
        {
            try
            {
                var boxExist = await _boxRepository.FindByBoxCodeAsync(boxCode).ConfigureAwait(false);
                if (boxExist == null)
                    return new ResponseDto() { success = true, message = $"容器{boxCode}不存在，默认删除成功" };

                //if (boxExist.StocksInBox != null && boxExist.StocksInBox.Count > 0)  //数据不是很正确
                //    return new ResponseDto() { success = false, message = $"容器{boxCode}中存在物料，无法删除容器" };
                var stocksInBox = await _stockRepository.GetByBoxIdAsync(boxExist.Id).ConfigureAwait(false);
                if (stocksInBox != null && stocksInBox.Count > 0)
                    return new ResponseDto() { success = false, message = $"容器{boxCode}中存在物料，无法删除容器" };

                //if (boxExist.CellData.CellId != null)
                //    //boxExist.DisBindCell();
                //    return new ResponseDto() { success = false, message = $"容器{boxCode}与库位{boxExist.CellData.CellId}绑定中，无法删除容器" };
                boxExist.DisBindCell(); //解绑库位，再删除容器

                await _boxRepository.DeleteAsync(boxExist).ConfigureAwait(false);
                return new ResponseDto() { success = true, message = "删除容器成功" };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<ResponseDto> DelAllBoxesAsync()
        {
            try
            {
                var allboxes = await _boxRepository.GetAllAsync(false).ConfigureAwait(false);
                foreach (var box in allboxes)
                {
                    if (box.StocksInBox != null && box.StocksInBox.Count > 0)
                        return new ResponseDto() { success = false, message = $"容器{box.BoxCode}中存在物料，无法删除容器" };

                    if (box.CellData.CellId != null)
                        return new ResponseDto() { success = false, message = $"容器{box.BoxCode}与库位{box.CellData.CellId}绑定中，无法删除容器" };

                    await _boxRepository.DeleteAsync(box).ConfigureAwait(false);
                }

                return new ResponseDto() { success = true, message = "删除容器成功" };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<ResponseDto> UpdateBoxAsync(Guid boxId, BoxUpdateDto para)
        {
            try
            {
                var boxExist = await _boxRepository.FindAsync(boxId).ConfigureAwait(false);
                if (boxExist == null)
                    return new ResponseDto() { success = false, message = $"容器{boxId}不存在，更新失败" };

                BoxSpecsValObj specs = new BoxSpecsValObj(para.BoxSpecsNameNew, para.BoxLengthNew, para.BoxWidthNew, para.BoxHeightNew);
                await _boxManager.ModifyBoxAsync(boxExist, para.BoxCodeNew, para.BoxNameNew, para.BoxTypeNameNew, specs).ConfigureAwait(false);

                await _boxRepository.UpdateAsync(boxExist).ConfigureAwait(false);
                return new ResponseDto() { success = true, message = "更新容器成功" };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<PagedResultDto<BoxDto>> PagedBoxesGetAsync(PagedBoxesQueryDto para)
        {
            try
            {
                Guid? cellId = null;
                if (para.CellName != null)
                {
                    var cell = await _cellRepository.FindByCellNameAsync(para.CellName).ConfigureAwait(false);
                    if (cell == null)
                        return new PagedResultDto<BoxDto>();

                    cellId = cell.Id;
                }

                Guid? warehouseId = null;
                int? warehouseAreaId = null;
                if (para.WarehouseName != null)
                {
                    var warehouse = await _warehouseRepository.FindByNameAsync(para.WarehouseName).ConfigureAwait(false);
                    if (warehouse == null)
                        return new PagedResultDto<BoxDto>();

                    warehouseId = warehouse.Id;

                    if (para.WarehouseAreaName != null)
                    {
                        var area = warehouse.GetAreaByAreaName(para.WarehouseAreaName);
                        if (area == null)
                            return new PagedResultDto<BoxDto>();

                        warehouseAreaId = area.Id;
                    }
                }
                else
                {
                    if (para.WarehouseAreaName != null)  //未指定仓库的时候，如果指定库区，默认不返回数据
                        return new PagedResultDto<BoxDto>();
                }

                var boxes = await _boxRepository.GetPagedBoxAsync(
                    para.BoxCode, para.BoxName, cellId, warehouseAreaId, warehouseId, 
                    false, false, para.SkipCount, para.MaxResultCount);

                if (boxes == null)
                    return new PagedResultDto<BoxDto>();

                List<BoxDto> boxDtos = new List<BoxDto>();
                foreach(var box in boxes.Items)
                {
                    BoxDto boxDto = new BoxDto()
                    {
                        Id = box.Id,
                        BoxCode = box.BoxCode,
                        BoxName = box.BoxName,
                        BoxTypeName = box.BoxTypeName,
                        SpecsName = box.BoxSpecs.SpecsName,
                        Length = box.BoxSpecs.Length,
                        Width = box.BoxSpecs.Width,
                        Height = box.BoxSpecs.Height,
                        Status = box.Status.ToString(),
                        CellName = box.CellData.CellName,
                        WarehouseAreaName = box.WarehouseData.WarehouseAreaName,
                        WarehouseName = box.WarehouseData.WarehouseName,
                    };
                    boxDtos.Add(boxDto);
                }

                return new PagedResultDto<BoxDto>()
                {
                    TotalCount = boxes.TotalCount,
                    Items = boxDtos
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }



        //public async Task<ResponseDto> BindGoodsAsync(Guid boxId, Guid goodsId)
        //{
        //    try
        //    {
        //        var box = await _boxRepository.FindAsync(boxId).ConfigureAwait(false);
        //        if (box == null)
        //            throw new Exception($"Id为{boxId}的容器不存在");

        //        BoxGoods boxGoods = new BoxGoods(boxId, goodsId);
        //        box.AddGoods(boxGoods);

        //        await _boxRepository.UpdateAsync(box).ConfigureAwait(false);
        //        return new ResponseDto() { success = true, message = "容器添加物料成功" };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(ex.Message);
        //        throw new UserFriendlyException(ex.Message);
        //    }
        //}
    }
}
