using System;
using System.Collections.Generic;
using System.Text;

using TuTa.Wms.Cells.Dtos;
using TuTa.Wms.Stocks.Dtos;

namespace TuTa.Wms.Skips.Dtos
{
    public class ResultGetBySkipDto
    {
        public List<GetBySkipDto> getBySkip { get; set; }

        public List<StockDto> stocks { get; set; }
    }

    public class GetBySkipDto
    {
        public string boxCode { get; set; }
        public string startCode { get; set; }
        public CellDto endCell { get; set; }
        public int endArea { get; set; }
    }
}
