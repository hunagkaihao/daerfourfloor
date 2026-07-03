using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using TuTa.Wms.PickLists.Entities;
using TuTa.Wms.PickLists.Events;
using TuTa.Wms.PickLists.ValueObjects;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace TuTa.Wms.PickLists.Aggregates
{
    public class PickList : AuditedAggregateRoot<Guid>
    {
        private PickList()
        {            
        }

        internal PickList(
            Guid id,
            string pickListCode, 
            DateTime pickListDate, 
            int type, 
            PickerInfoOfPickList picker,
            string pickBatch,
            GoodsInfoOfPickList goods)
        {
            Check.NotNullOrWhiteSpace(pickListCode, nameof(pickListCode));
            Check.NotNull(picker, nameof(picker));
            Check.NotNull(goods, nameof(goods));
            Check.Positive(type, nameof(type));
            //if (pickBatch != null && string.IsNullOrWhiteSpace(pickBatch))
            //    throw new Exception("pickBatch的值无效");

            Id = id;
            PickListCode = pickListCode;
            PickListDate = pickListDate;
            Type = type;
            Picker = picker;
            PickBatch = pickBatch;
            Goods = goods;
            Status = PickOrderStatus.Created;
            PickItems = new List<PickItem>();
            //PickStocks = new List<PickStock>();
        }

        public void ModifyPickList(
            DateTime pickListDate,
            int type,
            PickerInfoOfPickList picker,
            string pickBatch,
            GoodsInfoOfPickList goods)
        {
            Check.NotNull(picker, nameof(picker));
            Check.NotNull(goods, nameof(goods));
            Check.Positive(type, nameof(type));
            //if (pickBatch != null && string.IsNullOrWhiteSpace(pickBatch))
            //    throw new Exception("pickBatch的值无效");

            PickListDate = pickListDate;
            Type = type;
            Picker = picker;
            PickBatch = pickBatch;
            Goods = goods;
        }

        internal void AddPickItem(
            string uniqueCode,
            string materialCode,
            string materialName,
            string specs,
            string unit,
            decimal countToPick,
            string checkNo) 
        {
            PickItem item = new PickItem(Id, uniqueCode, materialCode, materialName, specs, unit, countToPick, checkNo);
            
            //if (PickItems == null) PickItems = new List<PickItem>();

            var pickItemsExist = PickItems.Where(o => o.UniqueCode == item.UniqueCode).ToList();
            if (pickItemsExist.Count > 0)
                throw new Exception("值为{uniqueCode}的UniqueCode已经存在，不能重复！");

            //一个领料单中可能会有多个相同的物料
            //pickItemsExist = PickItems.Where(o => o.MaterialCode == item.MaterialCode).ToList();
            //if (pickItemsExist.Count > 0)
            //    throw new Exception($"该领料单中已经存在物料码为{item.MaterialCode}的领用项，不能重复添加");

            PickItems.Add(item);
        }

        public void RemovePickItem(string uniqueCode)
        {
            //if (PickItems == null) PickItems = new List<PickItem>();

            var item = PickItems.FirstOrDefault(o => o.UniqueCode.Equals(uniqueCode));
            if (item == null) //没有该唯一码的领料项，默认删除成功
                return;

            if (item.Status != PickItemStatus.Created)
                throw new Exception("该唯一码对应的领料项已经在领用中或已经领用完成，不能删除");

            PickItems.Remove(item);
        }

        public void ModifyPickItem(
            string uniqueCodeOfPickItemToModify,
            string materialCodeNew,
            string materialNameNew,
            string specsNew,
            string unitNew,
            decimal countToPickNew)
        {
            //if (PickItems == null) PickItems = new List<PickItem>();

            var pickItemExist = PickItems.FirstOrDefault(o => o.UniqueCode == uniqueCodeOfPickItemToModify);
            if (pickItemExist == null)
                throw new Exception($"UniqueCode为{uniqueCodeOfPickItemToModify}的领用项不存在");

            pickItemExist.Modify(materialCodeNew, materialNameNew, specsNew, unitNew, countToPickNew);
        }

        public PickItem GetPickItemByUniqueCode(string uniqueCode)
        {
            //if (PickItems == null) PickItems = new List<PickItem>();

            return PickItems.FirstOrDefault(o => o.UniqueCode == uniqueCode);
        }

        public PickItem GetPickItemByPickItemId(int pickItemId)
        {
            return PickItems.FirstOrDefault(o => o.Id == pickItemId);
        }

        /// <summary>
        /// 对指定物料编号的领用项进行领用
        /// </summary>
        /// <param name="uniqueCode">领料项唯一码</param>
        /// <param name="cellId">领用库位</param>
        /// <param name="barcode">领用的收料条形码</param>
        /// <param name="pickCount">领用的数量</param>
        public void Pick(string uniqueCode, Guid stockId, decimal pickCount, string operatorName)
        {
            //if (PickItems == null) PickItems = new List<PickItem>();
            //if (PickStocks == null) PickStocks = new List<PickStock>();


            var pickItemExist = PickItems.FirstOrDefault(o => o.UniqueCode == uniqueCode);
            if (pickItemExist == null)
                throw new Exception($"UniqueCode为{uniqueCode}的领用项不存在");

            //var pickStockExist = PickStocks.FirstOrDefault(o => 
            //    o.PickItemId == pickItemExist.Id && 
            //    o.StockId == stockId);

            //if (pickStockExist == null)
            //    throw new Exception($"物料码为{materialCode}的领用项，没有指定从Id为{stockId}的库存中领用");

            pickItemExist.PickAway(stockId, pickCount);
            //pickStockExist.pickAway(pickCount);

            bool isAllPicked = true;
            foreach(var item in PickItems)
            {
                if (item.Status != PickItemStatus.Picked)
                {
                    isAllPicked = false;
                    break;
                }
            }

            Status = isAllPicked ? PickOrderStatus.Finished : PickOrderStatus.Picking;

            //领料完成时，发出领料完成事件，库存相应修改
            //AddLocalEvent(new PickListStockOutEvent()
            //{
            //    StockId = stockId,
            //    PickOutCnt = pickCount,

            //    PickListCode = this.PickListCode,
            //    PickType = this.Type,
            //    DeptCode = this.Picker.DeptCode,
            //    DeptName = this.Picker.DeptName,
            //    GysCode = this.Picker.GysCode,
            //    GysName = this.Picker.GysName,
            //    PickerName = this.Picker.PickManName,
            //    PickBatch = this.PickBatch,
            //    GoodsCode = this.Goods.GoodsCode,
            //    GoodsName = this.Goods.GoodsName,
            //    GoodsSpecs = this.Goods.GoodsSpecs,
            //    UniqueCode = pickItemExist.UniqueCode,

            //    PickTypeChs = PickTypeHelper.PickTypeToChinese(Type),
            //    OperatorName = operatorName
            //});
        }
        public void Pick(string uniqueCode, Guid stockId, decimal pickCount)
        {
            //if (PickItems == null) PickItems = new List<PickItem>();
            //if (PickStocks == null) PickStocks = new List<PickStock>();


            var pickItemExist = PickItems.FirstOrDefault(o => o.UniqueCode == uniqueCode);
            if (pickItemExist == null)
                throw new Exception($"UniqueCode为{uniqueCode}的领用项不存在");

            //var pickStockExist = PickStocks.FirstOrDefault(o => 
            //    o.PickItemId == pickItemExist.Id && 
            //    o.StockId == stockId);

            //if (pickStockExist == null)
            //    throw new Exception($"物料码为{materialCode}的领用项，没有指定从Id为{stockId}的库存中领用");

            pickItemExist.PickAway(stockId, pickCount);
            //pickStockExist.pickAway(pickCount);

            bool isAllPicked = true;
            foreach (var item in PickItems)
            {
                if (item.Status != PickItemStatus.Picked)
                {
                    isAllPicked = false;
                    break;
                }
            }

            Status = isAllPicked ? PickOrderStatus.Finished : PickOrderStatus.Picking;


        }

        /// <summary>
        /// 增加领料库存来源
        /// </summary>
        /// <param name="materialCodeOfPickItem"></param>
        /// <param name="warehouseId"></param>
        /// <param name="warehouseAreaId"></param>
        /// <param name="cellId"></param>
        /// <param name="boxId"></param>
        /// <param name="barcode"></param>
        /// <param name="checkOrderCode"></param>
        /// <param name="stockCount"></param>
        /// <param name="pickCount"></param>
        /// <exception cref="Exception"></exception>
        //public void AddPickStock(
        //    string materialCodeOfPickItem,
        //    Guid stockId,
        //    decimal pickCount)
        //{
        //    if (PickStocks == null) PickStocks = new List<PickStock>();

        //    var pickItem = GetPickItemByMaterialCode(materialCodeOfPickItem);
        //    if (pickItem == null)
        //        throw new Exception($"物料码为{materialCodeOfPickItem}的领用项不存在");

        //    var pickStockExist = PickStocks.Where(o =>
        //        o.PickItemId == pickItem.Id &&
        //        o.StockId == stockId).ToList();

        //    if (pickStockExist != null && pickStockExist.Count > 0)
        //        throw new Exception($"物料码为{materialCodeOfPickItem}的领用项，已经指定从Id为{stockId}的库存中领用");

        //    PickStock pickStock = new PickStock(
        //        Id,
        //        pickItem.Id,
        //        stockId,
        //        pickCount);

        //    PickStocks.Add(pickStock);
        //}

        /// <summary>
        /// 获取当前领料单中某个物料的库存来源
        /// </summary>
        /// <param name="materialCode"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        //public List<PickStock> GetPickStocksOfMaterial(string materialCode)
        //{
        //    if (PickItems == null) PickItems = new List<PickItem>();
        //    //if (PickStocks == null) PickStocks = new List<PickStock>();

        //    var pickItemExist = PickItems.FirstOrDefault(o => o.MaterialCode == materialCode);
        //    if (pickItemExist == null) //不存在指定物料的领用项
        //        return new List<PickStock>();

        //    //return PickStocks.Where(o => o.PickItemId == pickItemExist.Id).OrderBy(o => o.Id).ToList();
        //    return pickItemExist.PickStocks == null ? new List<PickStock>() : pickItemExist.PickStocks;
        //}

        /// <summary>
        /// 删除当前领料单中指定物料对应的所有领料源
        /// </summary>
        /// <param name="materialCode"></param>
        /// <exception cref="Exception"></exception>
        //public void RemovePickStocksOfMaterial(string materialCode)
        //{
        //    if (PickItems == null) PickItems = new List<PickItem>();
        //    if (PickStocks == null) PickStocks = new List<PickStock>();

        //    var pickItemExist = PickItems.FirstOrDefault(o => o.MaterialCode == materialCode);
        //    if (pickItemExist == null) //不存在指定的物料，默认删除成功
        //        return;

        //    var pickSources = PickStocks.Where(o => o.PickItemId == pickItemExist.Id).ToList();
        //    if (pickSources != null && pickSources.Count > 0)
        //    {
        //        foreach(var s in pickSources)
        //            PickStocks.Remove(s);
        //    }    
        //}

        /// <summary>
        /// 领料单单号
        /// </summary>
        [StringLength(30)]
        [Required]
        public string PickListCode { get; private set; }

        /// <summary>
        /// 领料单日期
        /// </summary>
        [Column(TypeName = "date")]
        public DateTime PickListDate {  get; private set; }

        /// <summary>
        /// 领用类型  1 生产领用  2外协领用 15生产领用2（非生产车间领用)  11试样领用   19退供应商   
        /// </summary>
        public int Type { get; private set; }

        /// <summary>
        /// 领料单状态
        /// </summary>
        public PickOrderStatus Status { get; private set; }

        /// <summary>
        /// 领料部门或单位信息
        /// </summary>
        [Required]
        public PickerInfoOfPickList Picker { get; private set; }

        /// <summary>
        /// 领用生产批号，和领料通知单号一一对应，生产领用及外协领用时存在，无计划领用不存在
        /// </summary>
        [StringLength(30)]
        public string PickBatch { get; private set; }

        /// <summary>
        /// 领用物料用于的成品信息，生产领用及外协领用时存在，无计划领用不存在
        /// </summary>
        [Required]
        public GoodsInfoOfPickList Goods { get; private set; }

        /// <summary>
        /// 领料单中需要领用的物料信息
        /// </summary>
        public List<PickItem> PickItems { get; private set; }

        /// <summary>
        /// 领料项从领料来源
        /// </summary>
        //public List<PickStock> PickStocks { get; set; }
    }
}
