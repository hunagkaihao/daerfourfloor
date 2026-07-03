using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.Cells.Dtos
{
    public class CellDto : EntityDto<Guid>
    {
        public string WarehouseName { get; set; }

        /// <summary>
        /// 所属库区Id
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
        /// 库位状态，有货、无货、满货
        /// </summary>
        public string CellStatus { get; set; }

        /// <summary>
        /// 运行状态，禁用、可用、选定等
        /// </summary>
        public string RunStatus { get; set; }

        public string isHeigh {  get; set; }
        public string isWeight { get; set; }
        public string StartCellCode { get; set; }
        public string BoxCode { get; set; }

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
