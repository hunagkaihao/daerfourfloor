using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace TuTa.Wms.RecheckLists.Entities
{
    public class RecheckStock : Entity<int>
    {
        private RecheckStock()
        {
        }

        internal RecheckStock(
            //Guid recheckListId,
            int recheckItemId,
            Guid stockId,
            decimal pickCount)
        {
            //RecheckListId = recheckListId;
            RecheckItemId = Check.Positive(recheckItemId, nameof(recheckItemId));
            StockId = stockId;
            PickCount = Check.Positive(pickCount, nameof(pickCount));
            PickedCount = 0;
        }

        public void pickAway(decimal pickAwayCount)
        {
            Check.Positive(pickAwayCount, nameof(pickAwayCount));
            //if (PickedCount != 0)
            //    throw new Exception($"Id为{RecheckItemId}的复检项从库存{StockId}中重复领料");
            //if (pickAwayCount != PickCount)
            //    throw new Exception($"Id为{RecheckItemId}的复检项从库存{StockId}中需要领用{PickCount}，但准备领用的数量为{pickAwayCount}");
            if (pickAwayCount + PickedCount > PickCount)
                throw new Exception($"新领取数量{pickAwayCount}后，总领取数量为{pickAwayCount + PickedCount}，超过了建议的领取数量{PickCount}");
            PickedCount = pickAwayCount;
        }

        /// <summary>
        /// 针对的复检单
        /// </summary>
        //public Guid RecheckListId { get; private set; }

        /// <summary>
        /// 针对的复检项
        /// </summary>
        public int RecheckItemId { get; private set; }

        /// <summary>
        /// 从哪个库存中领料
        /// </summary>
        public Guid StockId { get; private set; }

        /// <summary>
        /// 领用数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal PickCount { get; private set; }

        /// <summary>
        /// 已领用数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal PickedCount { get; private set; }
    }
}
