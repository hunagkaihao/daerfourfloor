namespace TuTa.Wms.RecheckLists.Dtos
{
    public class RecheckPickOutDto
    {
        public string RecheckListCode { get; set; }

        public string Barcode { get; set; }

        public string BoxCode { get; set; }

        public decimal PickOutCnt { get; set; }

        public string OperatorName { get; set; }
    }
}
