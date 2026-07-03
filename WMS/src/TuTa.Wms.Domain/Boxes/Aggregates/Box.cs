using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using TuTa.Wms.Boxes.Entities;
using TuTa.Wms.Boxes.Events;
using TuTa.Wms.Boxes.ValueObjects;
using TuTa.Wms.Cells.Aggregates;
using TuTa.Wms.Warehouses.Aggregates;
using TuTa.Wms.Warehouses.Entities;

using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.VirtualFileSystem;

namespace TuTa.Wms.Boxes.Aggregates
{
    public class Box : AuditedAggregateRoot<Guid>
    {
        private Box()
        {
            
        }

        internal Box(Guid id, string boxCode, string boxName, string boxTypeName, BoxSpecsValObj boxSpecs)
            : base(id)
        {
            BoxCode = Check.NotNullOrWhiteSpace(boxCode, nameof(boxCode));
            BoxName = Check.NotNullOrWhiteSpace(boxName, nameof(boxName));
            BoxSpecs = Check.NotNull(boxSpecs, nameof(boxSpecs));
            BoxTypeName = WmsDomainHelper.NotWhiteSpaceCheck(boxTypeName, nameof(boxTypeName));
            
            Status = BoxStatus.NoHave;
            CellData = new CellInfo(null, null, null); //新创建的容器不会在库位中
            WarehouseData = new WarehouseInfo(null, null, null, null, null, null);

            StocksInBox = new List<BoxStock>();
        }

        internal void ModifyBox(string boxCodeNew, string boxNameNew, string boxTypeNameNew, BoxSpecsValObj boxSpecsNew)
        {
            BoxCode = Check.NotNullOrWhiteSpace(boxCodeNew, nameof(boxCodeNew));
            BoxName = Check.NotNullOrWhiteSpace(boxNameNew, nameof(boxNameNew));
            BoxSpecs = Check.NotNull(boxSpecsNew, nameof(boxSpecsNew));
            if (boxTypeNameNew == null) BoxTypeName = null;
            else BoxTypeName = Check.NotNullOrWhiteSpace(boxTypeNameNew, nameof(boxTypeNameNew));
        }

        public void AddStock(BoxStock boxStock)
        {
            if(StocksInBox == null)
                StocksInBox = new List<BoxStock>();

            Check.NotNull(boxStock, nameof(boxStock));
            if(Id != boxStock.BoxId)
                throw new Exception("新增库存的所属容器Id非当前容器的Id");

            //同一个库存，不能多次添加
            if (StocksInBox.Where(o => o.StockId == boxStock.StockId).Count() > 0)
                throw new Exception($"当前容器中已经存在Id为{boxStock.StockId}的库存");

            //同一个收料条形码，需要对库存进行合并，保留原来的库存
            if (StocksInBox.Where(o => o.StockBarcode == boxStock.StockBarcode).Count() > 0)
                return;

            StocksInBox.Add(boxStock);
            Status = BoxStatus.Have;
        }

        public void RemoveStock(Guid stockId)
        {
            if (StocksInBox == null)
                StocksInBox = new List<BoxStock>();

            var result = StocksInBox.Where(o => o.StockId == stockId).ToList();
            if (result.Count() > 0)
            {
                //    throw new Exception($"容器中不存在Id为{stockId}的库存，无法扣除该库存");
                //}

                foreach (BoxStock stock in result)
                {
                    StocksInBox.Remove(stock);
                }
            }

            if (StocksInBox.Count() == 0)
                Status = BoxStatus.NoHave;
        }

        /// <summary>
        /// 容器中一个收料条形码只对应一个库存
        /// </summary>
        /// <param name="stockBarcode"></param>
        /// <returns></returns>
        public BoxStock GetBoxStockByBarcode(string stockBarcode)
        {
            if (StocksInBox == null)
                StocksInBox = new List<BoxStock>();

            return StocksInBox.FirstOrDefault(o => o.StockBarcode == stockBarcode);
        }


        public virtual void BindCell(
            Guid cellId, string cellCode, string cellName,
            Guid warehouseId, string warehouseCode, string warehouseName,
            int? warehouseAreaId, string warehouseAreaCode, string warehouseAreaName)
        {
            CellData = new CellInfo(cellId, cellCode, cellName);
            WarehouseData = new WarehouseInfo(
                warehouseId, warehouseCode, warehouseName,
                warehouseAreaId, warehouseAreaCode, warehouseAreaName);

            //通知库位更新状态，以及通知库存更新状态
            BoxBindCellEvent bindEvent = new BoxBindCellEvent()
            {
                CellId = this.CellData.CellId.Value,
                CellCode = this.CellData.CellCode,
                CellName = this.CellData.CellName,
                WarehouseId = warehouseId,
                WarehouseCode = warehouseCode,
                WarehouseName = warehouseName,
                WarehouseAreaId = warehouseAreaId,
                WarehouseAreaCode = warehouseAreaCode,
                WarehouseAreaName = warehouseAreaName,


                BoxId = this.Id,
                BoxCode = this.BoxCode,
                BoxName = this.BoxName,
                BoxTypeName = this.BoxTypeName,
                SpecsName = this.BoxSpecs.SpecsName,
                Length = this.BoxSpecs.Length,
                Width = this.BoxSpecs.Width,
                Height = this.BoxSpecs.Height
            };
            AddLocalEvent(bindEvent);
        }

        public virtual void BindCell(
            Cell cell,
            Warehouse warehouse,
            WarehouseArea warehouseArea)
        {
            CellData = new CellInfo(cell.Id, cell.CellCode, cell.CellName);
            WarehouseData = new WarehouseInfo(
                warehouse.Id, warehouse.WarehouseCode, warehouse.WarehouseName,
                warehouseArea.Id, warehouseArea.WarehouseAreaCode, warehouseArea.WarehouseAreaName);

            //通知库位更新状态，以及通知库存更新状态
            BoxBindCellEvent bindEvent = new BoxBindCellEvent()
            {
                CellId = this.CellData.CellId.Value,
                CellCode = this.CellData.CellCode,
                CellName = this.CellData.CellName,
                WarehouseId = (Guid)this.WarehouseData.WarehouseId,
                WarehouseCode = this.WarehouseData.WarehouseCode,
                WarehouseName = this.WarehouseData.WarehouseName,
                WarehouseAreaId = this.WarehouseData.WarehouseAreaId,
                WarehouseAreaCode = this.WarehouseData.WarehouseAreaCode,
                WarehouseAreaName = this.WarehouseData.WarehouseAreaName,


                BoxId = this.Id,
                BoxCode = this.BoxCode,
                BoxName = this.BoxName,
                BoxTypeName = this.BoxTypeName,
                SpecsName = this.BoxSpecs.SpecsName,
                Length = this.BoxSpecs.Length,
                Width = this.BoxSpecs.Width,
                Height = this.BoxSpecs.Height
            };
            AddLocalEvent(bindEvent);
        }

        public virtual void DisBindCell()
        {
            
            if(CellData.CellId != null)
            {
                //通知库位更新状态，以及通知库存更新状态
                AddLocalEvent(new BoxDisBindCellEvent()
                {
                    BoxId = this.Id,
                    CellId = this.CellData.CellId.Value
                });

                CellData = new CellInfo(null, null, null);
            }

            WarehouseData = new WarehouseInfo(null, null, null, null, null, null);
        }

        public virtual void SetHeightWeight(decimal height,decimal weight)
        {
            Height = height;
            Weight = weight;
        }

        public virtual void SetNoHave()
        {
            Status = BoxStatus.NoHave;
            PickOutType = null;
            PickOutAreaId = null;
            PickWorkType = null;
            PickDeptName = null;
        }
        public virtual void SetHave()
        {
            Status = BoxStatus.Have;
        }


        [StringLength(20)]
        [Required]
        public virtual string BoxCode { get; private set; }

        [StringLength(50)]
        [Required]
        public virtual string BoxName { get; private set; }

        [StringLength(50)]
        public virtual string BoxTypeName { get; private set; }

        [Required]
        public virtual BoxSpecsValObj BoxSpecs { get; private set; }

        public virtual BoxStatus Status { get; private set; }

        [Required]
        public virtual CellInfo CellData { get; private set; }

        [Required]
        public virtual WarehouseInfo WarehouseData { get; set; }

        public List<BoxStock> StocksInBox { get; private set; }

        public virtual decimal Height { get; set; }

        public virtual decimal Weight { get; set; }

        public virtual string PickListCode { get; set; }

        public virtual string UniqueCode { get; set; }

        /// <summary>
        /// pick check move out
        /// </summary>
        public virtual string PickOutType { get; set; }

        public virtual string PickOutAreaId { get; set; }

        public virtual string PickWorkType { get; set; }
        public virtual string PickDeptName { get; set; }


        [Column(TypeName = "decimal(10,6)")]
        public virtual decimal? FullRate {  get; set; }
    }
}
