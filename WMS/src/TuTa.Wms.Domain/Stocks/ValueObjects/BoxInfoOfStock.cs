using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TuTa.Wms.Stocks.ValueObjects
{
    [Owned]
    public class BoxInfoOfStock
    {
        private BoxInfoOfStock()
        {
        }

        public BoxInfoOfStock(Guid? boxId, string boxCode, string boxName, decimal? fullRate, string boxNumber = null)
        {
            BoxId = boxId;
            BoxCode = WmsDomainHelper.NotWhiteSpaceCheck(boxCode, nameof(boxCode));
            BoxName = WmsDomainHelper.NotWhiteSpaceCheck(boxName, nameof(boxName));
            FullRate = fullRate;
            BoxNumber = boxNumber;
        }

        public Guid? BoxId { get; private set; }

        [StringLength(20)]
        public string BoxCode { get; private set; }

        [StringLength(50)]
        public string BoxName { get; private set; }

        [Column(TypeName = "decimal(10,6)")]
        public decimal? FullRate { get; set; }

        [StringLength(30)]
        public string BoxNumber { get; private set; }

    }
}
