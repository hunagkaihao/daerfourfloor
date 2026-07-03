using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TuTa.Wms.Cells.Entities;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace TuTa.Wms.Cells.Aggregates
{
    public class Cell : AuditedAggregateRoot<Guid>
    {
        private Cell()
        {
            
        }

        internal Cell(
            Guid cellId,
            Guid warehouseId,
            int? warehouseAreaId,
            string shelfName,
            string cellCode,
            string cellName,
            string cellType, 
            string availableBoxSpecsNames,
            string availableSkipSpecsNames)
        {
            Id = cellId;

            WarehouseId = warehouseId;

            if (warehouseAreaId != null && warehouseAreaId <= 0)
                throw new Exception("warehouseAreaId无效");
            WarehouseAreaId = warehouseAreaId;

            ShelfName = shelfName;
            //if (shelfName == null) ShelfName = null;
            //else ShelfName = Check.NotNullOrWhiteSpace(shelfName, nameof(shelfName));

            CellCode = Check.NotNullOrWhiteSpace(cellCode, nameof(cellCode));
            CellName = Check.NotNullOrWhiteSpace(cellName, nameof(cellName));

            if (!Enum.IsDefined(typeof(CellType), cellType))
                throw new Exception("cellType值无效");

            if (!Enum.TryParse<CellType>(cellType, out CellType type))
                throw new Exception("cellType值无效");

            CellType = type;

            if (availableBoxSpecsNames != null && string.IsNullOrWhiteSpace(availableBoxSpecsNames))
                throw new Exception("availableBoxSpecsNames的值无效");

            if (availableBoxSpecsNames != null && availableBoxSpecsNames.Split(",").Length < 1)
                throw new Exception("未指定有效可存放规格");

            AvailableBoxSpecsNames = availableBoxSpecsNames;
            AvailableSkipSpecsNames = availableSkipSpecsNames;

            CellStatus = CellStatus.Nohave;
            RunStatus = CellRunStatus.Enable;

            CellBoxes = new List<CellBox>();

            CellInOut = "InOut";

            CellHeight = "small";

            CellWeight = "small";

            //发送库位被创建的事件，用于虚拟容器的创建
            //AddLocalEvent(new CellCreatedEvent()
            //{
            //    CellId = this.Id,
            //    WarehouseId = this.WarehouseId,
            //    WarehouseAreaId = this.WarehouseAreaId,
            //    ShelfName = this.ShelfName,
            //    CellCode = this.CellCode,
            //    CellName = this.CellName,
            //    CellType = this.CellType,
            //    AvailableBoxSpecsNames = this.AvailableBoxSpecsNames
            //});
        }

        public void AddBox(CellBox box)
        {
            if (CellBoxes == null) CellBoxes = new List<CellBox>();

            if (box == null) throw new ArgumentNullException(nameof(box));

            if (box.CellId != this.Id) throw new Exception($"新增的容器不属于当前库位");

            var boxesExist = CellBoxes.Where(o => o.EntityEquals(box)).ToList();
            if (boxesExist.Count > 0) throw new Exception($"Id为{box.BoxId}的容器已经在当前库位中");

            CellBoxes.Add(box);
            CellStatus = CellStatus.Have;
        }

        public void RemoveBox(Guid boxId)
        {
            if (CellBoxes == null) CellBoxes = new List<CellBox>();

            var boxesExist = CellBoxes.Where(o => o.BoxId == boxId).ToList();
            if (boxesExist.Count == 0) return;

            foreach(var b in boxesExist)
                CellBoxes.Remove(b);

            if (CellBoxes.Count == 0)
                CellStatus = CellStatus.Nohave;
        }

        public bool IsBoxInThisCell(Guid boxId)
        {
            if (CellBoxes == null) CellBoxes = new List<CellBox>();

            if (CellBoxes.Count == 0) return false;
            var boxesExist = CellBoxes.Where(o => o.BoxId == boxId).ToList();
            return boxesExist.Count > 0;
        }

        public void BindToWarehouseArea(int areaId)
        {
            WarehouseAreaId = areaId;
        }

        public void DisBindFromWarehouseArea()
        {
            WarehouseAreaId = null;
        }

        public void SetSelected()
        {
            RunStatus = CellRunStatus.Selected;
        }
        public void SetEnable()
        {
            RunStatus = CellRunStatus.Enable;
        }

        public void SetCellStatus(CellStatus status)
        {
            CellStatus = status;
        }

        /// <summary>
        /// 所属仓库Id
        /// </summary>
        [Required]
        public Guid WarehouseId { get; private set; }

        /// <summary>
        /// 所属库区Id
        /// </summary>
        public int? WarehouseAreaId { get; private set; }

        /// <summary>
        /// 所属架子名称
        /// </summary>
        [StringLength(30)]
        public string ShelfName { get; private set; }

        [Required]
        [StringLength(20)]
        public string CellCode { get; set; }

        [Required]
        [StringLength(50)]
        public string CellName { get; set; }

        [StringLength(20)]
        public string CellCode2 { get; private set; }

        [StringLength(50)]
        public string CellName2 { get; private set; }

        /// <summary>
        /// 库位类型
        /// </summary>
        public CellType CellType { get; private set; }

        /// <summary>
        /// 可存放的容器规格名称，以半角逗号分隔
        /// </summary>
        [StringLength(100)]
        public string AvailableBoxSpecsNames { get; private set; }

        /// <summary>
        /// 可存放的料车规格，以半角逗号分隔
        /// </summary>
        [StringLength(100)]
        public string AvailableSkipSpecsNames { get; private set; }

        /// <summary>
        /// 库位状态，有货、无货、满货
        /// </summary>
        public CellStatus CellStatus { get; private set; }

        /// <summary>
        /// 运行状态，禁用、可用、选定等
        /// </summary>
        public CellRunStatus RunStatus { get; private set; }

        /// <summary>
        /// 库位中包含的容器
        /// </summary>
        public List<CellBox> CellBoxes { get; private set; }

        public string CellInOut { get; private set; }

        public string CellHeight { get; private set; }
        
        public string CellWeight { get; private set; }

        /// <summary>
        /// 巷到列
        /// </summary>
        [StringLength(50)]
        public string LaneToColumn { get; set; }

        /// <summary>
        /// 巷道位
        /// </summary>
        public int? LanePosition { get; set; }

        //[StringLength(20)]
        //public string DeviceCode { get; set; }

        ///// <summary>
        ///// 排
        ///// </summary>
        //public int Cell_z { get; set; }

        ///// <summary>
        ///// 列
        ///// </summary>
        //public int Cell_x { get; set; }

        ///// <summary>
        ///// 层
        ///// </summary>
        //public int Cell_y { get; set; }

        ///// <summary>
        ///// CTU库一般为InOut  入库分拨墙设置为In  出库分拨墙设置为Out
        ///// </summary>
        //[StringLength(20)]
        //public string CellInout { get; set; }


        //[StringLength(20)]
        //public string CellModel { get; set; }

        //[StringLength(20)]
        //public string CellForkType { get; set; }

        //[StringLength(20)]
        //public string CellLogicalName { get; set; }

        //[StringLength(20)]
        //public string LaneWay { get; set; }

        //[StringLength(20)]
        //public string CellGroup { get; set; }

        //[StringLength(20)]
        //public string CellFlag { get; set; }

        //[StringLength(20)]
        //public string ShelfType { get; set; }

        //[StringLength(20)]
        //public string ShelfNeighbour { get; set; }

        ///// <summary>
        ///// 对应料箱容器类型 CtnrCode
        ///// </summary>
        //[StringLength(20)]
        //public string CellStorageType { get; set; }

        //public int CellWidth { get; set; }

        //public int CellHeight { get; set; }

        //[StringLength(20)]
        //public string LockCellId { get; set; }

        //[StringLength(20)]
        //public string BelongArea { get; set; }

        ///// <summary>
        ///// 客户自定义编码
        ///// </summary>
        //[StringLength(20)]
        //public string CustomCode { get; set; }
        ///// <summary>
        ///// 分拨墙控制器IP
        ///// </summary>
        //[StringLength(20)]
        //public string ControllerIP { get; set; }
        ///// <summary>
        ///// 分拨墙控制器端口
        ///// </summary>
        //[StringLength(20)]
        //public string ChannelPort { get; set; }
        ///// <summary>
        ///// 标签灯位置ID
        ///// </summary>
        //public int LightPosition { get; set; }
        ///// <summary>
        ///// 是否需要出库确认当位CellInout出库位，OutConfirm为1  出库需要确认
        ///// </summary>
        //public int OutConfirm { get; set; }
    }
}
