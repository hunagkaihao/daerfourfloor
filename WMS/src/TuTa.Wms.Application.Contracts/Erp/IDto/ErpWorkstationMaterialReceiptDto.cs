using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.Erp.Dto
{
    /// <summary>
    /// ERP工位收料接口请求DTO
    /// </summary>
    public class ErpWorkstationMaterialReceiptRequestDto
    {
        /// <summary>
        /// 分拣批次号
        /// </summary>
        [Required(ErrorMessage = "分拣批次号不能为空")]
        public string SortingBatch { get; set; }

        /// <summary>
        /// 收料时间
        /// </summary>
        [Required(ErrorMessage = "收料时间不能为空")]
        public DateTime ReceiptTime { get; set; }
    }

    /// <summary>
    /// ERP工位收料接口响应DTO
    /// </summary>
    public class ErpWorkstationMaterialReceiptResponseDto
    {
        /// <summary>
        /// 成功与否
        /// </summary>
        public bool Succeed { get; set; }

        /// <summary>
        /// 响应消息
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// ERP工位收料DTO
    /// </summary>
    public class ErpWorkstationMaterialReceiptDto : AuditedEntityDto<Guid>
    {
        /// <summary>
        /// 分拣批次号
        /// </summary>
        public string SortingBatch { get; set; }

        /// <summary>
        /// 收料时间
        /// </summary>
        public DateTime ReceiptTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; }
    }

    /// <summary>
    /// ERP工位收料查询DTO
    /// </summary>
    public class ErpWorkstationMaterialReceiptQueryDto : PagedAndSortedResultRequestDto
    {
        /// <summary>
        /// 分拣批次号（可选）
        /// </summary>
        public string SortingBatch { get; set; }

        /// <summary>
        /// 开始时间（可选）
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间（可选）
        /// </summary>
        public DateTime? EndTime { get; set; }
    }
}
