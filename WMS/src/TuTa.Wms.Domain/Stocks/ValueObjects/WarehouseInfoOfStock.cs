using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Values;

namespace TuTa.Wms.Stocks.ValueObjects
{
    [Owned]
    public class WarehouseInfoOfStock : ValueObject
    {
        private WarehouseInfoOfStock()
        {

        }

        // 为了判断仓库和库位的有效性，只能在邻域服务中创建，仓库信息不能为空且不能修改
        public WarehouseInfoOfStock(
            Guid? warehouseId,
            string warehouseCode,
            string warehouseName,
            int? areaId,
            string areaCode,
            string areaName)
        {
            HouseId = warehouseId;
            HouseCode = WmsDomainHelper.NotWhiteSpaceCheck(warehouseCode, nameof(warehouseCode));
            HouseName = WmsDomainHelper.NotWhiteSpaceCheck(warehouseName, nameof(warehouseName));
            AreaId = WmsDomainHelper.NotNegativeOrZeroCheck(areaId, nameof(areaId));
            AreaCode = WmsDomainHelper.NotWhiteSpaceCheck(areaCode, nameof(areaCode));
            AreaName = WmsDomainHelper.NotWhiteSpaceCheck(areaName, nameof(areaName));
        }

        /// <summary>
        /// 仓库Id
        /// </summary>
        public virtual Guid? HouseId { get; set; }

        /// <summary>
        /// 仓库编号
        /// </summary>
        [StringLength(20)]
        //[Required]
        public virtual string HouseCode { get; private set; }

        /// <summary>
        /// 仓库名称
        /// </summary>
        [StringLength(50)]
        //[Required]
        public virtual string HouseName { get; private set; }

        /// <summary>
        /// 库区Id
        /// </summary>
        public int? AreaId { get; set; }

        /// <summary>
        /// 库区编号
        /// </summary>
        [StringLength(20)]
        public string AreaCode { get; set; }

        /// <summary>
        /// 库区名称
        /// </summary>
        [StringLength(50)]
        public string AreaName { get; set; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new object[] { HouseId, HouseCode, HouseName, AreaId, AreaCode, AreaName };
        }
    }
}
