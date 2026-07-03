using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace TuTa.Wms.Erp.Aggregates
{
    /// <summary>
    /// ERP收料工位聚合根
    /// </summary>
    public class ErpDeliveryStation : AuditedAggregateRoot<Guid>
    {
        private ErpDeliveryStation()
        {
        }

        internal ErpDeliveryStation(
            Guid id,
            string deliveryCode,
            long syncTimeStamp,
            string syncType)
        {
            Check.NotNullOrWhiteSpace(deliveryCode, nameof(deliveryCode));
            Check.NotNullOrWhiteSpace(syncType, nameof(syncType));

            Id = id;
            DeliveryCode = deliveryCode;
            SyncTimeStamp = syncTimeStamp;
            SyncType = syncType;
        }

        /// <summary>
        /// 更新收料工位信息
        /// </summary>
        internal void Update(
            long syncTimeStamp,
            string syncType)
        {
            Check.NotNullOrWhiteSpace(syncType, nameof(syncType));

            SyncTimeStamp = syncTimeStamp;
            SyncType = syncType;
        }

        /// <summary>
        /// 设置同步类型
        /// </summary>
        internal void SetSyncType(string syncType)
        {
            Check.NotNullOrWhiteSpace(syncType, nameof(syncType));
            SyncType = syncType;
        }

        /// <summary>
        /// 更新同步时间戳
        /// </summary>
        internal void UpdateSyncTimeStamp(long syncTimeStamp)
        {
            SyncTimeStamp = syncTimeStamp;
        }

        /// <summary>
        /// 配送位置代号
        /// </summary>
        [StringLength(50)]
        [Required]
        public virtual string DeliveryCode { get; private set; }

        /// <summary>
        /// 同步时间戳
        /// </summary>
        [Required]
        public virtual long SyncTimeStamp { get; private set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        [StringLength(20)]
        [Required]
        public virtual string SyncType { get; private set; }
    }
}
