namespace TuTa.Wms.PickLists.Dtos
{
    public class PickOutDto
    {
        /// <summary>
        /// 领料容器码
        /// </summary>
        public string BoxCode { get; set; }

        /// <summary>
        /// 收料条形码
        /// </summary>
        public string Barcode { get; set; }

        /// <summary>
        /// 领料数量
        /// </summary>
        public decimal PickOutCnt { get; set; }

        /// <summary>
        /// 领料员
        /// </summary>
        public string OperatorName { get; set; }
    }
}
