namespace TuTa.Wms.Stocks.Dtos
{
    public class StockMoveDto
    {
        public string BarcodeToMove { get; set; }

        public string SrcCellCode { get; set; }

        public string SrcBoxCode => SrcCellCode;

        public string TgtCellCode { get; set; }

        public string TgtBoxCode => TgtCellCode;

        public string OperatorName { get; set; }

        public decimal MoveCount { get; set; }
    }
}
