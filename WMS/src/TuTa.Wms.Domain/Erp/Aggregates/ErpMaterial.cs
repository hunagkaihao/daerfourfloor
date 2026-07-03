using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace TuTa.Wms.Erp.Aggregates
{
    /// <summary>
    /// ERP物料聚合根
    /// </summary>
    public class ErpMaterial : AuditedAggregateRoot<Guid>
    {
        private ErpMaterial()
        {
        }

        internal ErpMaterial(
            Guid id,
            string materialCode,
            string materialName,
            string unitCode,
            long syncTimeStamp,
            string syncType)
        {
            Check.NotNullOrWhiteSpace(materialCode, nameof(materialCode));
            Check.NotNullOrWhiteSpace(materialName, nameof(materialName));
            Check.NotNullOrWhiteSpace(unitCode, nameof(unitCode));
            Check.NotNullOrWhiteSpace(syncType, nameof(syncType));

            Id = id;
            MaterialCode = materialCode;
            MaterialName = materialName;
            UnitCode = unitCode;
            SyncTimeStamp = syncTimeStamp;
            SyncType = syncType;
        }

        /// <summary>
        /// 更新物料信息
        /// </summary>
        internal void Update(
            string materialName,
            string unitCode,
            long syncTimeStamp,
            string syncType)
        {
            Check.NotNullOrWhiteSpace(materialName, nameof(materialName));
            Check.NotNullOrWhiteSpace(unitCode, nameof(unitCode));
            Check.NotNullOrWhiteSpace(syncType, nameof(syncType));

            MaterialName = materialName;
            UnitCode = unitCode;
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
        /// 材料代号
        /// </summary>
        [StringLength(50)]
        [Required]
        public virtual string MaterialCode { get; private set; }

        /// <summary>
        /// 材料名称
        /// </summary>
        [StringLength(100)]
        [Required]
        public virtual string MaterialName { get; private set; }

        /// <summary>
        /// 单位
        /// </summary>
        [StringLength(20)]
        [Required]
        public virtual string UnitCode { get; private set; }

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
