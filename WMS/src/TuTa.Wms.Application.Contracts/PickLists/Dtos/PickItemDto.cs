using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TuTa.Wms.PickLists.Dtos
{
    public class PickItemDto
    {
        /// <summary>
        /// 所属领料单Id
        /// </summary>
        public Guid PickListId { get; set; }

        /// <summary>
        /// 领料项Id
        /// </summary>
        public int PickItemId { get; set; }

        /// <summary>
        /// 领料单编号
        /// </summary>
        public string PickListCode { get; set; }

        /// <summary>
        /// 领料通知日期
        /// </summary>
        [StringLength(30)]
        public string PickListDate { get; set; }

        /// <summary>
        /// 领用类型  1 生产领用  14超计划领用  15无计划领用  2外协领用
        /// </summary>
        public string PickType { get; set; }

        /// <summary>
        /// 领用类型名称
        /// </summary>
        public int PickTypeNo { get; set; }

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
        /// 领用人名称
        /// </summary>
        public string PickManName { get; set; }

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

        /// <summary>
        /// 领取的物料码
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        /// 领取的物料名称
        /// </summary>
        public string MaterialName { get; set; }

        /// <summary>
        /// 领取的物料规格
        /// </summary>
        public string Specs { get; set; }

        /// <summary>
        /// 领取的物料单位
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// 需要领的数量
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal CountToPick { get; set; }

        /// <summary>
        /// 已领数量
        /// </summary>
        public decimal PickedCount { get; set; }

        public string CheckNo { get; set; }

        /// <summary>
        /// 领料项状态：创建、领料中、领料完成
        /// </summary>
        public string PickItemStatus { get; set; }

        /// <summary>
        /// 检验时间最早的那个库存所在库位
        /// </summary>
        public string CellCode { get; set; }

        /// <summary>
        /// 检验时间最早的那个库存中的数量
        /// </summary>
        public decimal? CountInCell { get; set; }

        /// <summary>
        /// 剩余领用数量
        /// </summary>
        public decimal? CountInRemaining { get; set; }
    }
}
