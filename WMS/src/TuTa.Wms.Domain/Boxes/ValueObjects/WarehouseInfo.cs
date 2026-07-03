using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Values;

namespace TuTa.Wms.Boxes.ValueObjects
{
    public class WarehouseInfo : ValueObject
    {
        private WarehouseInfo()
        {
        }

        public WarehouseInfo(
            Guid? warehouseId, string warehouseCode, string warehouseName,
            int? warehouseAreaId, string warehouseAreaCode, string warehouseAreaName)
        {
            WarehouseId = warehouseId;
            WarehouseCode = WmsDomainHelper.NotWhiteSpaceCheck(warehouseCode, nameof(warehouseCode));
            WarehouseName = WmsDomainHelper.NotWhiteSpaceCheck(warehouseName, nameof(warehouseName));
            WarehouseAreaId = WmsDomainHelper.NotNegativeOrZeroCheck(warehouseAreaId, nameof(warehouseAreaId));
            WarehouseAreaCode = WmsDomainHelper.NotWhiteSpaceCheck(warehouseAreaCode, nameof(warehouseAreaCode));
            WarehouseAreaName = WmsDomainHelper.NotWhiteSpaceCheck(warehouseAreaName, nameof(warehouseAreaName));
        }


        public Guid? WarehouseId { get; private set; }

        [StringLength(20)]
        public string WarehouseCode { get; private set; }

        [StringLength(50)]
        public string WarehouseName { get; private set; }

        public int? WarehouseAreaId { get; private set; }

        [StringLength(20)]
        public string WarehouseAreaCode { get; private set; }

        [StringLength(50)]
        public string WarehouseAreaName { get; private set; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return [WarehouseId, WarehouseCode, WarehouseName, WarehouseAreaId, WarehouseAreaCode, WarehouseAreaName];
        }
    }
}
