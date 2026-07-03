using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TuTa.Wms.Cells.Aggregates;
using TuTa.Wms.Skips;
using TuTa.Wms.Skips.Aggregates;

using Volo.Abp.Domain.Repositories;

namespace TuTa.Wms.Cells
{
    public class CellManager : WmsDomainService
    {
        private readonly ICellRepository _cellRepository;
        private readonly ISkipRepository _skipRepository;

        public CellManager(ICellRepository cellRepository, ISkipRepository skipRepository)
        {
            _cellRepository = cellRepository;
            _skipRepository = skipRepository;
        }

        public async Task<Cell> CreateCellAsync(
            Guid warehouseId,
            int? warehouseAreaId,
            string shelfName,
            string cellCode,
            string cellName,
            string cellType,
            string availableBoxSpecsNames,
            string availableSkipSpecsNames)
        {
            var cellExist = await _cellRepository.FindByCellCodeAsync(cellCode).ConfigureAwait(false);
            if (cellExist != null)
                throw new Exception($"编号为{cellCode}的库位已经存在");
            cellExist = await _cellRepository.FindByCellNameAsync(cellName).ConfigureAwait(false);
            if (cellExist != null)
                throw new Exception($"名称为{cellName}的库位已经存在");

            Cell cell = new Cell(GuidGenerator.Create(), warehouseId, warehouseAreaId, shelfName, cellCode, cellName, cellType, availableBoxSpecsNames, availableSkipSpecsNames);
            return cell;
        }

        public async Task<Cell> GetCellByBarcodeAreaId(string barcode,int areaId,CellType celltype)
        {
            var tgtCell = await _cellRepository.FirstOrDefaultAsync(t => t.WarehouseAreaId == areaId && t.CellType == celltype
                && t.RunStatus == CellRunStatus.Enable && t.CellStatus == CellStatus.Nohave);
            return tgtCell;
        }

        public async Task<Cell> GetCellBySkipType(SkipRunStatus status ,string deptName,string materialType)
        {
            var zzSkipCell = await _cellRepository.FindByZhouZhuanAsync();

            zzSkipCell = zzSkipCell.Where(t => t.RunStatus != CellRunStatus.Selected).ToList();

            var skips = await _skipRepository.FindInZhouZhuanAsync(zzSkipCell.Select(o => o.Id).ToList(), 1);

            List<Cell> cells;

            if (skips.Where(t => t.SkipRunStatus == status).Count() > 0)
            {
                List<Skip> skips1 = skips.Where(t => t.SkipRunStatus == status).ToList();

                if (status == SkipRunStatus.OutByWork)
                {
                    skips1 = skips1.Where(t => t.TargetLocation == deptName && t.TargetCellType == materialType).ToList();
                }

                cells = await _cellRepository.FindByZhouZhuanCellAsync(skips1.Select(o => o.SkipCode).ToList());

                if (cells.Where(t=>t.RunStatus == CellRunStatus.Enable && t.CellStatus == CellStatus.Nohave).ToList().Count == 0)
                {
                    skips1 = skips.Where(t => t.TargetLocation.IsNullOrEmpty() && t.SkipRunStatus == SkipRunStatus.Enable).ToList();
                }

                skips = skips1;
            }
            else
            {
                skips = skips.Where(t => t.TargetLocation.IsNullOrEmpty() && t.SkipRunStatus == SkipRunStatus.Enable).ToList();
            }

            cells = await _cellRepository.FindByZhouZhuanCellAsync(skips.Select(o => o.SkipCode).ToList());

            return cells.Where(t => t.RunStatus == CellRunStatus.Enable && t.CellStatus == CellStatus.Nohave).FirstOrDefault();
        }

        public async Task<Skip> GetCellBySkipTypeLift(SkipRunStatus status)
        {
            var zzSkipCell = await _cellRepository.FindByZhouZhuanAsync();



            var skips = await _skipRepository.FindInZhouZhuanAsync(zzSkipCell.Select(o => o.Id).ToList(), 3);

            if (skips.Where(t => t.SkipRunStatus == SkipRunStatus.Enable && t.SkipStatus == SkipStatus.NoHave).Count() > 0)
            {
                return skips.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }
    }
}
