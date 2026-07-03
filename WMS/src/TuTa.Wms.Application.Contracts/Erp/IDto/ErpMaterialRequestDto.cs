using System.ComponentModel.DataAnnotations;

namespace TuTa.Wms.Erp.Dto
{
    /// <summary>
    /// ERP物料接口请求DTO
    /// </summary>
    public class ErpMaterialRequestDto
    {
        /// <summary>
        /// 材料代号
        /// </summary>
        [Required(ErrorMessage = "材料代号不能为空")]
        public string fGoodsCode { get; set; }

        /// <summary>
        /// 材料名称
        /// </summary>
        [Required(ErrorMessage = "材料名称不能为空")]
        public string fGoodsName { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        [Required(ErrorMessage = "单位不能为空")]
        public string fUnitCode { get; set; }

        /// <summary>
        /// 同步时间戳
        /// </summary>
        [Required(ErrorMessage = "同步时间戳不能为空")]
        public long syncTimeStamp { get; set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        [Required(ErrorMessage = "操作类型不能为空")]
        public string syncType { get; set; }
    }

    /// <summary>
    /// ERP物料接口响应DTO
    /// </summary>
    public class ErpMaterialResponseDto
    {
        /// <summary>
        /// 成功与否
        /// </summary>
        public bool Succeed { get; set; }

        /// <summary>
        /// 失败的详细信息
        /// </summary>
        public string Message { get; set; }
    }
}
