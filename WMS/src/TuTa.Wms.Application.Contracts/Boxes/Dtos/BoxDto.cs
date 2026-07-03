using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.Boxes.Dtos
{
    public class BoxDto : EntityDto<Guid>
    {
        public string BoxCode { get; set; }

        public string BoxName { get; set; }

        public string BoxTypeName { get; set; }

        public string SpecsName { get; set; }

        public int? Length { get; set; }

        public int? Width { get; set; }

        public int? Height { get; set; }

        public string Status { get; set; }

        public string CellName { get; set; }

        public string WarehouseAreaName { get; set; }

        public string WarehouseName { get; set; }
    }
}
