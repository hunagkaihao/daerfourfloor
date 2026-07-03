namespace TuTa.Wms.Cells
{
    public enum CellType
    {
        /// <summary>
        /// 货位
        /// </summary>
        Cell = 1,
        /// <summary>
        /// CTU库位
        /// </summary>
        CTUCell,
        /// <summary>
        /// 分拨墙
        /// </summary>
        WallCell,
        /// <summary>
        /// 站台/输送台
        /// </summary>
        Station,
        /// <summary>
        /// 异常站台/工位
        /// </summary>
        ErrorStation,
        /// <summary>
        /// 生产工位
        /// </summary>
        WorkStation,
        /// <summary>
        /// 料车上货位
        /// </summary>
        SkipCell,
        /// <summary>
        /// 料车点位
        /// </summary>
        Skip,
        /// <summary>
        /// 虚拟点位
        /// </summary>
        Virtual,
        /// <summary>
        /// 手工库位
        /// </summary>
        HandCell


    }
}
