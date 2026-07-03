using System;
using System.Threading.Tasks;
using TuTa.Wms.Roles;
using TuTa.Wms.Roles.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Identity;

namespace TuTa.Wms.Controllers.Systems
{
    [Route("Permissions")]
    public class PermissionController : WmsController, IRolePermissionAppService
    {
        private readonly IRolePermissionAppService _rolePermissionAppService;

        public PermissionController(IRolePermissionAppService rolePermissionAppService)
        {
            _rolePermissionAppService = rolePermissionAppService;
        }


        [HttpPost("tree")]
        [SwaggerOperation(summary: "获取角色权限", Tags = new[] { "Permissions" })]
        public Task<PermissionOutput> GetPermissionAsync(GetPermissionInput input)
        {
            return _rolePermissionAppService.GetPermissionAsync(input);
        }

        [HttpPost("update")]
        [SwaggerOperation(summary: "更新角色", Tags = new[] { "Permissions" })]
        public Task UpdatePermissionAsync(UpdateRolePermissionsInput input)
        {
            return _rolePermissionAppService.UpdatePermissionAsync(input);
        }
    }
}
