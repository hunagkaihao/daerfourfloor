using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace TuTa.Wms.Stocks.ValueObjects
{
    /// <summary>
    /// 物料信息生成后不再修改
    /// </summary>
    [Owned]
    public class MaterialInfoOfStock : ValueObject
    {
        private MaterialInfoOfStock()
        {

        }

        public MaterialInfoOfStock(string materialCode, string materialName, string specs, string unit,string finGoodsList)
        {
            Check.NotNullOrWhiteSpace(materialCode, nameof(materialCode));
            Check.NotNullOrWhiteSpace(materialName, nameof(materialName));
            //Check.NotNullOrWhiteSpace(specs, nameof(specs)); //specs从测试数据中看到是可以为empty的
            Check.NotNullOrWhiteSpace(unit, nameof(unit));

            MaterialCode = materialCode;
            MaterialName = materialName;
            Specs = specs;
            Unit = unit;
            FinGoodsList = finGoodsList;
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
        [Required]
        public virtual string Unit { get; private set; }

        /// <summary>
        /// 成品列表
        /// </summary>
        [StringLength(450)]

        public virtual string FinGoodsList {  get; private set; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new object[] { MaterialCode, MaterialName, Specs, Unit };
        }
    }
}
