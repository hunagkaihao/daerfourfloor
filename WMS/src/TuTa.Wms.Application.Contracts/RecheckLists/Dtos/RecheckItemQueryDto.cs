namespace TuTa.Wms.RecheckLists.Dtos
{
    public class RecheckItemQueryDto
    {
        /// <summary>
        /// 根据什么查询：1、按照物料号查询，2、按照物料名查询，3、按照物料规格查询，4、按照检验编号查询
        /// </summary>
        public int? QueryBy { get; set; }

        public string CheckNoTip { get; set; }

        public string MaterialCode { get; set; }

        public string MaterialNameTip { get; set; }

        public string MaterialSpecsTip { get; set; }
    }
}
