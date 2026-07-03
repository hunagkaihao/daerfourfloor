using System;
using System.ComponentModel.DataAnnotations;
using TuTa.Wms.Stocks.ValueObjects;

namespace TuTa.Wms.Stocks.Events
{
    public class StockRecheckOutEvent
    {
        public StockRecheckOutEvent(
            string barcode,
            MaterialInfoOfStock materialInfo,
            CheckInfoOfStock checkInfo,
            SupplierInfoOfStock supplierInfo,
            string deptCode,
            string deptName,
            string gysCode,
            string gysName,
            string goodsCode,
            string goodsName,
            string goodsSpecs,
            BoxInfoOfStock boxInfo,
            CellInfoOfStock cellInfo,
            WarehouseInfoOfStock houseInfo,
            string stockOutTypeInChs,
            short stockOutType,
            string pickBatch,
            string uniqueCode,
            decimal stockOutCount,
            DateTime stockOutTime,
            string operatorName)
        {
            Barcode = barcode;
            Material = new MaterialInfoOfStock(
                materialInfo.MaterialCode,
                materialInfo.MaterialName,
                materialInfo.Specs,
                materialInfo.Unit, 
                materialInfo.FinGoodsList);
            CheckData = new CheckInfoOfStock(
                checkInfo.CheckOrderCode,
                checkInfo.CheckDate,
                checkInfo.CheckNo,
                checkInfo.CheckNoBeforeReCheck,
                checkInfo.CheckType,
                checkInfo.CheckResult,
                checkInfo.PassCnt);
            Supplier = new SupplierInfoOfStock(
                supplierInfo.SupplierCode,
                supplierInfo.SupplierName, 
                supplierInfo.SupplierBatchCode);
            DeptCode = deptCode;
            DeptName = deptName;
            GysCode = gysCode;
            GysName = gysName;
            GoodsCode = goodsCode;
            GoodsName = goodsName;
            GoodsSpecs = goodsSpecs;
            BoxData = new BoxInfoOfStock(boxInfo.BoxId, boxInfo.BoxCode, boxInfo.BoxName, boxInfo.FullRate);
            CellData = new CellInfoOfStock(cellInfo.CellId, cellInfo.CellCode, cellInfo.CellName, cellInfo.AvaBoxType, cellInfo.CellType);
            Warehouse = new WarehouseInfoOfStock(
                houseInfo.HouseId, houseInfo.HouseCode, houseInfo.HouseName,
                houseInfo.AreaId, houseInfo.AreaCode, houseInfo.AreaName);
            StockOutTypeInChs = stockOutTypeInChs;
            StockOutType = stockOutType;
            PickBatch = pickBatch;
            UniqueCode = uniqueCode;
            StockOutCount = stockOutCount;
            StockOutTime = stockOutTime;
            OperatorName = operatorName;
        }

        /// <summary>
        /// 收料条形码，一次收料生成唯一性条码，WMS作为物料识别码，但可以分成多份与不同的容器进行绑定
        /// </summary>
        public string Barcode { get; private set; }

        /// <summary>
        /// 物料信息
        /// </summary>
        public MaterialInfoOfStock Material { get; private set; }

        /// <summary>
        /// 检测信息
        /// </summary>
        public CheckInfoOfStock CheckData { get; private set; }

        /// <summary>
        /// 供应商信息
        /// </summary>
        public SupplierInfoOfStock Supplier { get; private set; }


        /// <summary>
        /// 领用部门编号
        /// </summary>
        public string DeptCode { get; private set; }

        /// <summary>
        /// 领用部门名称
        /// </summary>
        public string DeptName { get; private set; }

        /// <summary>
        /// 领用外协单位编号
        /// </summary>
        public string GysCode { get; private set; }

        /// <summary>
        /// 领用外协单位名称
        /// </summary>
        public string GysName { get; private set; }

        /// <summary>
        /// 成品编号
        /// </summary>
        public string GoodsCode { get; private set; }

        /// <summary>
        /// 成品名称
        /// </summary>
        public string GoodsName { get; private set; }

        /// <summary>
        /// 成品规格
        /// </summary>
        [StringLength(130)]
        public string GoodsSpecs { get; private set; }

        /// <summary>
        /// 所在容器信息
        /// </summary>
        public BoxInfoOfStock BoxData { get; private set; }

        /// <summary>
        /// 所在库位信息
        /// </summary>
        public CellInfoOfStock CellData { get; private set; }

        /// <summary>
        /// 所在仓库信息
        /// </summary>
        public WarehouseInfoOfStock Warehouse { get; private set; }

        /// <summary>
        /// 出库类型 字符串
        /// </summary>
        public string StockOutTypeInChs { get; private set; }

        /// <summary>
        /// 出库类型
        /// </summary>
        public short StockOutType { get; private set; }

        /// <summary>
        /// 领用生产批号，和领料通知单号一一对应，生产领用及外协领用时存在，无计划领用不存在
        /// </summary>
        public string PickBatch { get; private set; }

        /// <summary>
        /// 唯一编号
        /// </summary>
        public string UniqueCode { get; private set; }

        /// <summary>
        /// 出库数量
        /// </summary>
        public decimal StockOutCount { get; private set; }

        /// <summary>
        /// 出库时间
        /// </summary>
        public DateTime StockOutTime { get; private set; }

        /// <summary>
        /// 操作员
        /// </summary>
        public string OperatorName { get; private set; }
    }
}
