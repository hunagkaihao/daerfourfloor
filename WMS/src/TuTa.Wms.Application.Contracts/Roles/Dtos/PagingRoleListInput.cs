using System.ComponentModel.DataAnnotations;
using TuTa.Wms.Shared;

namespace TuTa.Wms.Roles.Dtos
{
    public class PagingRoleListInput : PagingBase
    {
        public string Filter { get; set; }
    }
}
