using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using TuTa.Wms.Warehouses.Entities;

namespace TuTa.Wms.Warehouses.Aggregates
{
    public class Warehouse : AuditedAggregateRoot<Guid>
    {
        /// <summary>
        /// 仓库基础信息表
        /// </summary>
        private Warehouse()
        {
        }

        internal Warehouse(
            Guid id,
            string warehouseCode,
            string warehouseName,
            WarehouseType warehouseType,
            string warehouseRemark = null,
            string warehouseFlag = null,
            string warehouseOrder = null)
            : base(id)
        {
            WarehouseCode = Check.NotNullOrWhiteSpace(warehouseCode, nameof(warehouseCode));
            WarehouseName = Check.NotNullOrWhiteSpace(warehouseName, nameof(warehouseName));
            WarehouseType = warehouseType;

            WarehouseRemark = warehouseRemark;

            if (warehouseFlag == null)
                WarehouseFlag = null;
            else
            {
                if (string.IsNullOrWhiteSpace(warehouseFlag))
                    throw new Exception("warehouseFlag无效");
                WarehouseFlag = warehouseFlag;
            }

            if (warehouseOrder == null)
                WarehouseOrder = null;
            else
            {
                if (string.IsNullOrWhiteSpace(warehouseOrder))
                    throw new Exception("warehouseOrder无效");
                WarehouseOrder = warehouseOrder;
            }

            WarehouseAreas = new List<WarehouseArea>();
        }

        internal void Update(
            string warehouseCode,
            string warehouseName,
            WarehouseType warehouseType,
            string warehouseRemark,
            string warehouseFlag,
            string warehouseOrder)
        {
            WarehouseCode = Check.NotNullOrWhiteSpace(warehouseCode, nameof(warehouseCode));
            WarehouseName = Check.NotNullOrWhiteSpace(warehouseName, nameof(warehouseName));
            WarehouseType = warehouseType;

            WarehouseRemark = warehouseRemark;

            if (warehouseFlag == null)
                WarehouseFlag = null;
            else
            {
                if (string.IsNullOrWhiteSpace(warehouseFlag))
                    throw new Exception("warehouseFlag无效");
                WarehouseFlag = warehouseFlag;
            }

            if (warehouseOrder == null)
                WarehouseOrder = null;
            else
            {
                if (string.IsNullOrWhiteSpace(warehouseOrder))
                    throw new Exception("warehouseOrder无效");
                WarehouseOrder = warehouseOrder;
            }
        }

        public void AddArea(
            string warehouseAreaCode,
            string warehouseAreaName,
            //WarehouseAreaType warehouseAreaType,
            string warehouseAreaRemark = null,
            string warehouseAreaFlag = null,
            string warehouseAreaOrder = null,
            string warehouseAreaGroup = null)
        {
            if (WarehouseAreas == null) WarehouseAreas = new List<WarehouseArea>();

            WarehouseArea area = new WarehouseArea(
                Id,
                warehouseAreaCode,
                warehouseAreaName,
                //warehouseAreaType, 
                warehouseAreaRemark,
                warehouseAreaFlag,
                warehouseAreaOrder,
                warehouseAreaGroup);

            var areasExist = WarehouseAreas.Where(
                o => o.WarehouseAreaCode == area.WarehouseAreaCode ||
                o.WarehouseAreaName == area.WarehouseAreaName).ToList();

            if (areasExist.Count > 0)
                throw new Exception($"区域名{area.WarehouseAreaName}或区域码{area.WarehouseAreaCode}已经存在");

            WarehouseAreas.Add(area);
        }

        public void RemoveArea(int warehouseAreaId)
        {
            if (WarehouseAreas == null) WarehouseAreas = new List<WarehouseArea>();

            var area = WarehouseAreas.FirstOrDefault(o => o.Id == warehouseAreaId);
            if (area == null)
                return;

            WarehouseAreas.Remove(area);
        }

        public void ModifyArea(
            int warehouseAreaId,
            string areaCodeNew,
            string areaNameNew,
            string areaRemarkNew,
            string areaFlagNew,
            string areaOrderNew,
            string areaGroupNew)
        {
            if (WarehouseAreas == null) WarehouseAreas = new List<WarehouseArea>();

            int index = -1;
            for (int i = 0; i < WarehouseAreas.Count; i++)
            {
                if (WarehouseAreas[i].Id == warehouseAreaId)
                {
                    index = i;
                    break;
                }
            }
            if (index == -1)
                throw new Exception($"Id为{warehouseAreaId}的库区不存在");

            WarehouseAreas[index].Update(areaCodeNew, areaNameNew, areaRemarkNew, areaFlagNew, areaOrderNew, areaGroupNew);
        }

        /// <summary>
        /// 根据库区Id查询库区
        /// </summary>
        /// <param name="areaCode"></param>
        /// <returns></returns>
        public WarehouseArea GetAreaByAreaId(int areaId)
        {
            if (WarehouseAreas == null) WarehouseAreas = new List<WarehouseArea>();

            foreach (var area in WarehouseAreas)
            {
                if (area.Id == areaId)
                    return area;
            }
            return null;
        }

        /// <summary>
        /// 根据库区码查询库区
        /// </summary>
        /// <param name="areaCode"></param>
        /// <returns></returns>
        public WarehouseArea GetAreaByAreaCode(string areaCode)
        {
            if (WarehouseAreas == null) WarehouseAreas = new List<WarehouseArea>();

            foreach (var area in WarehouseAreas)
            {
                if (area.WarehouseAreaCode.Equals(areaCode))
                    return area;
            }
            return null;
        }

        /// <summary>
        /// 根据库区名查询库区
        /// </summary>
        /// <param name="areaCode"></param>
        /// <returns></returns>
        public WarehouseArea GetAreaByAreaName(string areaName)
        {
            if (WarehouseAreas == null) WarehouseAreas = new List<WarehouseArea>();

            foreach (var area in WarehouseAreas)
            {
                if (area.WarehouseAreaName.Equals(areaName))
                    return area;
            }
            return null;
        }

        public List<WarehouseArea> GetAreasForSkip()
        {
            return WarehouseAreas.Where(t => t.WarehouseAreaGroup == "车间" || t.WarehouseAreaName == "周转区" || t.WarehouseAreaName == "入库区").ToList();
        }

        public List<WarehouseArea> GetAreasForWarehouse()
        {
            return WarehouseAreas.Where(t => t.WarehouseAreaGroup == "仓库").ToList();
        }

        public List<WarehouseArea> GetAreas()
        {
            return WarehouseAreas.Where(t => t.WarehouseAreaGroup.IsNullOrEmpty() && t.WarehouseAreaName != "料车").ToList();
        }

        public List<WarehouseArea> GetAreasForMove()
        {
            return WarehouseAreas.Where(t => t.WarehouseAreaGroup == "仓库" || t.WarehouseAreaGroup == "车间").ToList();
        }

        /// <summary>
        /// 仓库编码
        /// </summary>
        [StringLength(20)]
        public string WarehouseCode { get; private set; }
        /// <summary>
        /// 仓库名称
        /// </summary>
        [StringLength(50)]
        public string WarehouseName { get; private set; }
        /// <summary>
        /// 仓库类型
        /// </summary>
        public WarehouseType WarehouseType { get; private set; }
        /// <summary>
        /// 仓库备注
        /// </summary>
        [StringLength(200)]
        public string WarehouseRemark { get; private set; }
        /// <summary>
        /// 仓库标记
        /// </summary>
        [StringLength(20)]
        public string WarehouseFlag { get; private set; }
        /// <summary>
        /// 排序号
        /// </summary>
        [StringLength(20)]
        public string WarehouseOrder { get; private set; }

        /// <summary>
        /// 包含的仓库区域
        /// </summary>
        public List<WarehouseArea> WarehouseAreas { get; set; }
    }
}
