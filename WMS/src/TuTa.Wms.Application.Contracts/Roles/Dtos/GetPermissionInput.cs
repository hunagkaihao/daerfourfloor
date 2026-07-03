using System.ComponentModel.DataAnnotations;

namespace TuTa.Wms.Roles.Dtos
{
    public class GetPermissionInput
    {
        [Required]
        public string ProviderName { get; set; }
        [Required]
        public string ProviderKey { get; set; }
    }
}
