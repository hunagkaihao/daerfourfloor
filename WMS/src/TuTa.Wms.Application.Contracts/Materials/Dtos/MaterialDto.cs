using System;
using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.Materials.Dtos
{
    public class MaterialDto : EntityDto<Guid>
    {
        /// <summary>
        /// 物料码
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        public string MaterialName { get; set; }

        /// <summary>
        /// 规格特性
        /// </summary>
        public string Specs { get; set; }

        /// <summary>
        /// 计量单位
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// 物料类型码
        /// </summary>
        public string TypeCode { get; set; }

        /// <summary>
        /// 物料类型名称
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// 是否环保，可取值：符合H，符合L，符合W，符合R，不符合
        /// </summary>
        public string IsHB { get; set; }

        /// <summary>
        /// 安全库存
        /// </summary>
        public int SafetyStock { get; set; }

        /// <summary>
        /// 保质期
        /// </summary>
        public int ExpiryDate { get; set; }

        /// <summary>
        /// 是否汽车配件
        /// </summary>
        public bool IsQCPJ { get; set; }

        /// <summary>
        /// 是否符合PPAP
        /// </summary>
        public bool IsPPAP { get; set; }
    }
}
