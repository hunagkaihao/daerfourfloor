using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using TuTa.Wms.Cells;

using Volo.Abp.Domain.Values;

namespace TuTa.Wms.Stocks.ValueObjects
{
    [Owned]
    public class CellInfoOfStock : ValueObject
    {
        private CellInfoOfStock()
        {
        }

        public CellInfoOfStock(Guid? cellId, string cellCode, string cellName,string avaType,CellType? cellType)
        {
            CellId = cellId;
            CellCode = WmsDomainHelper.NotWhiteSpaceCheck(cellCode, nameof(cellCode));
            CellName = WmsDomainHelper.NotWhiteSpaceCheck(cellName, nameof(cellName));
            AvaBoxType = avaType;
            CellType = cellType;
        }

        public Guid? CellId { get; private set; }

        [StringLength(20)]
        public string CellCode { get; private set; }

        [StringLength(50)]
        public string CellName { get; private set; }

        [StringLength(50)]
        public string AvaBoxType { get; set; }
        public CellType? CellType { get; set; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return [ CellId, CellCode, CellName, AvaBoxType ];
        }
    }
}
