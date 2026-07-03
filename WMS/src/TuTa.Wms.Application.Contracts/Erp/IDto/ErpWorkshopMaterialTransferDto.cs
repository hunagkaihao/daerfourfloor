using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.Erp.Dto
{
    /// <summary>
    /// ERP车间物料转移请求DTO
    /// </summary>
    public class ErpWorkshopMaterialTransferRequestDto
    {
        /// <summary>
        /// 启动位置
        /// </summary>
        [Required(ErrorMessage = "启动位置不能为空")]
        [StringLength(100, ErrorMessage = "启动位置长度不能超过100个字符")]
        public string StartLocation { get; set; }

        /// <summary>
        /// 终点位置
        /// </summary>
        [Required(ErrorMessage = "终点位置不能为空")]
        [StringLength(100, ErrorMessage = "终点位置长度不能超过100个字符")]
        public string EndLocation { get; set; }
    }

    /// <summary>
    /// ERP车间物料转移响应DTO
    /// </summary>
    public class ErpWorkshopMaterialTransferResponseDto
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Succeed { get; set; }

        /// <summary>
        /// 响应消息
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// ERP车间物料转移DTO
    /// </summary>
    public class ErpWorkshopMaterialTransferDto : AuditedEntityDto<Guid>
    {
        /// <summary>
        /// 启动位置
        /// </summary>
        public string StartLocation { get; set; }

        /// <summary>
        /// 终点位置
        /// </summary>
        public string EndLocation { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 状态描述
        /// </summary>
        public string StatusDescription { get; set; }
    }

    /// <summary>
    /// ERP车间物料转移查询DTO
    /// </summary>
    public class ErpWorkshopMaterialTransferQueryDto : PagedAndSortedResultRequestDto
    {
        /// <summary>
        /// 启动位置
        /// </summary>
        public string StartLocation { get; set; }

        /// <summary>
        /// 终点位置
        /// </summary>
        public string EndLocation { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// 创建时间开始
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 创建时间结束
        /// </summary>
        public DateTime? EndTime { get; set; }
    }

    /// <summary>
    /// ERP车间物料转移状态更新DTO
    /// </summary>
    public class ErpWorkshopMaterialTransferStatusUpdateDto
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        [Required(ErrorMessage = "任务ID不能为空")]
        public Guid Id { get; set; }

        /// <summary>
        /// 新状态
        /// </summary>
        [Required(ErrorMessage = "状态不能为空")]
        [Range(0, 4, ErrorMessage = "状态值必须在0-4之间")]
        public int Status { get; set; }
    }
}
