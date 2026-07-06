using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using TuTa.Wms.Stocks.ValueObjects;
using TuTa.Wms.Stocks.Events;
using System.ComponentModel.DataAnnotations.Schema;
using TuTa.Wms.ChkResultLists;
using TuTa.Wms.Cells.Aggregates;
using TuTa.Wms.Warehouses.Aggregates;
using TuTa.Wms.Warehouses.Entities;
using TuTa.Wms.Cells;

namespace TuTa.Wms.Stocks.Aggregates
{
    public class Stock : AuditedAggregateRoot<Guid>
    {
        private Stock()
        {

        }
        internal Stock(
            Guid id,
            string barcode,
            decimal totalCountInTime,
            MaterialInfoOfStock materialInformation,
            StockInType stockInType)
            : base(id)
        {
            Barcode = Check.NotNullOrWhiteSpace(barcode, nameof(barcode));
            Material = Check.NotNull(materialInformation, nameof(materialInformation));
            StockInType = stockInType;

            //if (CheckData.PassCnt == null && totalCountInTime > ReceiveCount.ReceiveTotalCount)
            //{
            //    throw new Exception($"未检测库存的库存总数不能大于收料总数，当前库存总数为{totalCountInTime}，收料总数为{ReceiveCount.ReceiveTotalCount}");
            //}
            //if (CheckData.PassCnt != null && totalCountInTime > CheckData.PassCnt)
            //{
            //    throw new Exception($"已检测库存的库存总数不能大于检测通过总数，当前库存总数为{totalCountInTime}，检测通过数为{CheckData.PassCnt}");
            //}
            TotalCountInTime = totalCountInTime;


            //默认没有绑定容器和库位,检验信息
            BoxData = new BoxInfoOfStock(null, null, null, null);
            CellData = new CellInfoOfStock(null, null, null, null, null);
            Warehouse = new WarehouseInfoOfStock(null, null, null, null, null, null);
            CheckData = new CheckInfoOfStock(null, null, null);

            RunStatus = RunStatus.In;

        }
        internal Stock(
            Guid id,
            string barcode, 
            decimal totalCountInTime,
            MaterialInfoOfStock materialInformation, 
            CountInfoOfStock countInformation, 
            SupplierInfoOfStock supplierInformation, 
            StockInType stockInType, 
            int isTag,
            string batchCode, 
            string bLCode, 
            string bHCode)
            : base(id)
        {
            Barcode = Check.NotNullOrWhiteSpace(barcode, nameof(barcode));
            Material = Check.NotNull(materialInformation, nameof(materialInformation));
            ReceiveCount = Check.NotNull(countInformation, nameof(countInformation));
            Supplier = Check.NotNull(supplierInformation, nameof(supplierInformation));
            StockInType = stockInType;

            //if (CheckData.PassCnt == null && totalCountInTime > ReceiveCount.ReceiveTotalCount)
            //{
            //    throw new Exception($"未检测库存的库存总数不能大于收料总数，当前库存总数为{totalCountInTime}，收料总数为{ReceiveCount.ReceiveTotalCount}");
            //}
            //if (CheckData.PassCnt != null && totalCountInTime > CheckData.PassCnt)
            //{
            //    throw new Exception($"已检测库存的库存总数不能大于检测通过总数，当前库存总数为{totalCountInTime}，检测通过数为{CheckData.PassCnt}");
            //}
            TotalCountInTime = totalCountInTime;
            TotalPagOrBox = countInformation.ReceivePkgOrBoxCount;

            BatchCode = batchCode;
            BLCode = bLCode;
            BHCode = bHCode;

            //默认没有绑定容器和库位,检验信息
            BoxData = new BoxInfoOfStock(null, null, null,null); 
            CellData = new CellInfoOfStock(null, null, null,null,null);
            Warehouse = new WarehouseInfoOfStock(null, null, null, null, null, null);
            CheckData = new CheckInfoOfStock(null, null, null);

            RunStatus = RunStatus.In;
            if (isTag == 1)
            {
                Status = StockStatus.Waiting;
            }
            else if (isTag == 2)
            {
                Status = StockStatus.Available;
            }
            else
            {
                throw new Exception($"是否需要检验状态错误，状态为{isTag}");
            }
        }
        internal Stock(
            Guid id,
            string barcode,
            decimal totalCountInTime,
            MaterialInfoOfStock materialInformation,
            CountInfoOfStock countInformation,
            SupplierInfoOfStock supplierInformation,
            StockInType stockInType,
            StockStatus status,
            string batchCode,
            string bLCode,
            string bHCode)
            : base(id)
        {
            Barcode = Check.NotNullOrWhiteSpace(barcode, nameof(barcode));
            Material = Check.NotNull(materialInformation, nameof(materialInformation));
            ReceiveCount = Check.NotNull(countInformation, nameof(countInformation));
            Supplier = Check.NotNull(supplierInformation, nameof(supplierInformation));
            StockInType = stockInType;

            //if (CheckData.PassCnt == null && totalCountInTime > ReceiveCount.ReceiveTotalCount)
            //{
            //    throw new Exception($"未检测库存的库存总数不能大于收料总数，当前库存总数为{totalCountInTime}，收料总数为{ReceiveCount.ReceiveTotalCount}");
            //}
            //if (CheckData.PassCnt != null && totalCountInTime > CheckData.PassCnt)
            //{
            //    throw new Exception($"已检测库存的库存总数不能大于检测通过总数，当前库存总数为{totalCountInTime}，检测通过数为{CheckData.PassCnt}");
            //}
            TotalCountInTime = totalCountInTime;

            BatchCode = batchCode;
            BLCode = bLCode;
            BHCode = bHCode;

            //默认没有绑定容器和库位,检验信息
            BoxData = new BoxInfoOfStock(null, null, null, null);
            CellData = new CellInfoOfStock(null, null, null,null,null);
            Warehouse = new WarehouseInfoOfStock(null, null, null, null, null, null);
            CheckData = new CheckInfoOfStock(null, null, null);

            RunStatus = RunStatus.In;
            Status = status;
        }
        internal Stock(
            Guid id,
            string barcode,
            decimal totalCountInTime,
            MaterialInfoOfStock materialInformation,
            CountInfoOfStock countInformation,
            SupplierInfoOfStock supplierInformation,
            CheckInfoOfStock checkInformation,
            StockInType stockInType,
            string batchCode,
            string bLCode,
            string bHCode)
            : base(id)
        {
            Barcode = Check.NotNullOrWhiteSpace(barcode, nameof(barcode));
            Material = Check.NotNull(materialInformation, nameof(materialInformation));
            ReceiveCount = Check.NotNull(countInformation, nameof(countInformation));
            Supplier = Check.NotNull(supplierInformation, nameof(supplierInformation));
            StockInType = stockInType;

            //if (CheckData.PassCnt == null && totalCountInTime > ReceiveCount.ReceiveTotalCount)
            //{
            //    throw new Exception($"未检测库存的库存总数不能大于收料总数，当前库存总数为{totalCountInTime}，收料总数为{ReceiveCount.ReceiveTotalCount}");
            //}
            //if (CheckData.PassCnt != null && totalCountInTime > CheckData.PassCnt)
            //{
            //    throw new Exception($"已检测库存的库存总数不能大于检测通过总数，当前库存总数为{totalCountInTime}，检测通过数为{CheckData.PassCnt}");
            //}
            TotalCountInTime = totalCountInTime;

            BatchCode = batchCode;
            BLCode = bLCode;
            BHCode = bHCode;

            //默认没有绑定容器和库位,检验信息
            BoxData = new BoxInfoOfStock(null, null, null, null);
            CellData = new CellInfoOfStock(null, null, null, null, null);
            Warehouse = new WarehouseInfoOfStock(null, null, null, null, null, null);
            CheckData = checkInformation;

            RunStatus = RunStatus.Out;
            Status = StockStatus.StockOut;
        }

        internal Stock(
            Guid id,
            string barcode,
            decimal totalCountInTime,
            MaterialInfoOfStock materialInformation,
            CountInfoOfStock countInformation,
            CheckInfoOfStock checkInfoOfStock,
            SupplierInfoOfStock supplierInformation,
            StockInType stockInType,
            StockStatus stockStatus,
            string batchCode,
            string bLCode,
            string bHCode)
            : base(id)
        {
            Barcode = Check.NotNullOrWhiteSpace(barcode, nameof(barcode));
            Material = Check.NotNull(materialInformation, nameof(materialInformation));
            ReceiveCount = Check.NotNull(countInformation, nameof(countInformation));
            CheckData = Check.NotNull(checkInfoOfStock, nameof(checkInfoOfStock));
            Supplier = Check.NotNull(supplierInformation, nameof(supplierInformation));
            StockInType = stockInType;


            TotalCountInTime = totalCountInTime;

            BatchCode = batchCode;
            BLCode = bLCode;
            BHCode = bHCode;

            //默认没有绑定容器和库位,检验信息
            BoxData = new BoxInfoOfStock(null, null, null, null);
            CellData = new CellInfoOfStock(null, null, null, null, null);
            Warehouse = new WarehouseInfoOfStock(null, null, null, null, null, null);

            RunStatus = RunStatus.Enable;
            Status = stockStatus;
        }

        //private int StockInTypeCheck(int stockType)
        //{
        //    if (stockType != 1 && stockType != 2 && stockType != 4 && stockType != 5 && stockType != 7)
        //        throw new Exception("入库类型stockType值无效，可取值范围为：1，2，4，5，7");

        //    return stockType;
        //}

        //public static string StockInTypeToString(int stockInType)
        //{
        //    if (stockInType != 1 && stockInType != 2 && stockInType != 4 && stockInType != 5 && stockInType != 7)
        //        throw new Exception("入库类型stockType值无效，可取值范围为：1，2，4，5，7");

        //    switch(stockInType)
        //    {
        //        case 1: return "正常采购";
        //        case 2: return "生产入库";
        //        case 4: return "委托加工";
        //        case 5: return "盘点入库";
        //        default: return "超期复检";
        //    }
        //}

        //public static int StockInTypeToInt(string stockType)
        //{
        //    if (stockType != "正常采购" && stockType != "生产入库" && stockType != "委托加工" && stockType != "盘点入库" && stockType != "超期复检")
        //        throw new Exception("入库类型stockType值无效，可取值范围为：正常采购，生产入库，委托加工，盘点入库，超期复检");

        //    switch (stockType)
        //    {
        //        case "正常采购": return 1;
        //        case "生产入库": return 2;
        //        case "委托加工": return 4;
        //        case "盘点入库": return 5;
        //        default: return 7;
        //    }
        //}


        /// <summary>
        /// 移除指定数量的物料
        /// </summary>
        /// <param name="outCount"></param>
        /// <exception cref="ArgumentException"></exception>
        public virtual void Remove(decimal outCount, int? pagOrBoxCount = null)
        {
            if (outCount < 0)
                throw new ArgumentException("出库数量不能小于0");

            if (outCount > TotalCountInTime)
                throw new ArgumentException("出库数量超过已有数量");

            TotalCountInTime -= outCount;
            if (pagOrBoxCount.HasValue && TotalPagOrBox.HasValue && TotalPagOrBox >= pagOrBoxCount.Value)
                TotalPagOrBox = TotalPagOrBox - pagOrBoxCount.Value;

            if (TotalCountInTime == 0) //领完了
            {
                //通知容器更新数据
                //AddLocalEvent(
                //    new StockUsedUpEvent()
                //    {
                //        StockId = Id,
                //        BoxId = BoxData.BoxId
                //    }
                //);
            }
        }
        /// <summary>
        /// 库存合并，将另一个库存合并到本库存中
        /// </summary>
        /// <param name="otherStock"></param>
        /// <exception cref="ArgumentException"></exception>
        public virtual void CombineStock(Stock otherStock)
        {
            if (otherStock == null)
                throw new ArgumentNullException("otherStock不能为null");

            if (otherStock.Barcode != this.Barcode)
                throw new Exception("库存的收料条形码必须一致，才能合并库存");

            if (otherStock.TotalCountInTime < 0)
                throw new ArgumentException("合并的库存数量小于0，属于无效库存");

            //decimal totalCountOfThisBarcode = CheckData.PassCnt == null ? ReceiveCount.ReceiveTotalCount : CheckData.PassCnt.Value;

            //if (otherStock.TotalCountInTime + this.TotalCountInTime > totalCountOfThisBarcode)
            //    throw new ArgumentException($"合并库存后的总库存数量的超过了收料时的总数");

            TotalCountInTime += otherStock.TotalCountInTime;
        }

        /// <summary>
        /// 库存添加
        /// </summary>
        /// <param name="otherStock"></param>
        /// <exception cref="ArgumentException"></exception>
        public virtual void CombineStock(decimal count)
        {
            TotalCountInTime += count;
        }

        public virtual void SetCheck(CheckInfoOfStock check)
        {
            CheckData = check;
        }


        public virtual void BindBox(Guid boxId, string boxCode, string boxName, string boxNumber = null)
        {


            BoxData = new BoxInfoOfStock(boxId, boxCode, boxName , 0, boxNumber);



            //库存绑定容器后，需要通知容器进行状态修改
            AddLocalEvent(new StockBindBoxEvent()
            {
                StockId = this.Id,
                StockBarcode = this.Barcode,
                BoxId = boxId
            });
        }

        public virtual void BindCell(
            Guid cellId, string cellCode, string cellName,string avaType,CellType cellType,
            int? areaId, string areaCode, string areaName, 
            Guid? houseId, string houseCode, string houseName)
        {
            if (BoxData.BoxId == null)
                throw new Exception("当前库存尚未绑定容器，无法绑定库位");

            CellData = new CellInfoOfStock(cellId, cellCode, cellName, avaType, cellType);
            Warehouse = new WarehouseInfoOfStock(houseId, houseCode, houseName, areaId, areaCode, areaName);
            //StockInDate = DateTime.Now;
        }

        public virtual void BindCell(
            Cell cell,
            Warehouse warehouse,
            WarehouseArea warehouseArea)
        {
            CellData = new CellInfoOfStock(cell.Id, cell.CellCode, cell.CellName, cell.AvailableBoxSpecsNames,cell.CellType);
            Warehouse = new WarehouseInfoOfStock(warehouse.Id, warehouse.WarehouseCode, warehouse.WarehouseName
                , warehouseArea.Id, warehouseArea.WarehouseAreaCode, warehouseArea.WarehouseAreaName);
        }

        public virtual void DisBindCell()
        {
            //BoxData = new BoxInfoOfStock(null, null, null);
            CellData = new CellInfoOfStock(null, null, null, null, null);
            Warehouse = new WarehouseInfoOfStock(null, null, null, null, null, null);
            //StockInDate = null;
        }

        public void FreezeStock()
        {
            Status = StockStatus.Freezing;
        }

        public void ReturnToAvailable()
        {
            Status = StockStatus.Available;
        }

        public void SetStatus(StockStatus status)
        {
            Status = status;
        }

        public void SetRunStatus(RunStatus status)
        {
            RunStatus = status;
        }

        /// <summary>
        /// 更新入库时间
        /// </summary>
        /// <param name="date"></param>
        public void UpdateStockInDate(DateTime date)
        {
            StockInDate = date;
        }

        /// <summary>
        /// 设置等级
        /// </summary>
        /// <param name="grade"></param>
        public void SetGrade(string grade)
        {
            Grade = grade;
        }

        /// <summary>
        /// 设置工序号
        /// </summary>
        /// <param name="processNo"></param>
        public void SetProcessNo(string processNo)
        {
            ProcessNo = processNo;
        }

        /// <summary>
        /// 设置收料条形码
        /// </summary>
        /// <param name="receivingMaterialBarcode"></param>
        public void SetReceivingMaterialBarcode(string receivingMaterialBarcode)
        {
            ReceivingMaterialBarcode = receivingMaterialBarcode;
        }

        /// <summary>
        /// 收料条形码，一次收料生成唯一性条码，WMS作为物料识别码，但可以分成多份与不同的容器进行绑定
        /// </summary>
        [StringLength(30)]
        [Required]
        public virtual string Barcode { get; private set; }

        /// <summary>
        /// 料车ID
        /// </summary>
        //public virtual Guid? VehicleId { get; set; }


        /// <summary>
        /// 所在容器信息
        /// </summary>
        [Required]
        public virtual BoxInfoOfStock BoxData { get; private set; }

        /// <summary>
        /// 所在库位信息
        /// </summary>
        [Required]
        public virtual CellInfoOfStock CellData { get; private set; }

        /// <summary>
        /// 所在仓库信息
        /// </summary>
        [Required]
        public virtual WarehouseInfoOfStock Warehouse { get; private set; }



        /// <summary>
        /// 实时物料总数
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public virtual decimal TotalCountInTime { get; private set; }

        /// <summary>
        /// 实时包或箱数
        /// </summary>
        public virtual int? TotalPagOrBox { get; set; }

        /// <summary>
        /// 库存状态，包括：可用的，冻结的
        /// </summary>
        public virtual StockStatus Status { get; private set; }

        /// <summary>
        /// 运行状态，包括：可用的，锁定的
        /// </summary>
        public virtual RunStatus RunStatus { get; private set; }

        /// <summary>
        /// 入库类型  1(正常采购） 2（生产入库：指半成品） 4(委托加工） 7(超期复检）
        /// </summary>
        public virtual StockInType StockInType { get; private set; }

        /// <summary>
        /// 生产批号
        /// </summary>
        [StringLength(180)]
        public virtual string BatchCode { get; private set; }

        /// <summary>
        /// 备料单号
        /// </summary>
        [StringLength(30)]
        public virtual string BLCode { get; private set; }

        /// <summary>
        /// 备货单号
        /// </summary>
        [StringLength(30)]
        public virtual string BHCode { get; private set; }

        /// <summary>
        /// 等级
        /// </summary>
        [StringLength(20)]
        public virtual string Grade { get; private set; }

        /// <summary>
        /// 工序号
        /// </summary>
        [StringLength(20)]
        public virtual string ProcessNo { get; private set; }

        /// <summary>
        /// 收料条形码
        /// </summary>
        [StringLength(200)]
        public virtual string ReceivingMaterialBarcode { get; private set; }

        /// <summary>
        /// 入库日期
        /// </summary>
        [Column(TypeName = "date")]
        public virtual DateTime StockInDate { get; set; }

        /// <summary>
        /// 物料信息
        /// </summary>
        [Required]
        public virtual MaterialInfoOfStock Material { get; private set; }

        /// <summary>
        /// 数量信息
        /// </summary>
        [Required]
        public virtual CountInfoOfStock ReceiveCount { get; private set; }

        /// <summary>
        /// 检测信息
        /// </summary>
        [Required]
        public virtual CheckInfoOfStock CheckData { get; private set; }

        /// <summary>
        /// 供应商信息
        /// </summary>
        [Required]
        public virtual SupplierInfoOfStock Supplier { get; private set; }
    }
}
