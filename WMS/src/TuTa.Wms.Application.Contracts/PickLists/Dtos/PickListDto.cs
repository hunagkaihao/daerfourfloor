using System;
using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.PickLists.Dtos
{
    public class PickListDto : EntityDto<Guid>
    {
        /// <summary>
        /// 领料单号
        /// </summary>
        public string PickListCode { get; set; }

        /// <summary>
        /// 领料单日期
        /// </summary>
        public string PickListDate { get; set; }

        /// <summary>
        /// 领用类型  1 生产领用  14超计划领用  15无计划领用  2外协领用
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 领料单状态
        /// </summary>
        public string Status { get; set; }

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
    }
}
