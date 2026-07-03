using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TuTa.Wms.RecheckLists.Events;
using TuTa.Wms.RecheckLists.Entities;
using TuTa.Wms.RecheckLists.ValueObjects;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;

namespace TuTa.Wms.RecheckLists.Aggregates
{
    /// <summary>
    /// 复检单
    /// </summary>
    public class RecheckList : AuditedAggregateRoot<Guid>
    {
        private RecheckList()
        {            
        }

        internal RecheckList(
            string recheckListCode,
            DateTime recheckListDate)
        {
            RecheckListCode = Check.NotNullOrWhiteSpace(recheckListCode, nameof(recheckListCode));
            RecheckListDate = recheckListDate;
            Status = RecheckListStatus.Created;
            RecheckItems = new List<RecheckItem>();
            //RecheckStocks = new List<RecheckStock>();
        }

        //添加复检项，barcode与material是否对应只能在业务层处理
        internal void AddRecheckItem(
            string checkNo,
            string barcode,
            MaterialInfoOfRechkList material,
            decimal checkCount,
            int? reCheckTimes = null,
            DateTime? expiryLimitDate = null)
        {
            if (RecheckItems == null) RecheckItems = new List<RecheckItem>();
            var checkItemExist = RecheckItems.FirstOrDefault(o => o.CheckNo == checkNo);
            if (checkItemExist != null)
                throw new Exception($"检验码为{checkNo}的复检项已存在");
            
            RecheckItem checkItem = new RecheckItem(this.Id, checkNo, barcode, material, checkCount, reCheckTimes, expiryLimitDate);
            RecheckItems.Add(checkItem);

            //添加复检项，同时通知库存对象进行冻结操作
            AddLocalEvent(new FreezeStockEvent()
            {
                CheckNo = checkNo
            });
        }

        //删除复检项
        public void RemoveRecheckItem(string checkNo)
        {
            if (RecheckItems == null) RecheckItems = new List<RecheckItem>();

            var item = RecheckItems.FirstOrDefault(o => o.CheckNo == checkNo);
            if (item == null) //没有该收料码的复检项，默认删除成功
                return;

            if (item.Status != RecheckItemStatus.Created)
                throw new Exception("该收料码对应的复检项已经在领用中或已经领用完成，不能删除");

            RecheckItems.Remove(item);

            AddLocalEvent(new UnFreezeStockEvent()
            {
                CheckNo = checkNo
            });
        }

        //修改复检项，barcodeToModify与material是否对应只能在业务层处理
        public void ModifyRecheckItem(
            string barcodeToModify,
            string checkNo,
            MaterialInfoOfRechkList material,
            decimal checkCount,
            //decimal sampleCount,
            int? recheckTimes = null,
            DateTime? expiryLimitDate = null)
        {
            if (RecheckItems == null) RecheckItems = new List<RecheckItem>();

            var item = RecheckItems.FirstOrDefault(o => o.Barcode == barcodeToModify);
            if (item == null) //没有该收料码的复检项，默认删除成功
                throw new Exception($"收料码为{barcodeToModify}的复检项不存在");

            item.Modify(checkNo, material, checkCount, recheckTimes, expiryLimitDate);
        }


        //对指定收料码的复检项从指定的库存中进行出库
        //public void PickAway(string barcode, Guid stockId, decimal pickCount)
        //{
        //    if (RecheckItems == null) RecheckItems = new List<RecheckItem>();

        //    var recheckItem = RecheckItems.FirstOrDefault(o => o.Barcode == barcode); //一个复检项对应一个收料码
        //    if (recheckItem == null)
        //        throw new Exception($"收料码为{barcode}的复检项不存在");

        //    if (RecheckStocks == null) RecheckStocks = new List<RecheckStock>();

        //    var recheckStock = RecheckStocks.FirstOrDefault(o =>
        //        o.RecheckItemId == recheckItem.Id && 
        //        o.StockId == stockId);

        //    if (recheckStock == null)
        //        throw new Exception($"收料码为{barcode}的复检项，没有指定从库存{stockId}中抽检");

        //    recheckItem.PickAway(pickCount);
        //    recheckStock.pickAway(pickCount);

        //    bool isAllFinished = true;
        //    foreach(var item in RecheckItems)
        //    {
        //        if (item.Status != RecheckItemStatus.Finished)
        //        {
        //            isAllFinished = false;
        //            break;
        //        }
        //    }

        //    Status = isAllFinished ? RecheckListStatus.Finished : RecheckListStatus.Picking;

        //    //复检项出库，同时通知库存进行扣减
        //    AddLocalEvent(new ReCheckOutboundEvent()
        //    {
        //        StockId = stockId,
        //        Barcode = barcode,
        //        PickedCount = pickCount
        //    });
        //}

        public void PickAway(string barcode, Guid stockId, decimal pickCount, string operatorName)
        {
            if (RecheckItems == null) RecheckItems = new List<RecheckItem>();

            var recheckItem = RecheckItems.FirstOrDefault(o => o.Barcode == barcode); //一个复检项对应一个收料码
            if (recheckItem == null)
                throw new Exception($"收料码为{barcode}的复检项不存在");

            recheckItem.PickAway(stockId, pickCount);

            bool isAllFinished = true;
            foreach (var item in RecheckItems)
            {
                if (item.Status != RecheckItemStatus.Finished)
                {
                    isAllFinished = false;
                    break;
                }
            }

            Status = isAllFinished ? RecheckListStatus.Finished : RecheckListStatus.Picking;

            //复检项出库，同时通知库存进行扣减
            AddLocalEvent(new ReCheckStockOutEvent()
            {
                StockId = stockId,
                Barcode = barcode, //验证用
                PickedCount = pickCount,
                OperatorName = operatorName
            });
        }

        /// <summary>
        /// 获取复检单中指定收料码对应的复检项
        /// </summary>
        /// <param name="barCode"></param>
        /// <returns></returns>
        public RecheckItem GetReCheckItemByBarcode(string checkNo)
        {
            if (RecheckItems == null) RecheckItems = new List<RecheckItem>();

            return RecheckItems.FirstOrDefault(o => o.CheckNo == checkNo);
        }

        //public void AddReCheckStock(
        //    string barcodeOfReCheckItem,
        //    Guid stockId,
        //    decimal pickCount)
        //{
        //    if (RecheckStocks == null) RecheckStocks = new List<RecheckStock>();

        //    var recheckItem = GetReCheckItemByBarcode(barcodeOfReCheckItem);
        //    if (recheckItem == null)
        //        throw new Exception($"收料码为{barcodeOfReCheckItem}的复检项不存在");

        //    var reCheckStockExist = RecheckStocks.Where(o =>
        //        o.RecheckItemId == recheckItem.Id &&
        //        o.StockId == stockId)
        //        .ToList();

        //    if (reCheckStockExist != null && reCheckStockExist.Count > 0)
        //        throw new Exception($"收料码为{barcodeOfReCheckItem}的领用项，已经指定从库存{stockId}中抽检");

        //    RecheckStock reCheckStock = new RecheckStock(
        //        Id,
        //        recheckItem.Id,
        //        stockId,
        //        pickCount);

        //    RecheckStocks.Add(reCheckStock);
        //}

        //public void AddReCheckStock(
        //    string barcodeOfReCheckItem,
        //    Guid stockId,
        //    decimal pickCount)
        //{
        //    var recheckItem = GetReCheckItemByBarcode(barcodeOfReCheckItem);
        //    if (recheckItem == null)
        //        throw new Exception($"收料码为{barcodeOfReCheckItem}的复检项不存在");

        //    recheckItem.AddRecheckStock(stockId, pickCount);
        //}

        /// <summary>
        /// 获取复检单中为某个收料码分配的出库库存
        /// </summary>
        /// <param name="materialCode"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        //public List<RecheckStock> GetRecheckStocksOfBarcode(string barCode)
        //{
        //    if (RecheckItems == null) RecheckItems = new List<RecheckItem>();
        //    if (RecheckStocks == null) RecheckStocks = new List<RecheckStock>();

        //    var reCheckItem = RecheckItems.FirstOrDefault(o => o.Barcode == barCode);
        //    if (reCheckItem == null) //不存在指定物料的复检项
        //        return new List<RecheckStock>();

        //    return RecheckStocks.Where(o => o.RecheckItemId == reCheckItem.Id).OrderBy(o => o.Id).ToList();
        //}

        //public List<RecheckStock> GetRecheckStocksOfBarcode(string barCode)
        //{
        //    if (RecheckItems == null) RecheckItems = new List<RecheckItem>();

        //    var reCheckItem = RecheckItems.FirstOrDefault(o => o.Barcode == barCode);
        //    if (reCheckItem == null) //不存在指定物料的复检项
        //        return new List<RecheckStock>();

        //    return reCheckItem.RecheckStocks == null ? new List<RecheckStock>() : reCheckItem.RecheckStocks;
        //}

        /// <summary>
        /// 删除复检单中为某个收料码分配的出库库存
        /// </summary>
        /// <param name="barcode"></param>
        /// <exception cref="Exception"></exception>
        //public void RemoveReCheckStocksOfBarcode(string barcode)
        //{
        //    if (RecheckItems == null) RecheckItems = new List<RecheckItem>();
        //    if (RecheckStocks == null) RecheckStocks = new List<RecheckStock>();

        //    var reCheckItemExist = RecheckItems.FirstOrDefault(o => o.Barcode == barcode);
        //    if (reCheckItemExist == null) //不存在指定收料码的复检项，默认删除成功
        //        return;

        //    var recheckStocks = RecheckStocks.Where(o => o.RecheckItemId == reCheckItemExist.Id).ToList();
        //    if (recheckStocks != null && recheckStocks.Count > 0)
        //    {
        //        foreach (var s in recheckStocks)
        //            RecheckStocks.Remove(s);
        //    }
        //}

        //public void RemoveReCheckStocksOfBarcode(string barcode)
        //{
        //    if (RecheckItems == null) RecheckItems = new List<RecheckItem>();

        //    var reCheckItemExist = RecheckItems.FirstOrDefault(o => o.Barcode == barcode);
        //    if (reCheckItemExist == null) //不存在指定收料码的复检项
        //        throw new Exception($"收料码为{barcode}的复检项不存在，删除对应抽检库存失败");

        //    reCheckItemExist.RemoveRecheckStocks();
        //}


        /// <summary>
        /// 复检单单号
        /// </summary>
        [StringLength(30)]
        [Required]
        public string RecheckListCode { get; private set; }

        /// <summary>
        /// 复检单日期
        /// </summary>
        [Column(TypeName = "date")]
        public DateTime RecheckListDate { get; private set; }

        /// <summary>
        /// 复检单状态
        /// </summary>
        public RecheckListStatus Status { get; private set; }

        /// <summary>
        /// 包含的复检项
        /// </summary>
        public List<RecheckItem> RecheckItems { get; private set; }

        /// <summary>
        /// 复检项分配的出库库存
        /// </summary>
        //public List<RecheckStock> RecheckStocks { get; private set; }   
    }
}
