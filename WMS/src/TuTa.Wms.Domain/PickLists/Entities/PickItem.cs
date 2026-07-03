using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace TuTa.Wms.PickLists.Entities
{
    public class PickItem : Entity<int>
    {
        private PickItem()
        {            
        }

        internal PickItem(
            Guid pickListId,
            string uniqueCode, 
            string materialCode, 
            string materialName, 
            string specs, 
            string unit, 
            decimal countToPick, 
            string checkNo)
        {
            PickListId = pickListId;
            UniqueCode = Check.NotNullOrWhiteSpace(uniqueCode, nameof(uniqueCode));
            MaterialCode = Check.NotNullOrWhiteSpace(materialCode, nameof(materialCode));
            MaterialName = Check.NotNullOrWhiteSpace(materialName, nameof(materialName));
            CheckNo = checkNo;
            //if (specs != null && string.IsNullOrWhiteSpace(specs))
            //    throw new Exception("specs的值无效");
            //if (unit != null && string.IsNullOrWhiteSpace(unit))
            //    throw new Exception("unit的值无效");
            Specs = specs;
            Unit = unit;
            CountToPick = Check.Positive(countToPick, nameof(countToPick));
            PickedCount = 0;
            Status = PickItemStatus.Created;
            PickStocks = new List<PickStock>();
        }

        internal void Modify(
            string materialCodeNew,
            string materialNameNew,
            string specsNew,
            string unitNew,
            decimal countToPickNew)
        {
            if (Status != PickItemStatus.Created)
                throw new Exception("当前领料项已经在领料中或已领料，不能进行修改");

            Check.NotNullOrWhiteSpace(materialCodeNew, nameof(materialCodeNew));
            Check.NotNullOrWhiteSpace(materialNameNew, nameof(materialNameNew));
            //if (specsNew != null && string.IsNullOrWhiteSpace(specsNew))
            //    throw new Exception("specsNew的值无效");
            //if (unitNew != null && string.IsNullOrWhiteSpace(unitNew))
            //    throw new Exception("unitNew的值无效");
            Check.Positive(countToPickNew, nameof(countToPickNew));
            if (PickedCount > countToPickNew)
                throw new Exception("需领用数量不能小于已领用数量");

            MaterialCode = materialCodeNew;
            MaterialName = materialNameNew;
            Specs = specsNew;
            Unit = unitNew;
            CountToPick = countToPickNew;
        }

        internal void SetStatus(PickItemStatus status)
        {
            Status = status;
        }

        internal void PickAway(Guid stockId, decimal pickAwayCount)
        {
            Check.Positive(pickAwayCount, nameof(pickAwayCount));
            if (pickAwayCount + PickedCount > CountToPick)
                throw new Exception($"新领取数量{pickAwayCount}后，总领取数量为{pickAwayCount + PickedCount}，超过了要求的领取数量{CountToPick}");

            //var pickStockExist = FindPickStockByStockId(stockId);
            //if (pickStockExist == null)
            //    throw new Exception($"当前领料项中没有指定从Id为{stockId}的库存中领取");

            //pickStockExist.PickAway(pickAwayCount);

            PickedCount += pickAwayCount;

            if (PickedCount < CountToPick)
                Status = PickItemStatus.Picking;
            else
                Status = PickItemStatus.Picked;
        }

        public void AddPickStock(
            Guid stockId,
            decimal pickCount)
        {
            //if (PickStocks == null) PickStocks = new List<PickStock>();

            var pickStockExist = PickStocks.FirstOrDefault(o => o.StockId == stockId);

            if (pickStockExist != null)
                throw new Exception($"当前领料项，已经指定从Id为{stockId}的库存中领用，请勿重复添加");

            PickStock pickStock = new PickStock(
                Id,
                stockId,
                pickCount);

            PickStocks.Add(pickStock);
        }

        public void RemovePickStocks()
        {
            //if (PickStocks == null) PickStocks = new List<PickStock>();
            PickStocks.Clear();
        }

        public void RemoveOnePickStock(Guid stockId)
        {
            var pickStockExist = PickStocks.FirstOrDefault(o => o.StockId == stockId);
            if (pickStockExist == null)
                return;

            PickStocks.Remove(pickStockExist);
        }

        public PickStock FindPickStockByStockId(Guid stockId)
        {
            //if (PickStocks == null) PickStocks = new List<PickStock>();

            return PickStocks.FirstOrDefault(o => o.StockId == stockId);
        }

        public List<PickStock> GetAllPickStocks()
        {
            //if (PickStocks == null) return new List<PickStock>();
            return PickStocks;
        }

        public Guid PickListId { get; private set; }

        /// <summary>
        /// 领料通知单唯一编号
        /// </summary>
        [StringLength(32)]
        [Required]
        public string UniqueCode { get; private set; }

        [StringLength(20)]
        [Required]
        public string MaterialCode { get; private set; }

        [StringLength(120)]
        [Required]
        public string MaterialName { get; private set; }

        [StringLength(120)]
        public string Specs { get; private set; }

        [StringLength(10)]
        //[Required]
        public string Unit { get; private set; }

        /// <summary>
        /// 需要领的数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal CountToPick { get; private set; }

        /// <summary>
        /// 领用检验编号
        /// </summary>
        [StringLength(50)]
        public string CheckNo { get; private set; }

        /// <summary>
        /// 已领数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal PickedCount { get; private set; }

        /// <summary>
        /// 领料项状态：创建、领料中、领料完成
        /// </summary>
        public PickItemStatus Status { get; private set; }

        /// <summary>
        /// 从哪里领料，一个领料项可从多个库存中领料
        /// </summary>
        public List<PickStock> PickStocks{ get; private set; }
    }
}
