using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Values;

namespace TuTa.Wms.StockInHistories.ValueObjects
{
    [Owned]
    public class MaterialInfoOfStockInHistory : ValueObject
    {
        private MaterialInfoOfStockInHistory()
        {            
        }

        public MaterialInfoOfStockInHistory(string materialCode, string materialName, string specs, string unit)
        {
            Code = materialCode;
            Name = materialName;
            Specs = specs;
            Unit = unit;
        }

        /// <summary>
        /// 物料码
        /// </summary>
        [StringLength(20)]
        public string Code { get; private set; }

        /// <summary>
        /// 物料名
        /// </summary>
        [StringLength(120)]
        public string Name { get; private set; }

        /// <summary>
        /// 物料规格
        /// </summary>
        [StringLength(120)]
        public string Specs { get; private set; }

        /// <summary>
        /// 物料单位
        /// </summary>
        [StringLength(10)]
        public string Unit { get; private set; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return [Code, Name, Specs, Unit];
        }
    }
}
