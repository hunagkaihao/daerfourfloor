using AutoMapper.Configuration;
using Volo.Abp.Domain.Entities;

namespace TuTa.Wms.Erp.Entities
{
    public class ErpNoPlanPickType : Entity
    {
        private ErpNoPlanPickType()
        {
        }

        public ErpNoPlanPickType(int type, string typeName)
        {
            CHKTYPE_ID = type;
            CHKTYPE_NAME = typeName;
        }

        public int CHKTYPE_ID { get; private set; }

        public string CHKTYPE_NAME { get; private set; }

        public override object[] GetKeys()
        {
            return [CHKTYPE_ID, CHKTYPE_NAME];
        }
    }
}
