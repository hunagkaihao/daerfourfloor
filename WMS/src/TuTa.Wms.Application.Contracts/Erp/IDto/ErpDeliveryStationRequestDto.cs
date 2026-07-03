using System.ComponentModel.DataAnnotations;

namespace TuTa.Wms.Erp.Dto
{
    /// <summary>
    /// ERP收料工位接口请求DTO
    /// </summary>
    public class ErpDeliveryStationRequestDto
    {
        /// <summary>
        /// 配送位置代号
        /// </summary>
        [Required(ErrorMessage = "配送位置代号不能为空")]
        public string deliveryCode { get; set; }

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
    /// ERP收料工位接口响应DTO
    /// </summary>
    public class ErpDeliveryStationResponseDto
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
