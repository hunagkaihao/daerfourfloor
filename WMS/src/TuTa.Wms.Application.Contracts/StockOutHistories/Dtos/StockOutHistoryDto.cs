using System;
using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.StockOutHistories.Dtos
{
    public class StockOutHistoryDto : AuditedEntityDto<int>
    {
        /// <summary>记录阶段：Completed关联搬运的实际出库、Direct直接出库；旧记录为空。</summary>
        public string RecordStage { get; set; }

        /// <summary>本条实际出库对应的库存标识；旧记录或多来源汇总时为空。</summary>
        public Guid? StockId { get; set; }

        /// <summary>关联的已完成AGV任务主键；直接出库及旧记录为空。</summary>
        public int? AgvTaskId { get; set; }

        /// <summary>已完成出库搬运任务的请求编号快照。</summary>
        public string AgvTaskReqCode { get; set; }

        /// <summary>实际出库来源库位编号快照；未知来源为空。</summary>
        public string StartCellCode { get; set; }

        /// <summary>实际出库来源库位名称快照。</summary>
        public string StartCellName { get; set; }

        /// <summary>实际出库来源容器编号快照。</summary>
        public string StartBoxCode { get; set; }

        /// <summary>实际出库来源容器名称快照。</summary>
        public string StartBoxName { get; set; }

        /// <summary>关联搬运的目标库位编号快照；无明确目标时为空。</summary>
        public string EndCellCode { get; set; }

        /// <summary>目标库位名称快照。</summary>
        public string EndCellName { get; set; }

        /// <summary>目标容器编号快照；无实体容器时为空。</summary>
        public string EndBoxCode { get; set; }

        /// <summary>目标容器名称快照。</summary>
        public string EndBoxName { get; set; }


        public string MaterialCode { get; set; }

        public string MaterialName { get; set; }

        public string Specs { get; set; }

        public string Unit { get; set; }


        public string StockOutType { get; set; }

        public decimal StockOutCount { get; set; }

        public string PickBatch { get; set; }

        public string UniqueCode { get; set; }

        public DateTime StockOutTime { get; set; }

        public string Operator { get; set; }


        public string Barcode { get; set; }

        public string BoxCode { get; set; }

        public string BoxName { get; set; }

        public string CellCode { get; set; }

        public string CellName { get; set; }

        public string AreaCode { get; set; }

        public string AreaName { get; set; }

        public string HouseCode { get; set; }

        public string HouseName { get; set; }


        public string CheckOrderCode { get; set; }

        public string CheckNo { get; set; }

        public DateTime? CheckDate { get; set; }

        public string CheckResult { get; set; }


        public string SupplierCode { get; set; }

        public string SupplierName { get; set; }


        public string DeptCode { get; set; }

        public string DeptName { get; set; }

        public string GysCode { get; set; }

        public string GysName { get; set; }


        public string GoodsCode { get; set; }

        public string GoodsName { get; set; }

        public string GoodsSpecs { get; set; }
    }
}
