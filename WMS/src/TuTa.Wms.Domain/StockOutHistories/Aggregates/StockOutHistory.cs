using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using TuTa.Wms.StockOutHistories;

namespace TuTa.Wms.StockOutHistories.Aggregates
{
    public class StockOutHistory : AuditedAggregateRoot<int>
    {
        private StockOutHistory()
        {            
        }

        public StockOutHistory(
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
            string stockOutType,
            decimal outCount,
            DateTime outTime,
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
            StockOutType = stockOutType;
            OutCount = outCount;
            OutTime = outTime;
            OperatorName = operatorName;
            BatchNo = batchNo;
            RecordLocation(null, null);
        }

        /// <summary>记录阶段：Completed关联搬运的实际出库、Direct直接出库；旧记录为空。</summary>
        [StringLength(20)]
        public string RecordStage { get; private set; }

        /// <summary>本条实际出库对应的库存标识；旧记录或多来源汇总时为空。</summary>
        public Guid? StockId { get; private set; }

        /// <summary>关联的已完成AGV任务主键；直接出库及旧记录为空。</summary>
        public int? AgvTaskId { get; private set; }

        /// <summary>已完成出库搬运任务的请求编号快照。</summary>
        [StringLength(64)]
        public string AgvTaskReqCode { get; private set; }

        /// <summary>实际出库来源库位编号快照；未知来源为空。</summary>
        [StringLength(20)]
        public string StartCellCode { get; private set; }

        /// <summary>实际出库来源库位名称快照。</summary>
        [StringLength(50)]
        public string StartCellName { get; private set; }

        /// <summary>实际出库来源容器编号快照。</summary>
        [StringLength(20)]
        public string StartBoxCode { get; private set; }

        /// <summary>实际出库来源容器名称快照。</summary>
        [StringLength(50)]
        public string StartBoxName { get; private set; }

        /// <summary>关联搬运的目标库位编号快照；无明确目标时为空。</summary>
        [StringLength(20)]
        public string EndCellCode { get; private set; }

        /// <summary>目标库位名称快照。</summary>
        [StringLength(50)]
        public string EndCellName { get; private set; }

        /// <summary>目标容器编号快照；无实体容器时为空。</summary>
        [StringLength(20)]
        public string EndBoxCode { get; private set; }

        /// <summary>目标容器名称快照。</summary>
        [StringLength(50)]
        public string EndBoxName { get; private set; }


        /// <summary>保留旧字段，补充已知出库来源；只有明确关联的已完成搬运才填写目标和任务。</summary>
        /// <param name="stockId">本次实际扣减库存的标识，多库存汇总或未知来源可为空。</param>
        /// <param name="route">与当前库存位置核对后的搬运快照；没有真实搬运目标时为空。</param>
        public void RecordLocation(Guid? stockId, StockOutHistoryRoute route)
        {
            StockId = stockId;
            RecordStage = route == null ? "Direct" : "Completed";
            StartCellCode = route == null ? CellCode : route.StartCellCode;
            StartCellName = route == null ? CellName : route.StartCellName;
            StartBoxCode = route == null ? BoxCode : route.StartBoxCode;
            StartBoxName = route == null ? BoxName : route.StartBoxName;
            EndCellCode = route?.EndCellCode;
            EndCellName = route?.EndCellName;
            EndBoxCode = route?.EndBoxCode;
            EndBoxName = route?.EndBoxName;
            AgvTaskId = route?.AgvTaskId;
            AgvTaskReqCode = route?.AgvTaskReqCode;
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
        public string StockOutType { get; private set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal OutCount { get; private set; }

        [Column(TypeName = "datetime")]
        public DateTime OutTime { get; private set; }

        [StringLength(20)]
        public string OperatorName { get; private set; }

        [StringLength(30)]
        public string BatchNo { get; private set; }
    }
}
