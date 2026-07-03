using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Values;

namespace TuTa.Wms.ChkResultLists.ValueObjects
{
    [Owned]
    public class SupplierInfoOfChkRsltList : ValueObject
    {
        private SupplierInfoOfChkRsltList()
        {

        }

        public SupplierInfoOfChkRsltList(string supplierCode, string supplierName)
        {
            //if (supplierCode == null)
            //    SupplierCode = null;
            //else
            //{
            //    if (string.IsNullOrWhiteSpace(supplierCode))
            //        throw new Exception("供应商编号无效");

            //    SupplierCode = supplierCode;
            //}

            //if (supplierName == null)
            //    SupplierName = null;
            //else
            //{
            //    if (string.IsNullOrWhiteSpace(supplierName))
            //        throw new Exception("供应商名称无效");

            //    SupplierName = supplierName;
            //}

            SupplierCode = supplierCode;
            SupplierName = supplierName;
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

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new object[] { SupplierCode, SupplierName };
        }
    }
}
