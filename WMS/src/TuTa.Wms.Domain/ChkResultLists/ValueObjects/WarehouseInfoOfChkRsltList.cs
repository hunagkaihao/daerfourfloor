using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace TuTa.Wms.ChkResultLists.ValueObjects
{
    [Owned]
    public class WarehouseInfoOfChkRsltList : ValueObject
    {
        private WarehouseInfoOfChkRsltList()
        {

        }

        // 为了判断仓库和库位的有效性，只能在邻域服务中创建，仓库信息不能为空且不能修改
        public WarehouseInfoOfChkRsltList(
            string targetWarehouseCode,
            string targetWarehouseName)
        {
            Check.NotNullOrWhiteSpace(targetWarehouseCode, nameof(targetWarehouseCode));
            Check.NotNullOrWhiteSpace(targetWarehouseName, nameof(targetWarehouseName));
            WarehouseCodeCheck(targetWarehouseCode);

            TargetWarehouseCode = targetWarehouseCode;
            TargetWarehouseName = targetWarehouseName;
        }

        /// <summary>
        /// 收料仓编号
        /// </summary>
        [StringLength(10)]
        [Required]
        public virtual string TargetWarehouseCode { get; private set; }

        /// <summary>
        /// 收料仓名称
        /// </summary>
        [StringLength(30)]
        [Required]
        public virtual string TargetWarehouseName { get; private set; }


        private void WarehouseCodeCheck(string warehouseCode)
        {
            if (warehouseCode != "01" && warehouseCode != "26" && warehouseCode != "04")
                throw new Exception($"收料仓库编号{warehouseCode}无效，可取值为01（综合库）或26（暂存库）或 04（待处理库）");
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new object[] { TargetWarehouseCode, TargetWarehouseName };
        }
    }
}
