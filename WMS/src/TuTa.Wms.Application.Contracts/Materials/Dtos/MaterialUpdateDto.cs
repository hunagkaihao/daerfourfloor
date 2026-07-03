namespace TuTa.Wms.Materials.Dtos
{
    public class MaterialUpdateDto
    {
        /// <summary>
        /// 物料码
        /// </summary>
        public string MaterialCodeNew { get; set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        public string MaterialNameNew { get; set; }

        /// <summary>
        /// 规格特性
        /// </summary>
        public string SpecsNew { get; set; }

        /// <summary>
        /// 计量单位
        /// </summary>
        public string UnitNew { get; set; }

        /// <summary>
        /// 物料类型码
        /// </summary>
        public string TypeCodeNew { get; set; }

        /// <summary>
        /// 物料类型名
        /// </summary>
        public string TypeNameNew { get; set; }

        /// <summary>
        /// 是否环保，可取值：符合H，符合L，符合W，符合R，不符合
        /// </summary>
        public string IsHBNew { get; set; }

        /// <summary>
        /// 安全库存
        /// </summary>
        public int SafetyStockNew { get; set; }

        /// <summary>
        /// 满箱数量
        /// </summary>
        public decimal FullBoxCount {  get; set; }

        /// <summary>
        /// 保质期
        /// </summary>
        public int ExpiryDateNew { get; set; }

        /// <summary>
        /// 是否汽车配件
        /// </summary>
        public bool IsQCPJNew { get; set; }

        /// <summary>
        /// 是否符合PPAP
        /// </summary>
        public bool IsPPAPNew { get; set; }
    }
}
