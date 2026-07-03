using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Volo.Abp.Account.Localization;
using Volo.Abp.Account.Settings;
using Volo.Abp.Account.Web.Areas.Account.Controllers.Models;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Identity;
using Volo.Abp.Identity.AspNetCore;
using Volo.Abp.Settings;
using Volo.Abp.Validation;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using UserLoginInfo = Volo.Abp.Account.Web.Areas.Account.Controllers.Models.UserLoginInfo;
using IdentityUser = Volo.Abp.Identity.IdentityUser;
using AccountController = Volo.Abp.Account.Web.Areas.Account.Controllers.AccountController;
using Volo.Abp.Account;
using Volo.Abp;
using TuTa.Wms.Users.Dtos;
using IdentityModel;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Volo.Abp.Security.Claims;
using System.Linq;
using Swashbuckle.AspNetCore.Annotations;
using Wms.ConfigTool;

namespace TuTa.Wms.Controllers.Accounts;


[Route("wms/account")]
[ApiController]
public class NewAccountController : AccountController
{
    protected SignInManager<IdentityUser> SignInManager { get; }
    protected IdentityUserManager UserManager { get; }
    protected ISettingProvider SettingProvider { get; }
    protected IdentitySecurityLogManager IdentitySecurityLogManager { get; }
    protected IOptions<IdentityOptions> IdentityOptions { get; }
    protected IdentityDynamicClaimsPrincipalContributorCache IdentityDynamicClaimsPrincipalContributorCache { get; }

    private readonly Jwt _jwtOptions ;
    public NewAccountController(
        SignInManager<IdentityUser> signInManager,
        IdentityUserManager userManager,
        ISettingProvider settingProvider,
        IdentitySecurityLogManager identitySecurityLogManager,
        IOptions<IdentityOptions> identityOptions,
        IdentityDynamicClaimsPrincipalContributorCache identityDynamicClaimsPrincipalContributorCache)
        :base(signInManager, userManager, settingProvider, identitySecurityLogManager, identityOptions, identityDynamicClaimsPrincipalContributorCache)
    {
        LocalizationResource = typeof(AccountResource);

        SignInManager = signInManager;
        UserManager = userManager;
        SettingProvider = settingProvider;
        IdentitySecurityLogManager = identitySecurityLogManager;
        IdentityOptions = identityOptions;
        IdentityDynamicClaimsPrincipalContributorCache = identityDynamicClaimsPrincipalContributorCache;

        _jwtOptions = new Jwt()
        {
            ExpirationTime = 24,
            SecurityKey = "dzehzRz9a8asdfasfdadfasdfasdfafsdadfasbasdf=",
            Audience = "WMS",
            Issuer = "WMS"
        };
    }
    [HttpPost("newlogin")]
    [SwaggerOperation(summary: "获取所有角色", Tags = new[] { "Roles" })]
    public async Task<LoginOutput> MyLoginAsync(LoginInput input)
    {
        var result = await SignInManager.PasswordSignInAsync(input.Name, input.Password, false, true);
        if (result.IsNotAllowed)
        {
            throw new UserFriendlyException("当前用户已锁定");
        }

        if (!result.Succeeded)
        {
            throw new UserFriendlyException("用户名或者密码错误");
        }

        var user = await UserManager.FindByNameAsync(input.Name);
        return await BuildResult(user);
    }
    private async Task<LoginOutput> BuildResult(Volo.Abp.Identity.IdentityUser user)
    {
        if (user.LockoutEnabled) throw new UserFriendlyException("当前用户已被锁定");
        var roles = await UserManager.GetRolesAsync(user);
        if (roles == null || roles.Count == 0) throw new UserFriendlyException("当前用户未分配角色");
        var token = GenerateJwt(user.Id, user.UserName, user.Name, user.Email,
            user.TenantId.ToString(), roles.ToList());
        var loginOutput = ObjectMapper.Map<Volo.Abp.Identity.IdentityUser, LoginOutput>(user);
        loginOutput.Token = token;
        loginOutput.Roles = roles.ToList();
        return loginOutput;
    }
    /// <summary>
    /// 生成jwt token
    /// </summary>
    /// <returns></returns>
    private string GenerateJwt(Guid userId, string userName, string name, string email,
        string tenantId, List<string> roles)
    {
        var dateNow = DateTime.Now;
        var expirationTime = dateNow + TimeSpan.FromHours(_jwtOptions.ExpirationTime);
        var key = Encoding.ASCII.GetBytes(_jwtOptions.SecurityKey);

        var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Audience, _jwtOptions.Audience),
                new Claim(JwtClaimTypes.Issuer, _jwtOptions.Issuer),
                new Claim(AbpClaimTypes.UserId, userId.ToString()),
                new Claim(AbpClaimTypes.Name, name),
                new Claim(AbpClaimTypes.UserName, userName),
                new Claim(AbpClaimTypes.Email, email),
                new Claim(AbpClaimTypes.TenantId, tenantId)
            };

        foreach (var item in roles)
        {
            claims.Add(new Claim(JwtClaimTypes.Role, item));
        }

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expirationTime,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };
        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }

}