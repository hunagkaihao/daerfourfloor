using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace TuTa.Wms.Erp.Aggregates
{
    /// <summary>
    /// ERP车间物料转移聚合根
    /// </summary>
    public class ErpWorkshopMaterialTransfer : AuditedAggregateRoot<Guid>
    {
        /// <summary>
        /// 启动位置
        /// </summary>
        public string StartLocation { get; private set; }

        /// <summary>
        /// 终点位置
        /// </summary>
        public string EndLocation { get; private set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public MaterialTransferStatus Status { get; private set; }

        /// <summary>
        /// 私有构造函数，防止外部直接实例化
        /// </summary>
        private ErpWorkshopMaterialTransfer() { }

        /// <summary>
        /// 创建车间物料转移任务
        /// </summary>
        /// <param name="startLocation">启动位置</param>
        /// <param name="endLocation">终点位置</param>
        /// <returns>车间物料转移任务</returns>
        public static ErpWorkshopMaterialTransfer Create(string startLocation, string endLocation)
        {
            if (string.IsNullOrWhiteSpace(startLocation))
            {
                throw new ArgumentException("启动位置不能为空", nameof(startLocation));
            }

            if (string.IsNullOrWhiteSpace(endLocation))
            {
                throw new ArgumentException("终点位置不能为空", nameof(endLocation));
            }

            if (startLocation.Equals(endLocation, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("启动位置和终点位置不能相同");
            }

            return new ErpWorkshopMaterialTransfer
            {
                StartLocation = startLocation,
                EndLocation = endLocation,
                Status = MaterialTransferStatus.Pending
            };
        }

        /// <summary>
        /// 更新任务状态
        /// </summary>
        /// <param name="status">新状态</param>
        public void UpdateStatus(MaterialTransferStatus status)
        {
            Status = status;
        }

        /// <summary>
        /// 更新位置信息
        /// </summary>
        /// <param name="startLocation">启动位置</param>
        /// <param name="endLocation">终点位置</param>
        public void UpdateLocations(string startLocation, string endLocation)
        {
            if (string.IsNullOrWhiteSpace(startLocation))
            {
                throw new ArgumentException("启动位置不能为空", nameof(startLocation));
            }

            if (string.IsNullOrWhiteSpace(endLocation))
            {
                throw new ArgumentException("终点位置不能为空", nameof(endLocation));
            }

            if (startLocation.Equals(endLocation, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("启动位置和终点位置不能相同");
            }

            StartLocation = startLocation;
            EndLocation = endLocation;
        }
    }

    /// <summary>
    /// 物料转移状态枚举
    /// </summary>
    public enum MaterialTransferStatus
    {
        /// <summary>
        /// 待处理
        /// </summary>
        Pending = 0,

        /// <summary>
        /// 执行中
        /// </summary>
        InProgress = 1,

        /// <summary>
        /// 已完成
        /// </summary>
        Completed = 2,

        /// <summary>
        /// 已取消
        /// </summary>
        Cancelled = 3,

        /// <summary>
        /// 执行失败
        /// </summary>
        Failed = 4
    }
}
