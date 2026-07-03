using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace TuTa.Wms.Warehouses.Entities
{
    public class WarehouseArea : Entity<int>
    {
        private WarehouseArea()
        {
        }

        internal WarehouseArea(
            Guid warehouseId, 
            string warehouseAreaCode, 
            string warehouseAreaName, 
            //WarehouseAreaType warehouseAreaType,
            string warehouseAreaRemark = null,
            string warehouseAreaFlag = null,
            string warehouseAreaOrder = null,
            string warehouseAreaGroup = null)
        {
            WarehouseId = warehouseId;
            WarehouseAreaCode = Check.NotNullOrWhiteSpace(warehouseAreaCode, nameof(warehouseAreaCode));
            WarehouseAreaName = Check.NotNullOrWhiteSpace(warehouseAreaName, nameof(warehouseAreaName));
            //WarehouseAreaType = warehouseAreaType;

            if (warehouseAreaRemark.IsNullOrWhiteSpace() && warehouseAreaRemark != null)
                throw new Exception("warehouseAreaRemark值无效");

            if (warehouseAreaFlag.IsNullOrWhiteSpace() && warehouseAreaFlag != null)
                throw new Exception("warehouseAreaFlag值无效");

            if (warehouseAreaOrder.IsNullOrWhiteSpace() && warehouseAreaOrder != null)
                throw new Exception("warehouseAreaOrder值无效");

            if (warehouseAreaGroup.IsNullOrWhiteSpace() && warehouseAreaGroup != null)
                throw new Exception("warehouseAreaGroup值无效");

            WarehouseAreaRemark = warehouseAreaRemark; 
            WarehouseAreaFlag = warehouseAreaFlag;
            WarehouseAreaOrder = warehouseAreaOrder;
            WarehouseAreaGroup = warehouseAreaGroup;
        }

        internal void Update(
            string warehouseAreaCode, 
            string warehouseAreaName, 
            //WarehouseAreaType warehouseAreaType,
            string warehouseAreaRemark,
            string warehouseAreaFlag,
            string warehouseAreaOrder,
            string warehouseAreaGroup)
        {
            Check.NotNullOrWhiteSpace(warehouseAreaCode, nameof(warehouseAreaCode));
            Check.NotNullOrWhiteSpace(warehouseAreaName, nameof(warehouseAreaName));
            //WarehouseAreaType = warehouseAreaType;

            if (warehouseAreaRemark.IsNullOrWhiteSpace() && warehouseAreaRemark != null)
                throw new Exception("warehouseAreaRemark值无效");

            if (warehouseAreaFlag.IsNullOrWhiteSpace() && warehouseAreaFlag != null)
                throw new Exception("warehouseAreaFlag值无效");

            if (warehouseAreaOrder.IsNullOrWhiteSpace() && warehouseAreaOrder != null)
                throw new Exception("warehouseAreaOrder值无效");

            if (warehouseAreaGroup.IsNullOrWhiteSpace() && warehouseAreaGroup != null)
                throw new Exception("warehouseAreaGroup值无效");

            WarehouseAreaCode = warehouseAreaCode;
            WarehouseAreaName = warehouseAreaName;
            WarehouseAreaRemark = warehouseAreaRemark;
            WarehouseAreaFlag = warehouseAreaFlag;
            WarehouseAreaOrder = warehouseAreaOrder;
            WarehouseAreaGroup = warehouseAreaGroup;
        }

        /// <summary>
        /// 仓库ID
        /// </summary>
        public Guid WarehouseId { get; set; }
        /// <summary>
        /// 仓库分区编码
        /// </summary>
        [StringLength(20)]
        public string WarehouseAreaCode { get; set; }
        /// <summary>
        /// 仓库分区名称
        /// </summary>
        [StringLength(50)]
        public string WarehouseAreaName { get; set; }
        /// <summary>
        /// 仓库分区标记
        /// </summary>
        [StringLength(20)]
        public string WarehouseAreaFlag { get; set; }
        /// <summary>
        /// 仓库分区类型
        /// </summary>
        //public WarehouseAreaType WarehouseAreaType { get; set; }
        /// <summary>
        /// 仓库分区备注
        /// </summary>
        [StringLength(200)]
        public string WarehouseAreaRemark { get; set; }
        /// <summary>
        /// 排序号
        /// </summary>
        [StringLength(20)]
        public string WarehouseAreaOrder { get; set; }
        /// <summary>
        /// 仓库分区分组
        /// </summary>
        [StringLength(20)]
        public string WarehouseAreaGroup { get; set; }
    }
}
