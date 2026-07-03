using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Volo.Abp.Domain.Values;

namespace TuTa.Wms.BarcodeLists.ValueObjects
{
    [Owned]
    public class SupplierInfoOfBarcodeList:ValueObject
    {
        private SupplierInfoOfBarcodeList() { }

        public SupplierInfoOfBarcodeList(string supplierCode, string supplierName, string supplierBatchCode)
        {
            SupplierCode = supplierCode;
            SupplierName = supplierName;
            SupplierBatchCode = supplierBatchCode;
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
