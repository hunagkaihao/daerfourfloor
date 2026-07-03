using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TuTa.Wms.Boxes.ValueObjects;
using TuTa.Wms.Cells.Aggregates;
using TuTa.Wms.Domain;
using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.Cells
{
    public interface ICellRepository : IRepository<Cell, Guid>
    {
        Task<Cell> FindByCellCodeAsync(
            string cellCode, 
            bool includeDetails = true, 
            CancellationToken cancellationToken = default);
        Task<Cell> FindByCellCode2Async(
            string cellCode,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);

        Task<Cell> FindByCellNameAsync(
            string cellName,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);

        Task<Cell> FindByIdAsync(
            Guid cellId,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);



        public Task<int> FindCountByShelfNameAsync(
            string shelfName,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);

        Task<List<Cell>> FindByZhouZhuanAsync(
            bool includeDetails = true,
            CancellationToken cancellationToken = default);
        Task<List<Cell>> FindByZhouZhuanCellAsync(
            List<string> skips,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);
        Task<List<Cell>> FindBySkipCellAsync(
            string skip,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);
        Task<List<Cell>> FindByAreaCellAsync(
            int areaId,int count,string ava,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);
        Task<List<Cell>> FindByWorkSendAsync(
            int areaId, string boxtype,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);
        Task<List<Cell>> FindByAreaTypeAvailableAsync(
            int areaId, CellType type,string ava,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);

        Task<List<Cell>> FindSkipCellByAreaTypeAsync(
            int areaId,int skipType,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);

        //Task<List<Cell>> GetNoHaveByWall(
        //    int count,
        //    bool includeDetails = true,
        //    CancellationToken cancellationToken = default);

        Task<List<Cell>> GetNoHaveByAreaCellType(
            int count,int areaId,CellType type,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);

        Task<List<Cell>> GetNoHaveBox(
            string ava,CellType type,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);

        Task<List<Cell>> GetNoHaveBoxWall(
            bool includeDetails = true,
            CancellationToken cancellationToken = default);


        Task<QueryDataInPage<Cell>> GetPagedCellsAsync(
            Guid? warehouseId,
            int? warehouseAreaId,
            string shelfName,
            CellStatus? cellStatus,
            CellRunStatus? cellRunStatus,
            CellType? cellType,
            string availableBoxSpecsNamesTip,
            string cellCodeTip, 
            string cellNameTip,
            bool includeDetails = true,
            int skipCount = 0,
            int maxResultCount = 10,
            CancellationToken cancellationToken = default);


        Task<QueryDataInPage<Cell>> GetPagedCellsByAreaAsync(
            int warehouseAreaId,
            string heigh,
            string weight,
            CellType cellType,
            string cellCode,
            bool includeDetails = true,
            int skipCount = 0,
            int maxResultCount = 10,
            CancellationToken cancellationToken = default);
        Task<List<string>> GetCTUAreaAvaAsync(
            CancellationToken cancellationToken = default);
    }
}
