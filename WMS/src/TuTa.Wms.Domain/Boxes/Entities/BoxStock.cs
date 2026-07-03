using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace TuTa.Wms.Boxes.Entities
{
    public class BoxStock : Entity
    {
        private BoxStock()
        {
            
        }

        public BoxStock(Guid boxId, Guid stockId, string stockBarcode)
        {
            BoxId = boxId;
            StockId = stockId;
            StockBarcode = Check.NotNullOrWhiteSpace(stockBarcode, nameof(stockBarcode));
        }

        public Guid BoxId { get; private set; }

        public Guid StockId { get; private set; }

        /// <summary>
        /// 库存的收料条形码
        /// </summary>
        [StringLength(30)]
        public string StockBarcode { get; private set; }


        public override object[] GetKeys()
        {
            return new object[] { BoxId, StockId };
        }
    }
}
