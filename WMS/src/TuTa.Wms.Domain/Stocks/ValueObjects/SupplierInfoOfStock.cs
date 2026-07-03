using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Values;

namespace TuTa.Wms.Stocks.ValueObjects
{
    [Owned]
    public class SupplierInfoOfStock : ValueObject
    {
        private SupplierInfoOfStock()
        {

        }

        public SupplierInfoOfStock(string supplierCode, string supplierName,string supplierBatchCode)
        {
            if (supplierCode == null)
                SupplierCode = null;
            else
            {
                if (string.IsNullOrWhiteSpace(supplierCode))
                    throw new Exception("供应商编号无效");

                SupplierCode = supplierCode;
            }

            if (supplierName == null)
                SupplierName = null;
            else
            {
                if (string.IsNullOrWhiteSpace(supplierName))
                    throw new Exception("供应商名称无效");

                SupplierName = supplierName;
            }

            if (supplierBatchCode == null)
                SupplierBatchCode = null;
            else
            {
                SupplierBatchCode = supplierBatchCode;
            }
        }

        /// <summary>
        /// 供应商编号
        /// </summary>
        [StringLength(20)]
        public virtual string SupplierCode { get; private set; }


        /// <summary>
        /// 供应商名称
        /// </summary>
        [StringLength(120)]
        public virtual string SupplierName { get; private set; }

        /// <summary>
        /// 供应商生产批号(目前只有压电片有)
        /// </summary>
        [StringLength(40)]
        public virtual string SupplierBatchCode { get; private set; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new object[] { SupplierCode, SupplierName, SupplierBatchCode };
        }
    }
}
