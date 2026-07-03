using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace TuTa.Wms.Erp.Entities
{
    public class ErpDepartment : Entity
    {
        private ErpDepartment()
        {            
        }

        public ErpDepartment(string deptID, string deptName)
        {
            DEPT_ID = Check.NotNullOrWhiteSpace(deptID, nameof(deptID));
            DEPT_NAME = Check.NotNullOrWhiteSpace(deptName, nameof(deptName));
        }

        [StringLength(30)]
        public string DEPT_ID { get; private set; }

        [StringLength(60)]
        public string DEPT_NAME { get; private set; }

        public override object[] GetKeys()
        {
            return [DEPT_ID];
        }
    }
}
