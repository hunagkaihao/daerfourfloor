namespace TuTa.Wms.Cells.Dtos
{
    /// <summary>
    /// 同巷道库位状态信息
    /// </summary>
    public class CellLaneStatusDto
    {
        public string CellCode { get; set; }

        /// <summary>
        /// 巷道位
        /// </summary>
        public int? LanePosition { get; set; }

        /// <summary>
        /// 库位状态，有货、无货、满货
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 运行状态，禁用、可用、选定等
        /// </summary>
        public string RunStatus { get; set; }
    }
}
