using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace TuTa.Wms.Erp.Aggregates
{
    /// <summary>
    /// ERP工位收料聚合根
    /// </summary>
    public class ErpWorkstationMaterialReceipt : AuditedAggregateRoot<Guid>
    {
        /// <summary>
        /// 分拣批次号
        /// </summary>
        public string SortingBatch { get; private set; }

        /// <summary>
        /// 收料时间
        /// </summary>
        public DateTime ReceiptTime { get; private set; }

        /// <summary>
        /// 私有构造函数，防止外部直接实例化
        /// </summary>
        private ErpWorkstationMaterialReceipt() { }

        /// <summary>
        /// 创建工位收料记录
        /// </summary>
        /// <param name="sortingBatch">分拣批次号</param>
        /// <param name="receiptTime">收料时间</param>
        /// <returns>工位收料记录</returns>
        public static ErpWorkstationMaterialReceipt Create(string sortingBatch, DateTime receiptTime)
        {
            if (string.IsNullOrWhiteSpace(sortingBatch))
            {
                throw new ArgumentException("分拣批次号不能为空", nameof(sortingBatch));
            }

            if (receiptTime == default)
            {
                throw new ArgumentException("收料时间不能为空", nameof(receiptTime));
            }

            return new ErpWorkstationMaterialReceipt
            {
                SortingBatch = sortingBatch,
                ReceiptTime = receiptTime
            };
        }

        /// <summary>
        /// 更新收料时间
        /// </summary>
        /// <param name="receiptTime">新的收料时间</param>
        public void UpdateReceiptTime(DateTime receiptTime)
        {
            if (receiptTime == default)
            {
                throw new ArgumentException("收料时间不能为空", nameof(receiptTime));
            }

            ReceiptTime = receiptTime;
        }
    }
}
