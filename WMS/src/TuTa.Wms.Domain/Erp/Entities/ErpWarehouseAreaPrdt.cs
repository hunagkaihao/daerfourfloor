using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Volo.Abp.Domain.Entities;

namespace TuTa.Wms.Erp.Entities
{
    public class ErpWarehouseAreaPrdt:Entity
    {
        private ErpWarehouseAreaPrdt() { }

        [Required]
        public int ID { get; set; }

        public string DEPT_ID { get; set; }

        public string DEPT_NAME { get; set; }

        public string CLCHKLB_ID { get; set; }

        public string CLCHKLB_NAME { get;set; }

        public string PRDT_ID { get; set; }

        public string PRDT_NAME { get; set; }

        public override object[] GetKeys()
        {
            return new object[] { ID };
        }
    }
}
