using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace TuTa.Wms.Erp.Aggregates
{
    /// <summary>
    /// ERP工位叫料任务聚合根
    /// </summary>
    public class ErpWorkstationMaterialRequest : AuditedAggregateRoot<Guid>
    {
        private ErpWorkstationMaterialRequest()
        {
        }

        internal ErpWorkstationMaterialRequest(
            Guid id,
            string sortingBatch,
            string deliveryPointLocation,
            DateTime deliveryTime)
        {
            Check.NotNullOrWhiteSpace(sortingBatch, nameof(sortingBatch));
            Check.NotNullOrWhiteSpace(deliveryPointLocation, nameof(deliveryPointLocation));

            Id = id;
            SortingBatch = sortingBatch;
            DeliveryPointLocation = deliveryPointLocation;
            DeliveryTime = deliveryTime;
            Status = MaterialRequestStatus.Created;
            CreatedTime = DateTime.Now;
        }

        /// <summary>
        /// 分拣批次
        /// </summary>
        [Required]
        [StringLength(100)]
        public string SortingBatch { get; private set; }

        /// <summary>
        /// 配送点位置（即车间的运送点）
        /// </summary>
        [Required]
        [StringLength(200)]
        public string DeliveryPointLocation { get; private set; }

        /// <summary>
        /// 配送时间
        /// </summary>
        [Required]
        public DateTime DeliveryTime { get; private set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public MaterialRequestStatus Status { get; private set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; private set; }

        /// <summary>
        /// 开始处理时间
        /// </summary>
        public DateTime? ProcessingStartTime { get; private set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime? CompletedTime { get; private set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500)]
        public string Remarks { get; private set; }

        /// <summary>
        /// 更新任务状态
        /// </summary>
        public void UpdateStatus(MaterialRequestStatus newStatus)
        {
            if (newStatus == MaterialRequestStatus.Processing && Status == MaterialRequestStatus.Created)
            {
                ProcessingStartTime = DateTime.Now;
            }
            else if (newStatus == MaterialRequestStatus.Completed && Status != MaterialRequestStatus.Completed)
            {
                CompletedTime = DateTime.Now;
            }

            Status = newStatus;
        }

        /// <summary>
        /// 添加备注
        /// </summary>
        public void AddRemarks(string remarks)
        {
            if (!string.IsNullOrWhiteSpace(remarks))
            {
                Remarks = remarks;
            }
        }
    }

    /// <summary>
    /// 物料请求状态枚举
    /// </summary>
    public enum MaterialRequestStatus
    {
        /// <summary>
        /// 已创建
        /// </summary>
        Created = 0,

        /// <summary>
        /// 处理中
        /// </summary>
        Processing = 1,

        /// <summary>
        /// 已完成
        /// </summary>
        Completed = 2,

        /// <summary>
        /// 已取消
        /// </summary>
        Cancelled = 3,

        /// <summary>
        /// 处理失败
        /// </summary>
        Failed = 4
    }
}
