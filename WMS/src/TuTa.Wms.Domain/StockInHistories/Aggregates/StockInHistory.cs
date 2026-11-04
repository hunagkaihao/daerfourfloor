using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace TuTa.Wms.StockInHistories.Aggregates
{
    public class StockInHistory : AuditedAggregateRoot<int>
    {
        private StockInHistory()
        {            
        }

        public StockInHistory(
            string barcode,
            string materialCode,
            string materialName,
            string materialSpecs,
            string materialUnit,
            string warehouseCode,
            string warehouseName,
            string areaCode,
            string areaName,
            string cellCode,
            string cellName,
            string boxCode,
            string boxName,
            string stockInType,
            decimal inCount,
            DateTime inTime,
            string operatorName = null,
            string batchNo = null)
        {
            Barcode = barcode;
            MaterialCode = materialCode;
            MaterialName = materialName;
            MaterialSpecs = materialSpecs;
            MaterialUnit = materialUnit;
            WarehouseCode = warehouseCode;
            WarehouseName = warehouseName;
            AreaCode = areaCode;
            AreaName = areaName;
            CellCode = cellCode;
            CellName = cellName;
            BoxCode = boxCode;
            BoxName = boxName;
            StockInType = stockInType;
            InCount = inCount;
            InTime = inTime;
            OperatorName = operatorName;
            BatchNo = batchNo;
            RecordDirectLocation();
        }

        /// <summary>记录业务阶段：Grouping组盘登记、Completed上架完成、Direct直接入库；旧记录为空。</summary>
        [StringLength(20)]
        public string RecordStage { get; private set; }

        /// <summary>本条记录对应的库存标识；无法确定来源的旧记录为空。</summary>
        public Guid? StockId { get; private set; }

        /// <summary>完成上架的AGV任务主键；组盘、直接入库及旧记录为空。</summary>
        public int? AgvTaskId { get; private set; }

        /// <summary>完成上架的AGV请求编号快照，避免任务信息变化影响追溯。</summary>
        [StringLength(64)]
        public string AgvTaskReqCode { get; private set; }

        /// <summary>组盘或搬运开始时的库位编号快照；未知来源为空。</summary>
        [StringLength(20)]
        public string StartCellCode { get; private set; }

        /// <summary>组盘或搬运开始时的库位名称快照。</summary>
        [StringLength(50)]
        public string StartCellName { get; private set; }

        /// <summary>组盘或搬运开始时的容器编号快照。</summary>
        [StringLength(20)]
        public string StartBoxCode { get; private set; }

        /// <summary>组盘或搬运开始时的容器名称快照。</summary>
        [StringLength(50)]
        public string StartBoxName { get; private set; }

        /// <summary>实际完成入库的目标库位编号快照；组盘阶段尚未上架时为空。</summary>
        [StringLength(20)]
        public string EndCellCode { get; private set; }

        /// <summary>实际完成入库的目标库位名称快照。</summary>
        [StringLength(50)]
        public string EndCellName { get; private set; }

        /// <summary>实际完成入库的目标容器编号快照。</summary>
        [StringLength(20)]
        public string EndBoxCode { get; private set; }

        /// <summary>实际完成入库的目标容器名称快照。</summary>
        [StringLength(50)]
        public string EndBoxName { get; private set; }

        /// <summary>直接入库只记录明确的落位目标，不虚构搬运起点，也不修改兼容字段。</summary>
        public void RecordDirectLocation(Guid? stockId = null)
        {
            RecordStage = "Direct";
            StockId = stockId;
            EndCellCode = CellCode;
            EndCellName = CellName;
            EndBoxCode = BoxCode;
            EndBoxName = BoxName;
        }

        /// <summary>组盘记录仅保存起点；当时尚未完成上架，目标和任务信息保持为空。</summary>
        public void RecordGroupingLocation(Guid stockId)
        {
            RecordStage = "Grouping";
            StockId = stockId;
            StartCellCode = CellCode;
            StartCellName = CellName;
            StartBoxCode = BoxCode;
            StartBoxName = BoxName;
            EndCellCode = EndCellName = EndBoxCode = EndBoxName = null;
        }

        /// <summary>保存同一次上架任务的库存、起始点位和容器；目标取本条完成记录的原有落位字段。</summary>
        /// <param name="stockId">本次实际迁移的库存标识。</param>
        /// <param name="agvTaskId">已完成的AGV任务主键。</param>
        /// <param name="reqCode">AGV任务请求编号快照。</param>
        /// <param name="startCellCode">迁移前的实际库位编号。</param>
        /// <param name="startCellName">迁移前的实际库位名称。</param>
        /// <param name="startBoxCode">迁移前的实际容器编号。</param>
        /// <param name="startBoxName">迁移前的实际容器名称。</param>
        public void RecordCompletedLocation(Guid stockId, int agvTaskId, string reqCode,
            string startCellCode, string startCellName, string startBoxCode, string startBoxName)
        {
            RecordStage = "Completed";
            StockId = stockId;
            AgvTaskId = agvTaskId;
            AgvTaskReqCode = reqCode;
            StartCellCode = startCellCode;
            StartCellName = startCellName;
            StartBoxCode = startBoxCode;
            StartBoxName = startBoxName;
        }

        [StringLength(30)]
        public string Barcode { get; private set; }

        [StringLength(20)]
        public string MaterialCode { get; private set; }

        [StringLength(120)]
        public string MaterialName { get; private set; }

        [StringLength(120)]
        public string MaterialSpecs { get; private set; }

        [StringLength(10)]
        public string MaterialUnit { get; private set; }

        [StringLength(20)]
        public string WarehouseCode { get; private set; }

        [StringLength(50)]
        public string WarehouseName { get; private set; }

        [StringLength(20)]
        public string AreaCode { get; private set; }

        [StringLength(50)]
        public string AreaName { get; private set; }

        [StringLength(20)]
        public string CellCode { get; private set; }

        [StringLength(50)]
        public string CellName { get; private set; }

        [StringLength(20)]
        public string BoxCode { get; private set; }

        [StringLength(50)]
        public string BoxName { get; private set; }

        [StringLength(120)]
        public string StockInType { get; private set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal InCount { get; private set; }

        [Column(TypeName = "datetime")]
        public DateTime InTime { get; private set; }

        [StringLength(20)]
        public string OperatorName { get; private set; }

        [StringLength(30)]
        public string BatchNo { get; private set; }
    }
}
