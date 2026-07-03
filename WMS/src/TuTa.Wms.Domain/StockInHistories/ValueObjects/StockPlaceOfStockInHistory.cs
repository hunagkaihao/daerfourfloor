using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Values;

namespace TuTa.Wms.StockInHistories.ValueObjects
{
    [Owned]
    public class StockPlaceOfStockInHistory : ValueObject
    {
        private StockPlaceOfStockInHistory()
        {            
        }

        public StockPlaceOfStockInHistory(
            string houseCode, string houseName,
            string areaCode, string areaName,
            string cellCode, string cellName,
            string boxCode, string boxName)
        {
            HouseCode = houseCode;
            HouseName = houseName;
            AreaCode = areaCode;
            AreaName = areaName;
            CellCode = cellCode;
            CellName = cellName;
            BoxCode = boxCode;
            BoxName = boxName;
        }

        /// <summary>
        /// 入库仓库码
        /// </summary>
        [StringLength(20)]
        public string HouseCode { get; private set; }

        /// <summary>
        /// 入库仓库名
        /// </summary>
        [StringLength(50)]
        public string HouseName { get; private set; }

        /// <summary>
        /// 入库库区码
        /// </summary>
        [StringLength(20)]
        public string AreaCode { get; private set; }

        /// <summary>
        /// 入库库区名
        /// </summary>
        [StringLength(50)]
        public string AreaName { get; private set; }

        /// <summary>
        /// 入库仓位码
        /// </summary>
        [StringLength(20)]
        public string CellCode { get; private set; }

        /// <summary>
        /// 入库仓位名
        /// </summary>
        [StringLength(50)]
        public string CellName { get; private set; }

        /// <summary>
        /// 入库容器码
        /// </summary>
        [StringLength(20)]
        public string BoxCode { get; private set; }

        /// <summary>
        /// 入库容器名
        /// </summary>
        [StringLength(50)]
        public string BoxName { get; private set; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return [ HouseCode, HouseName, AreaCode, AreaName, CellCode, CellName,  BoxCode, BoxName ];
        }
    }
}
