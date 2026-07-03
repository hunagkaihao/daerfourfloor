using System;

namespace TuTa.Wms.PickLists.Dtos
{
    public class PickItemQueryDto
    {
        /// <summary>
        /// 领用类型
        /// </summary>
        public int? PickType { get; set; }

        /// <summary>
        /// 领料单所属部门
        /// </summary>
        public Guid? DepartmentId{ get; set; }

        /// <summary>
        /// 根据什么查询：1、按照物料号查询，2、按照物料名查询，3、按照物料规格查询，4、按照批次查询
        /// </summary>
        public int? QueryBy { get; set; }

        public string MaterialNameTip { get; set; }

        public string MaterialSpecsTip { get; set; }

        public string MaterialCode { get; set; }

        public string BatchTip { get; set; }

        /// <summary>
        /// 1：按照物料排序，2：按照生产批号排序
        /// </summary>
        public int OrderBy { get; set; }
    }
}
