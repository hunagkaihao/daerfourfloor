using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuTa.Wms.Application.Contracts.Shared;
using TuTa.Wms.Cells.Dtos;
using TuTa.Wms.Skips.Dtos;

using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TuTa.Wms.Cells
{
    public interface ICellService : IApplicationService
    {
        Task<ResponseDto> AddCellAsync(CellAddDto para);

        Task<ResponseDto> AddCellsAsync(List<CellAddDto> paras);

        Task<ResponseDto> DelCellAsync(Guid cellId);

        Task<ResponseDto> CellsBindToAreaAsync(CellsBindAreaDto para);

        Task<ResponseDto> CellsDisBindToAreaAsync(CellsDisBindFromAreaDto para);

        Task<PagedResultDto<CellDto>> GetPagedCellsAsync(PagedCellsQueryDto para);

        Task<PagedResultDto<CellDto>> GetPagedCellsByAreaAsync(PagedCellsAreaDto para);

        Task<CellDto> GetCellByStock(string barcode, string boxCode);

        Task<CellDto> GetCellByWorkShop(string skipCode, int areaId);

        Task<CellDto> GetCellByPickOut(string barcode, string boxCode, string pickListCode, string uniqueCode);

        Task<CellDto> GetCellByWall();

        Task<CellDto> GetCellByCheck(string barcode, string boxCode);

        Task<ResultGetBySkipDto> GetCellsBySkip(string skipCode);

        Task<List<string>> GetCtuArea();

        Task<List<CellLaneStatusDto>> GetLaneCellStatusByCellCodeAsync(string cellCode);
    }
}
