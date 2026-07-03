using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.Erp.Dto
{
    /// <summary>
    /// ERP工位叫料任务接口请求DTO
    /// </summary>
    public class ErpWorkstationMaterialRequestRequestDto
    {
        /// <summary>
        /// 分拣批次
        /// </summary>
        [Required(ErrorMessage = "分拣批次不能为空")]
        public string SortingBatch { get; set; }

        /// <summary>
        /// 配送点位置（即车间的运送点）
        /// </summary>
        [Required(ErrorMessage = "配送点位置不能为空")]
        public string DeliveryPointLocation { get; set; }

        /// <summary>
        /// 配送时间
        /// </summary>
        [Required(ErrorMessage = "配送时间不能为空")]
        public DateTime DeliveryTime { get; set; }
    }

    /// <summary>
    /// ERP工位叫料任务接口响应DTO
    /// </summary>
    public class ErpWorkstationMaterialRequestResponseDto
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
    /// ERP工位叫料任务DTO
    /// </summary>
    public class ErpWorkstationMaterialRequestDto : AuditedEntityDto<Guid>
    {
        /// <summary>
        /// 分拣批次
        /// </summary>
        public string SortingBatch { get; set; }

        /// <summary>
        /// 配送点位置
        /// </summary>
        public string DeliveryPointLocation { get; set; }

        /// <summary>
        /// 配送时间
        /// </summary>
        public DateTime DeliveryTime { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 任务状态描述
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 开始处理时间
        /// </summary>
        public DateTime? ProcessingStartTime { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime? CompletedTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }
    }

    /// <summary>
    /// ERP工位叫料任务查询DTO
    /// </summary>
    public class ErpWorkstationMaterialRequestQueryDto : PagedAndSortedResultRequestDto
    {
        /// <summary>
        /// 分拣批次（可选）
        /// </summary>
        public string SortingBatch { get; set; }

        /// <summary>
        /// 配送点位置（可选）
        /// </summary>
        public string DeliveryPointLocation { get; set; }

        /// <summary>
        /// 任务状态（可选）
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// 开始时间（可选）
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间（可选）
        /// </summary>
        public DateTime? EndTime { get; set; }
    }

    /// <summary>
    /// ERP工位叫料任务状态更新DTO
    /// </summary>
    public class ErpWorkstationMaterialRequestStatusUpdateDto
    {
        /// <summary>
        /// 新状态
        /// </summary>
        [Required(ErrorMessage = "任务状态不能为空")]
        public int Status { get; set; }

        /// <summary>
        /// 备注（可选）
        /// </summary>
        public string Remarks { get; set; }
    }
}
