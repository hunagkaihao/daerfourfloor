using Volo.Abp.Domain.Entities;

namespace TuTa.Wms.Erp.Entities
{
    public class ErpPickMan : Entity
    {
        public string MAN_NAME { get; set; }

        public override object[] GetKeys()
        {
            return [ MAN_NAME ];
        }
    }
}
