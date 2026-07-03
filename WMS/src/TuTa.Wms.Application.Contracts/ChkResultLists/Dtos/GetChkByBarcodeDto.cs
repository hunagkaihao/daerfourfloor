using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

using TuTa.Wms.Stocks.Dtos;

using Volo.Abp.Application.Dtos;

namespace TuTa.Wms.ChkResultLists.Dtos
{
    public class GetChkByBarcodeDto
    {
        public ChkResultListDto ChkResultList { get; set; }
        public List<StockDto> items { get; set; }
    }
}
