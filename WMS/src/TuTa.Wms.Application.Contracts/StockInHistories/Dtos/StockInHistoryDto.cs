using System;
using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.StockInHistories.Dtos
{
    public class StockInHistoryDto : AuditedEntityDto<int>
    {
        /// <summary>记录阶段：Grouping组盘登记、Completed上架完成、Direct直接入库；旧记录为空。</summary>
        public string RecordStage { get; set; }

        /// <summary>来源库存标识；旧记录或无法明确关联时为空。</summary>
        public Guid? StockId { get; set; }

        /// <summary>完成上架的AGV任务主键；非AGV入库和旧记录为空。</summary>
        public int? AgvTaskId { get; set; }

        /// <summary>完成上架的AGV请求编号快照。</summary>
        public string AgvTaskReqCode { get; set; }

        /// <summary>起始库位编号快照；未知来源为空。</summary>
        public string StartCellCode { get; set; }

        /// <summary>起始库位名称快照。</summary>
        public string StartCellName { get; set; }

        /// <summary>起始容器编号快照。</summary>
        public string StartBoxCode { get; set; }

        /// <summary>起始容器名称快照。</summary>
        public string StartBoxName { get; set; }

        /// <summary>完成入库的目标库位编号快照；组盘阶段为空。</summary>
        public string EndCellCode { get; set; }

        /// <summary>目标库位名称快照。</summary>
        public string EndCellName { get; set; }

        /// <summary>目标容器编号快照。</summary>
        public string EndBoxCode { get; set; }

        /// <summary>目标容器名称快照。</summary>
        public string EndBoxName { get; set; }


        public string MaterialCode { get; set; }

        public string MaterialName { get; set; }

        public string MaterialSpecs { get; set; }

        public string MaterialUnit { get; set; }


        public string StockInType { get; set; }

        public decimal InCount { get; set; }

        public DateTime InTime { get; set; }

        public string OperatorName { get; set; }


        public string Barcode { get; set; }

        public string BoxCode { get; set; }

        public string BoxName { get; set; }

        public string CellCode { get; set; }

        public string CellName { get; set; }

        public string AreaCode { get; set; }

        public string AreaName { get; set; }

        public string WarehouseCode { get; set; }

        public string WarehouseName { get; set; }


        public string CheckOrderCode { get; set; }

        public string CheckNo { get; set; }

        public DateTime? CheckDate { get; set; }

        public string CheckResult { get; set; }


        public string SupplierCode { get; set; }

        public string SupplierName { get; set; }

        public string BatchNo { get; set; }

        public bool IsHB { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public string BLCode { get; set; }

        public string BHCode { get; set; }
    }
}
