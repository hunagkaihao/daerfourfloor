using System;

namespace TuTa.Wms.Cells.Dtos
{
    public class CellAddDto
    {
        /// <summary>
        /// 所属仓库名称
        /// </summary>
        public string WarehouseName { get; set; }

        /// <summary>
        /// 所属库区名称
        /// </summary>
        public string WarehouseAreaName { get; set; }

        /// <summary>
        /// 所属架子名称
        /// </summary>
        public string ShelfName { get; set; }

        public string CellCode { get; set; }

        public string CellName { get; set; }

        /// <summary>
        /// 库位类型
        /// </summary>
        public string CellType { get; set; }

        /// <summary>
        /// 可存放的容器规格名称，以半角逗号分隔
        /// </summary>
        public string AvailableBoxSpecsNames { get; set; }

        /// <summary>
        /// 可存放的料车，以半角逗号分隔
        /// </summary>
        public string AvailableSkipSpecsNames { get; set; }

        /// <summary>
        /// 巷到列
        /// </summary>
        public string LaneToColumn { get; set; }

        /// <summary>
        /// 巷道位
        /// </summary>
        public int? LanePosition { get; set; }
    }
}
