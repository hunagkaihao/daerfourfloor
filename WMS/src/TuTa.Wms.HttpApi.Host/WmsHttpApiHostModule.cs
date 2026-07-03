using Medallion.Threading;
using Medallion.Threading.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TuTa.Wms.AgvTasks.Aggregaes;
using TuTa.Wms.EntityFrameworkCore;
using TuTa.Wms.MultiTenancy;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.AntiForgery;
using Volo.Abp.AspNetCore.Mvc.UI.MultiTenancy;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Auditing;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Caching;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Identity.AspNetCore;
using Volo.Abp.Identity.Settings;
using Volo.Abp.Modularity;
using Volo.Abp.Security.Claims;
using Volo.Abp.Settings;
using Volo.Abp.Swashbuckle;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;
using Wms.LogTool;

namespace TuTa.Wms;

[DependsOn(
    typeof(WmsHttpApiModule),
    typeof(AbpAutofacModule),
    typeof(AbpCachingStackExchangeRedisModule),
    typeof(AbpDistributedLockingModule),
    typeof(AbpAspNetCoreMvcUiMultiTenancyModule),
    typeof(WmsApplicationModule),
    typeof(WmsEntityFrameworkCoreModule),
    //typeof(AbpAccountWebOpenIddictModule),//�û���¼
        //typeof(AbpAccountApplicationContractsModule),
    typeof(AbpIdentityAspNetCoreModule),
    //    typeof(AbpAspNetCoreMvcUiThemeSharedModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpAuditLoggingEntityFrameworkCoreModule)
)]
public class WmsHttpApiHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        // Serilog日志配置已在Program.cs中完成

        Configure<AbpAuditingOptions>(options =>
        {
            options.IsEnabled = true; //Disables the auditing system
        });

        ConfigureUrls(configuration);
        //����ȡ��post�ظ�������֤
        Configure<AbpAntiForgeryOptions>(options =>
        {
            options.AutoValidate = false;

        });
        //ConfigureConventionalControllers();  //�Զ�API
        ConfigureAuthentication(context, configuration);
        ConfigureCache(configuration);
        ConfigureVirtualFileSystem(context);
        ConfigureDataProtection(context, configuration, hostingEnvironment);
        ConfigureDistributedLocking(context, configuration);
        ConfigureCors(context, configuration);
        ConfigureSwaggerServices(context, configuration);
        ConfigureOptions(context);
    }

    /// <summary>
    /// ����options
    /// </summary>
    /// <param name="context"></param>
    private void ConfigureOptions(ServiceConfigurationContext context)
    {
        //���ӻ�ȡAGV����
        context.Services.Configure<AGVOptions>(context.Services.GetConfiguration()
            .GetSection("AGV"));
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
            //options.RedirectAllowedUrls.AddRange(configuration["App:RedirectAllowedUrls"].Split(','));
        });
    }

    private void ConfigureCache(IConfiguration configuration)
    {
        Configure<AbpDistributedCacheOptions>(options => { options.KeyPrefix = "Wms:"; });
    }

    private void ConfigureVirtualFileSystem(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<WmsDomainSharedModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}TuTa.Wms.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<WmsDomainModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}TuTa.Wms.Domain"));
                options.FileSets.ReplaceEmbeddedByPhysical<WmsApplicationContractsModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}TuTa.Wms.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<WmsApplicationModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}TuTa.Wms.Application"));
            });
        }
    }

    private void ConfigureConventionalControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(WmsApplicationModule).Assembly, options =>
            {
                options.RootPath = "Wms";
            });
        });
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context, IConfiguration configuration)
    {
        //context.Services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        //context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        //{
        //    options.IsDynamicClaimsEnabled = true;
        //});
        //context.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        //.AddJwtBearer(options =>
        //{
        //    options.Authority = configuration["AuthServer:Authority"];
        //    options.RequireHttpsMetadata = configuration.GetValue<bool>("AuthServer:RequireHttpsMetadata");
        //    options.Audience = "Wms";
        //});
        context.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters =
                new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    // �Ƿ���ǩ����֤
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    //ClockSkew = TimeSpan.Zero,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.ASCII.GetBytes(configuration["Jwt:SecurityKey"]))
                };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = currentContext =>
                {
                    var path = currentContext.HttpContext.Request.Path;
                    if (path.StartsWithSegments("/login"))
                    {
                        return Task.CompletedTask;
                    }

                    var accessToken =
                        currentContext.Request.Query["access_token"].FirstOrDefault() ??
                        currentContext.Request.Cookies["Wms"];

                    if (accessToken.IsNullOrWhiteSpace())
                    {
                        return Task.CompletedTask;
                    }

                    if (path.StartsWithSegments("/signalr"))
                    {
                        currentContext.Token = accessToken;
                    }

                    currentContext.Request.Headers.Remove("Authorization");
                    currentContext.Request.Headers.Add("Authorization",
                        $"Bearer {accessToken}");

                    //// �����������hangfire ����cap
                    //if (path.ToString().StartsWith("/hangfire") ||
                    //    path.ToString().StartsWith("/cap"))
                    //{
                    //    // currentContext.HttpContext.Response.Headers.Remove(
                    //    //     "X-Frame-Options");
                    //    currentContext.Token = accessToken;
                    //}


                    return Task.CompletedTask;
                }
            };
        });
        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
        });
    }

    private static void ConfigureSwaggerServices(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddAbpSwaggerGenWithOAuth(
            configuration["AuthServer:Authority"]!,
            new Dictionary<string, string>
            {
                    {"Wms", "Wms API"}
            },
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Wms API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);
                
                // 添加 XML 注释支持
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
                
                // 启用注解支持
                options.EnableAnnotations();
                
                //20240626添加SwaggerToken登录
                options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme,
                    new OpenApiSecurityScheme()
                    {
                        Description = "直接输入JWT生成的Token",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http,
                        Scheme = JwtBearerDefaults.AuthenticationScheme,
                        BearerFormat = "JWT"
                    });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                     {
                         new OpenApiSecurityScheme
                         {
                             Reference = new OpenApiReference
                             {
                                 Type = ReferenceType.SecurityScheme, Id = "Bearer"
                             }
                         },
                         new List<string>()
                     }
                });
            });


        //context.Services.AddAbpSwaggerGen(
        //       options =>
        //       {
        //           options.SwaggerDoc("Wms",
        //               new OpenApiInfo { Title = "Wms API", Version = "v1" });
        //           options.DocInclusionPredicate((docName, description) => true);
        //           //options.EnableAnnotations(); // ����ע��
        //           //options.DocumentFilter<HiddenAbpDefaultApiFilter>();
        //           //options.SchemaFilter<EnumSchemaFilter>();
        //           //// ��������xmlע�ͣ�����ᵼ��swagger�����е㻺��
        //           //var xmls = Directory.GetFiles(AppContext.BaseDirectory, "*.xml");
        //           //foreach (var xml in xmls)
        //           //{
        //           //    options.IncludeXmlComments(xml, true);
        //           //}

        //           //options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme,
        //           //    new OpenApiSecurityScheme()
        //           //    {
        //           //        Description = "ֱ�����¿�����JWT���ɵ�Token",
        //           //        Name = "Authorization",
        //           //        In = ParameterLocation.Header,
        //           //        Type = SecuritySchemeType.Http,
        //           //        Scheme = JwtBearerDefaults.AuthenticationScheme,
        //           //        BearerFormat = "JWT"
        //           //    });
        //           //options.AddSecurityRequirement(new OpenApiSecurityRequirement
        //           //{
        //           //     {
        //           //         new OpenApiSecurityScheme
        //           //         {
        //           //             Reference = new OpenApiReference
        //           //             {
        //           //                 Type = ReferenceType.SecurityScheme, Id = "Bearer"
        //           //             }
        //           //         },
        //           //         new List<string>()
        //           //     }
        //           //});

        //           //options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme()
        //           //{
        //           //    Type = SecuritySchemeType.ApiKey,
        //           //    In = ParameterLocation.Header,
        //           //    Name = "Accept-Language",
        //           //    Description = "���������ã�ϵͳԤ��������zh-Hans��en��Ĭ��Ϊzh-Hans"
        //           //});

        //           //options.AddSecurityRequirement(new OpenApiSecurityRequirement
        //           //{
        //           //     {
        //           //         new OpenApiSecurityScheme
        //           //         {
        //           //             Reference = new OpenApiReference
        //           //                 { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
        //           //         },
        //           //         new string[] { }
        //           //     }
        //           //});
        //       });
    }

    private void ConfigureDataProtection(
        ServiceConfigurationContext context,
        IConfiguration configuration,
        IWebHostEnvironment hostingEnvironment)
    {
        var dataProtectionBuilder = context.Services.AddDataProtection().SetApplicationName("Wms");
        if (!hostingEnvironment.IsDevelopment())
        {
            var redis = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]!);
            dataProtectionBuilder.PersistKeysToStackExchangeRedis(redis, "Wms-Protection-Keys");
        }
    }

    private void ConfigureDistributedLocking(
        ServiceConfigurationContext context,
        IConfiguration configuration)
    {
        context.Services.AddSingleton<IDistributedLockProvider>(sp =>
        {
            var connection = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]!);
            return new RedisDistributedSynchronizationProvider(connection.GetDatabase());
        });
    }

    private void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(configuration["App:CorsOrigins"]?
                        .Split(",", StringSplitOptions.RemoveEmptyEntries)
                        .Select(o => o.RemovePostFix("/"))
                        .ToArray() ?? Array.Empty<string>())
                    .WithAbpExposedHeaders()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    public override async void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        //20240626�������븴�Ӷ�����
        (await app.ApplicationServices.GetService<ISettingDefinitionManager>().GetAsync(IdentitySettingNames.Password.RequireNonAlphanumeric)).DefaultValue =false.ToString();
        (await app.ApplicationServices.GetService<ISettingDefinitionManager>().GetAsync(IdentitySettingNames.Password.RequireUppercase)).DefaultValue = false.ToString();
        (await app.ApplicationServices.GetService<ISettingDefinitionManager>().GetAsync(IdentitySettingNames.Password.RequireLowercase)).DefaultValue = false.ToString();
        (await app.ApplicationServices.GetService<ISettingDefinitionManager>().GetAsync(IdentitySettingNames.Password.RequiredLength)).DefaultValue = 6.ToString();
        (await app.ApplicationServices.GetService<ISettingDefinitionManager>().GetAsync(IdentitySettingNames.Password.RequireDigit)).DefaultValue = false.ToString();
        app.UseAbpRequestLocalization();
        app.UseCorrelationId();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors(x => x
            .AllowAnyMethod()
            .AllowAnyHeader()
            .SetIsOriginAllowed(origin => true) // allow any origin
            .AllowCredentials()); // allow credentials
        app.UseAuthentication();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }

        app.UseUnitOfWork();
        app.UseDynamicClaims();
        app.UseAuthorization();

        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Wms API");

            //var configuration = context.GetConfiguration();            
            //options.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
            //options.OAuthScopes("Wms");
            options.DocExpansion(DocExpansion.None);
            options.DefaultModelsExpandDepth(-1);
        });

        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();


        app.UseAuditing();
    }
}

