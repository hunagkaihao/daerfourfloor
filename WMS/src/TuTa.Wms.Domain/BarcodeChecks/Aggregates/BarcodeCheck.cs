using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace TuTa.Wms.BarcodeChecks.Aggregates
{
    public class BarcodeCheck : AuditedAggregateRoot<Guid>
    {
        private BarcodeCheck()
        {

        }

        public BarcodeCheck(Guid boxId, Guid barcodeId, int count)
        {
            BoxId = boxId;
            BarcodeId = barcodeId;
            Count = count;
        }

        public Guid? BoxId { get; set; }
        public Guid BarcodeId { get; set; }
        public int Count { get; set; }
    }
}
