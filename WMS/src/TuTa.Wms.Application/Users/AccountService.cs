using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using TuTa.Wms.Application.Contracts.Shared;
using Wms.LogTool;
using TuTa.Wms.Users.Dtos;
using Microsoft.AspNetCore.Authorization;
using TuTa.Wms.Permissions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Volo.Abp.Identity;
using System.Security.Claims;
using System.Text;
using Volo.Abp.Security.Claims;
using IdentityModel;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using Volo.Abp.MultiTenancy;
using IdentityUser = Volo.Abp.Identity.IdentityUser;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using UserLoginInfo = TuTa.Wms.Users.Dtos.UserLoginInfo;
using Wms.ConfigTool;
using Settings = Wms.ConfigTool.Settings;

namespace TuTa.Wms.Users
{

    //[Authorize]
    public class AccountService : WmsAppService, IAccountService
    {
        private readonly IdentityUserManager _userManager;
        private readonly Jwt _jwtOptions;
        private readonly SignInManager<Volo.Abp.Identity.IdentityUser>
    _signInManager;
        private readonly IOptions<IdentityOptions> _identityOptions;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILogger<AccountService> _logger;


        public AccountService(
                        IdentityUserManager userManager,
            //IOptionsSnapshot<JwtOptions> jwtOptions,
            IOptions<IdentityOptions> identityOptions,
            SignInManager<Volo.Abp.Identity.IdentityUser> signInManager,
                        IHttpClientFactory httpClientFactory,
            IHttpContextAccessor contextAccessor,
            ILogger<AccountService> logger)
        {
            _userManager = userManager;
            //_jwtOptions = jwtOptions.Value;
            _jwtOptions = new Jwt()
            {
                ExpirationTime = 24,
                SecurityKey = "dzehzRz9a8asdfasfdadfasdfasdfafsdadfasbasdf=",
                Audience = "WMS",
                Issuer = "WMS"
            };
            _identityOptions = identityOptions;
            _signInManager = signInManager;
            _httpClientFactory = httpClientFactory;
            _contextAccessor = contextAccessor;
            _logger = logger;
        }
        //public async Task<AbpLoginResult> Login(UserLoginInfo login)
        //{
        //    return null;
        //}

        public async Task<LoginOutput> LoginAsync(LoginInput input)
        {
            var result = await _signInManager.PasswordSignInAsync(input.Name, input.Password, false, true);
            if (result.IsNotAllowed)
            {
                throw new UserFriendlyException("当前用户已锁定");
            }

            if (!result.Succeeded)
            {
                throw new UserFriendlyException("用户名或者密码错误");
            }

            var user = await _userManager.FindByNameAsync(input.Name);
            return await BuildResult(user);
        }

        private async Task<LoginOutput> BuildResult(Volo.Abp.Identity.IdentityUser user)
        {
            if (user.LockoutEnabled) throw new UserFriendlyException("当前用户已被锁定");
            var roles = await _userManager.GetRolesAsync(user);
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
}
