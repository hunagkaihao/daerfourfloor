using System;

namespace TuTa.Wms.PickLists.Events
{
    public class PickListStockOutEvent
    {
        public Guid StockId{ get; set; }

        public decimal PickOutCnt { get; set; }


        /// <summary>
        /// 领料单单号
        /// </summary>
        public string PickListCode { get; set; }

        /// <summary>
        /// 领用类型  1 生产领用  14超计划领用  15无计划领用  2外协领用
        /// </summary>
        public int PickType { get; set; }

        /// <summary>
        /// 领用部门编号
        /// </summary>
        public string DeptCode { get; set; }

        /// <summary>
        /// 领用部门名称
        /// </summary>
        public string DeptName { get; set; }

        /// <summary>
        /// 领用外协单位编号
        /// </summary>
        public string GysCode { get; set; }

        /// <summary>
        /// 领用外协单位名称
        /// </summary>
        public string GysName { get; set; }

        /// <summary>
        /// 领料员
        /// </summary>
        public string PickerName { get; set; }

        /// <summary>
        /// 领用生产批号，和领料通知单号一一对应，生产领用及外协领用时存在，无计划领用不存在
        /// </summary>
        public string PickBatch { get; set; }

        /// <summary>
        /// 成品编号
        /// </summary>
        public string GoodsCode { get; set; }

        /// <summary>
        /// 成品名称
        /// </summary>
        public string GoodsName { get; set; }

        /// <summary>
        /// 成品规格
        /// </summary>
        public string GoodsSpecs { get; set; }

        /// <summary>
        /// 领料通知单唯一编号
        /// </summary>
        public string UniqueCode { get; set; }



        public string PickTypeChs { get; set; }

        public string OperatorName { get; set; }

    }
}
