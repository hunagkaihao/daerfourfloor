using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace TuTa.Wms.PickLists.Entities
{
    /// <summary>
    /// 领料来源库存
    /// </summary>
    public class PickStock : Entity<int>
    {
        private PickStock()
        {            
        }

        internal PickStock(
            //Guid pickListId,
            int pickItemId,
            Guid stockId,
            decimal pickCount)
        {
            //PickListId = pickListId;
            PickItemId = Check.Positive(pickItemId, nameof(pickItemId));
            StockId = stockId;
            PickCount = pickCount;
            PickedCount = 0;
            CreateTime = DateTime.Now;
            CurrentTime = DateTime.Now;
        }

        public void PickAway(decimal pickAwayCount)
        {
            Check.Positive(pickAwayCount, nameof(pickAwayCount));
            //if (PickedCount != 0)
            //    throw new Exception($"Id为{PickItemId}的领用项从库存{StockId}中重复领料");
            //if (pickAwayCount != PickCount)
            //    throw new Exception($"Id为{PickItemId}的领用项从库存{StockId}中需要领用{PickCount}，但准备领用的数量为{pickAwayCount}");
            if (pickAwayCount + PickedCount > PickCount)
                throw new Exception($"新领取数量{pickAwayCount}后，总领取数量为{pickAwayCount + PickedCount}，超过了建议的领取数量{PickCount}");
            PickedCount += pickAwayCount;
        }

        /// <summary>
        /// 超时检测，若超时，返回true，没有超时，返回false
        /// </summary>
        /// <returns></returns>
        internal bool OverTimeCheck()
        {
            if (CreateTime == null)
                CreateTime = DateTime.Now;
            CurrentTime = DateTime.Now;

            TimeSpan difference = CurrentTime.Value - CreateTime.Value;
            var totalMin = difference.TotalMinutes;
            return totalMin >= 10;
        }

        /// <summary>
        /// 针对的领用单
        /// </summary>
        //public Guid PickListId { get; private set; }

        /// <summary>
        /// 针对的领用项
        /// </summary>
        public int PickItemId { get; private set; }

        /// <summary>
        /// 从哪个库存中领用
        /// </summary>
        public Guid StockId { get; private set; }

        /// <summary>
        /// 领用物料的领用数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal PickCount { get; private set; }

        /// <summary>
        /// 已领用数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal PickedCount { get; private set; }

        /// <summary>
        /// 创建的时间
        /// </summary>
        [Column(TypeName = "datetime")]
        public DateTime? CreateTime { get; private set; }

        /// <summary>
        /// 当前的时间，减去创建的时间后，若超过默认的10分钟，销毁该对象
        /// </summary>
        public DateTime? CurrentTime { get; private set; }
    }
}
