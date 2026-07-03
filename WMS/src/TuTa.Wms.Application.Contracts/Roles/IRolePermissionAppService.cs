using System.Threading.Tasks;
using TuTa.Wms.Roles.Dtos;
using Volo.Abp.Application.Services;

namespace TuTa.Wms.Roles
{

    public interface IRolePermissionAppService : IApplicationService
    {

        Task<PermissionOutput> GetPermissionAsync(GetPermissionInput input);

        Task UpdatePermissionAsync(UpdateRolePermissionsInput input);
    }
}
