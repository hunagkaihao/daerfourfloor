using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace TuTa.Wms.BarcodeLists.ValueObjects
{
    [Owned]
    public class MaterialInfoOfBarcodeList:ValueObject
    {
        private MaterialInfoOfBarcodeList()
        {

        }

        public MaterialInfoOfBarcodeList(string materialCode, string materialName, string specs, string unit)
        {
            Check.NotNullOrWhiteSpace(materialCode, nameof(materialCode));
            Check.NotNullOrWhiteSpace(materialName, nameof(materialName));

            MaterialCode = materialCode;
            MaterialName = materialName;
            Specs = specs;
            Unit = unit;
        }

        /// <summary>
        /// 物料码
        /// </summary>
        [StringLength(20)]
        [Required]
        public virtual string MaterialCode { get; private set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        [StringLength(120)]
        [Required]
        public virtual string MaterialName { get; private set; }

        /// <summary>
        /// 规格特性
        /// </summary>
        [StringLength(120)]
        public virtual string Specs { get; private set; }

        /// <summary>
        /// 计量单位
        /// </summary>
        [StringLength(10)]
        public virtual string Unit { get; private set; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new object[] { MaterialCode, MaterialName, Specs, Unit };
        }
    }
}
