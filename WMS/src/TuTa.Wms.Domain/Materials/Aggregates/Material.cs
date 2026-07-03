using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace TuTa.Wms.Materials.Aggregates
{
    public class Material : AuditedAggregateRoot<Guid>
    {
        protected Material()
        {

        }

        internal Material(
            string materialCode,
            string materialName,
            string specs,
            string unit,
            string typeCode,
            string typeName,
            string isHB,
            decimal? safetyStock,
            decimal? fullBoxCount,
            int? expiryDate,
            bool? isQCPJ,
            bool? isPPAP,
            decimal? count,
            decimal? weight,
            string bindType,
            bool isBind,
            string finGoodsList)
        {
            Check.NotNullOrWhiteSpace(materialCode, nameof(materialCode));
            //Check.NotNullOrWhiteSpace(materialName, nameof(materialName));
            //Check.NotNullOrWhiteSpace(specs, nameof(specs));
            //Check.NotNullOrWhiteSpace(unit, nameof(unit));
            //Check.NotNullOrWhiteSpace(typeName, nameof(typeName));

            Check.NotNullOrWhiteSpace(typeCode, nameof(typeCode));
            CheckIsHB(isHB);

            if (safetyStock != null && safetyStock < 0)
                throw new Exception("safetyStock值无效");
            if (expiryDate != null && expiryDate < 0)
                throw new Exception("expiryDate值无效");

            MaterialCode = materialCode;
            MaterialName = materialName;
            Specs = specs;
            Unit = unit;
            TypeCode = typeCode;
            TypeName = typeName;
            IsHB = isHB;
            IsQCPJ = isQCPJ;
            IsPPAP = isPPAP;
            SafetyStock = safetyStock;
            ExpiryDays = expiryDate;
            FullBoxCount = fullBoxCount;
            PackingCount = count;
            PackingWeight = weight;
            BindType = bindType;
            IsBind = isBind;
            FinGoodsList = finGoodsList;
        }

        /// <summary>
        /// 程序集内可见，业务层需要修改物料信息，必须经过领域服务
        /// </summary>
        /// <param name="materialCode"></param>
        /// <param name="materialName"></param>
        /// <param name="specs"></param>
        /// <param name="unit"></param>
        /// <param name="typeCode"></param>
        /// <param name="typeName"></param>
        /// <param name="isHB"></param>
        /// <param name="safetyStock"></param>
        /// <param name="expiryDate"></param>
        /// <param name="isQCPJ"></param>
        /// <param name="isPPAP"></param>
        internal virtual void ModifyGoodsDefine(
            string materialCode,
            string materialName,
            string specs,
            string unit,
            string typeCode,
            string typeName,
            string isHB,
            decimal? safetyStock,
            decimal? fullBoxCount,
            int? expiryDate,
            bool? isQCPJ,
            bool? isPPAP,
            decimal? count,
            decimal? weight,
            string bindType,
            bool isBind,
            string finGoodsList)
        {
            Check.NotNullOrWhiteSpace(materialCode, nameof(materialCode));
            //Check.NotNullOrWhiteSpace(materialName, nameof(materialName));
            //Check.NotNullOrWhiteSpace(specs, nameof(specs));
            //Check.NotNullOrWhiteSpace(unit, nameof(unit));
            //Check.NotNullOrWhiteSpace(typeName, nameof(typeName));

            Check.NotNullOrWhiteSpace(typeCode, nameof(typeCode));
            CheckIsHB(isHB);

            if (safetyStock != null && safetyStock < 0)
                throw new Exception("safetyStock值无效");
            if (expiryDate != null && expiryDate < 0)
                throw new Exception("expiryDate值无效");

            MaterialCode = materialCode;
            MaterialName = materialName;
            Specs = specs;
            Unit = unit;
            TypeCode = typeCode;
            TypeName = typeName;
            IsHB = isHB;
            IsQCPJ = isQCPJ;
            IsPPAP = isPPAP;
            SafetyStock = safetyStock;
            ExpiryDays = expiryDate;
            FullBoxCount = fullBoxCount;
            PackingCount = count;
            PackingWeight = weight;
            BindType = bindType;
            IsBind = isBind;
            FinGoodsList = finGoodsList;
        }

        /// <summary>
        /// 环保字段有效性判断
        /// </summary>
        /// <param name="isHB"></param>
        /// <exception cref="Exception"></exception>
        internal void CheckIsHB(string isHB)
        {
            if (isHB != "符合H" && isHB != "符合L" && isHB != "符合W" && isHB != "符合R" && isHB != "不符合" && isHB != null)
                throw new Exception($"字段IsHB的值{isHB}无效");
        }

        /// <summary>
        /// 物料是否是半成品
        /// </summary>
        /// <returns></returns>
        public virtual bool IsSemiProduct()
        {
            return TypeCode.StartsWith("6");
        }

        /// <summary>
        /// 物料是否是成品
        /// </summary>
        /// <returns></returns>
        public virtual bool IsFinidhedProduct()
        {
            return TypeCode.StartsWith("7") || TypeCode.StartsWith("8");
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
        /// 物料类型码
        /// </summary>
        [StringLength(20)]
        [Required]
        public virtual string TypeCode { get; private set; }

        /// <summary>
        /// 物料类型名
        /// </summary>
        [StringLength(60)]
        [Required]
        public virtual string TypeName { get; private set; }

        /// <summary>
        /// 是否环保，可取值：符合H，符合L，符合W，符合R，不符合
        /// </summary>
        [StringLength(60)]
        public virtual string IsHB { get; private set; }

        /// <summary>
        /// 安全库存
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public virtual decimal? SafetyStock { get; private set; }

        /// <summary>
        /// 满箱数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public virtual decimal? FullBoxCount { get; private set; }

        /// <summary>
        /// 保质期
        /// </summary>
        public virtual int? ExpiryDays { get; private set; }

        /// <summary>
        /// 是否汽车配件
        /// </summary>
        public virtual bool? IsQCPJ { get; private set; }

        /// <summary>
        /// 是否符合PPAP
        /// </summary>
        public virtual bool? IsPPAP { get; private set; }

        /// <summary>
        /// 标准装箱数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public virtual decimal? PackingCount { get; private set; }

        /// <summary>
        /// 标准装箱重量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public virtual decimal? PackingWeight { get; private set; }

        /// <summary>
        /// 拼箱类别
        /// </summary>
        [StringLength(50)]
        public virtual string BindType { get; private set; }

        /// <summary>
        /// 成品列表
        /// </summary>
        [StringLength(450)]
        public virtual string FinGoodsList { get; private set; }

        public virtual bool IsBind { get; set; }
    }
}
