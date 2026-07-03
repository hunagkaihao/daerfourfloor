using Localization.Resources.AbpUi;
using Microsoft.Extensions.DependencyInjection;
using TuTa.Wms.Localization;
using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.HttpApi;
using Volo.Abp.TenantManagement;

namespace TuTa.Wms;

[DependsOn(
    typeof(WmsApplicationContractsModule),
    typeof(AbpAccountHttpApiModule),
    typeof(AbpIdentityHttpApiModule),
    typeof(AbpTenantManagementHttpApiModule),
    typeof(AbpPermissionManagementHttpApiModule)
    )]
public class WmsHttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        ConfigureLocalization();
        
        // 注册HttpClient
        context.Services.AddHttpClient();
    }

    private void ConfigureLocalization()
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<WmsResource>()
                .AddBaseTypes(
                    typeof(AbpUiResource)
                );
        });
    }
}