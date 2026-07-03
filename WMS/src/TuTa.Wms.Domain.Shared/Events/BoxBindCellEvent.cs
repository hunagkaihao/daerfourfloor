using System;
using System.Collections.Generic;
using System.Text;

namespace TuTa.Wms.Events
{
    /// <summary>
    /// 容器绑定到库位，
    /// </summary>
    public class BoxBindCellEvent
    {
        //以下Cell的信息
        public Guid CellId { get; set; }

        public string CellCode { get; set; }

        public string CellName { get; set; }

        public Guid WarehouseId { get; set; }

        public string WarehouseCode { get; set; }

        public string WarehouseName { get; set; }

        public int? WarehouseAreaId { get; set; }

        public string WarehouseAreaCode { get; set; }

        public string WarehouseAreaName { get; set; }


        //以下为Box的信息
        public Guid BoxId { get; set; }

        public string BoxCode { get; set; }

        public string BoxName { get; set; }

        public string BoxTypeName { get; set; }

        public string SpecsName { get; set; }

        public int? Length { get; set; }

        public int? Width { get; set; }

        public int? Height { get; set; }
    }
}
