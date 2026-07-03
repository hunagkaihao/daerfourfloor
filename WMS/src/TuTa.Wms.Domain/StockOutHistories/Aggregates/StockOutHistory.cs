using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

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