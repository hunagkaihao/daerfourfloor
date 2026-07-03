namespace TuTa.Wms.ChkResultLists.Dtos
{
    /// <summary>
    /// 待入库的物料
    /// </summary>
    public class MaterialToInBoundDto
    {
        /// <summary>
        /// 收料条形码
        /// </summary>
        public string Barcode { get; set; }

        /// <summary>
        /// 物料总数
        /// </summary>
        public decimal TotalCount { get; set; }
    }
}
