using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using TuTa.Wms.RecheckLists.ValueObjects;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace TuTa.Wms.RecheckLists.Entities
{
    public class RecheckItem : Entity<int>
    {
        private RecheckItem()
        {            
        }

        internal RecheckItem(
            Guid recheckListId,
            string checkNo,
            string barcode,
            MaterialInfoOfRechkList material,
            decimal checkCount,
            int? recheckTimes = null,
            DateTime? expiryLimitDate = null)
        {
            RecheckListId = recheckListId;
            CheckNo = Check.NotNullOrWhiteSpace(checkNo, nameof(checkNo));
            Barcode = Check.NotNullOrWhiteSpace(barcode, nameof(barcode));
            Material = Check.NotNull(material, nameof(material));
            CheckCount = Check.Positive(checkCount, nameof(checkCount));
            if (recheckTimes != null && recheckTimes < 0)
                throw new ArgumentException("recheckTimes的值无效");
            RecheckTimes = recheckTimes;
            ExpiryLimitDate = expiryLimitDate;
            PickedCount = 0;
            Status = RecheckItemStatus.Created;
        }

        internal void Modify(
            string checkNo,
            MaterialInfoOfRechkList material,
            decimal checkCount,
            int? recheckTimes = null,
            DateTime? expiryLimitDate = null)
        {
            if (Status != RecheckItemStatus.Created)
                throw new Exception("当前复检项已经在领料中或已领料，不能进行修改");

            CheckNo = Check.NotNullOrWhiteSpace(checkNo, nameof(checkNo));
            Material = Check.NotNull(material, nameof(material));
            CheckCount = Check.Positive(checkCount, nameof(checkCount));
            //SampleCount = Check.Positive(sampleCount, nameof(sampleCount));
            //if (sampleCount > CheckCount)
            //    throw new Exception($"抽检数量不能大于复检物料总数");
            if (recheckTimes != null && recheckTimes < 0)
                throw new ArgumentException("reCheckTimes的值无效");
            RecheckTimes = recheckTimes;
            ExpiryLimitDate = expiryLimitDate;
        }

        public void AddRecheckStock(Guid stockId, decimal pickCount)
        {
            if (RecheckStocks == null) RecheckStocks = new List<RecheckStock>();

            var reCheckStockExist = RecheckStocks.Where(o =>
                o.StockId == stockId)
                .ToList();

            if (reCheckStockExist != null && reCheckStockExist.Count > 0)
                throw new Exception($"收料码为{Barcode}的领用项，已经指定从库存{stockId}中抽检，请勿重复添加");

            if (pickCount <= 0)
                throw new Exception($"新增抽样数量{pickCount}无效，请保证大于0");

            decimal countExist = 0;
            foreach(var stock in RecheckStocks)
            {
                countExist += (stock.PickCount - stock.PickedCount);
            }

            if (countExist + pickCount > CheckCount)
                throw new Exception($"增加{pickCount}后，总的抽样数量超过了需要抽样的数量{CheckCount}");

            RecheckStock reCheckStock = new RecheckStock(
                Id,
                stockId,
                pickCount);

            RecheckStocks.Add(reCheckStock);
        }

        public void RemoveRecheckStocks()
        {
            if (RecheckStocks == null) 
                RecheckStocks = new List<RecheckStock>();
            else
                RecheckStocks.Clear();
        }

        public RecheckStock GetRecheckStockByStockId(Guid stockId)
        {
            if (RecheckStocks == null) RecheckStocks = new List<RecheckStock>();

            return RecheckStocks.FirstOrDefault(o => o.StockId == stockId);
        }

        /// <summary>
        /// 从库存中领取复检物料，一个复检项可能从多个库存中抽检
        /// </summary>
        /// <param name="stockId"></param>
        /// <param name="count"></param>
        /// <exception cref="Exception"></exception>
        internal void PickAway(Guid stockId, decimal count)
        {
            Check.Positive(count, nameof(count));
            if (PickedCount + count > CheckCount)
                throw new Exception($"领取{count}个物料后，超过了要求抽检的数量{CheckCount}");

            if (RecheckStocks == null) RecheckStocks = new List<RecheckStock>();

            var recheckStockExist = RecheckStocks.FirstOrDefault(o => o.StockId == stockId);
            if (recheckStockExist == null)
                throw new Exception($"当前复检项中未指定从Id为{stockId}的库存中抽检");

            recheckStockExist.pickAway(count);

            PickedCount += count;
            if (PickedCount < CheckCount)
                Status = RecheckItemStatus.Picking;
            else
                Status = RecheckItemStatus.Finished;
        }
        public void PickAway(decimal count)
        {
            Check.Positive(count, nameof(count));

            PickedCount += count;
            if (PickedCount < CheckCount)
                Status = RecheckItemStatus.Picking;
            else
                Status = RecheckItemStatus.Finished;
        }

        /// <summary>
        /// 所属的复检通知单Id
        /// </summary>
        public Guid RecheckListId { get; private set; }

        /// <summary>
        /// 检验编号
        /// </summary>
        [StringLength(30)]
        [Required]
        public string CheckNo { get; private set; }

        /// <summary>
        /// 复检次数
        /// </summary>
        public int? RecheckTimes { get; private set; }

        /// <summary>
        /// 复检数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal CheckCount { get; private set; }

        /// <summary>
        /// 抽检数量
        /// </summary>
        //[Column(TypeName = "decimal(18,6)")]
        //public decimal SampleCount { get; private set; }

        /// <summary>
        /// 已领料数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal PickedCount { get; private set; }

        /// <summary>
        /// 收料条形码
        /// </summary>
        [StringLength(30)]
        [Required]
        public string Barcode { get; private set; }

        /// <summary>
        /// 物料信息
        /// </summary>
        [Required]
        public MaterialInfoOfRechkList Material { get; private set; }

        /// <summary>
        /// 保质期限
        /// </summary>
        [Column(TypeName = "date")]
        public DateTime? ExpiryLimitDate { get; private set; }

        /// <summary>
        /// 复检项状态
        /// </summary>
        public RecheckItemStatus Status { get; private set; }

        /// <summary>
        /// 复检项从哪些库存中抽检
        /// </summary>
        public List<RecheckStock> RecheckStocks { get; private set; }
    }
}
