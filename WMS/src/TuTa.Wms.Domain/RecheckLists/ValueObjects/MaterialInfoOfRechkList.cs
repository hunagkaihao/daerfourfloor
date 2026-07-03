using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace TuTa.Wms.RecheckLists.ValueObjects
{
    public class MaterialInfoOfRechkList : ValueObject
    {
        public MaterialInfoOfRechkList(
            string materialCode,
            string materialName,
            string materialSpecs,
            string unit,
            int? expiryDays)
        {
            MaterialCode = Check.NotNullOrWhiteSpace(materialCode, nameof(materialCode));
            MaterialName = Check.NotNullOrWhiteSpace(materialName, nameof(materialName));
            if (materialSpecs != null && string.IsNullOrWhiteSpace(materialSpecs))
                throw new Exception("materialSpecs的值无效");
            if (unit != null && string.IsNullOrWhiteSpace(unit))
                throw new Exception("unit的值无效");
            if (expiryDays != null && expiryDays < 0)
                throw new Exception("expiryDays的值无效");
            MaterialSpecs = materialSpecs;
            Unit = unit;
            ExpiryDays = expiryDays;    
        }

        [StringLength(20)]
        [Required]
        public string MaterialCode { get; private set; }

        [StringLength(120)]
        [Required]
        public string MaterialName { get; private set; }

        [StringLength(120)]
        public string MaterialSpecs { get; private set; }

        [StringLength(10)]
        public string Unit { get; private set; }

        public int? ExpiryDays { get; private set; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new object[] { MaterialCode, MaterialName, MaterialSpecs, Unit, ExpiryDays };
        }
    }
}
