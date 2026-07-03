using System;

namespace TuTa.Wms.ChkResultLists.Events
{
    public class RecheckResultGettedEvent
    {
        public string BarcodeOfRecheckStock { get; set; }

        public string CheckOrderCode { get; set; }

        public DateTime CheckDate { get; set; }

        public string CheckNo { get; set; }

        public string CheckNoBeforeReCheck { get; set; }

        public EnumCheckType CheckType { get; set; }

        public EnumCheckResult CheckResult { get; set; }

        public decimal PassCnt { get; set; }
    }
}
