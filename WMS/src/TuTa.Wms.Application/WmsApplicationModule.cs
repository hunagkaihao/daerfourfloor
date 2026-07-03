using Microsoft.Extensions.DependencyInjection;
using TuTa.Wms.Erp;
using Volo.Abp.Account;
using Volo.Abp.AutoMapper;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.TenantManagement;
using Wms;

namespace TuTa.Wms;

[DependsOn(
    typeof(ToolsModule),
    typeof(WmsDomainModule),
    typeof(ErpModule),
    typeof(AbpAccountApplicationModule),
    typeof(WmsApplicationContractsModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpTenantManagementApplicationModule)
    )]
public class WmsApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<WmsApplicationModule>();  
        });
        //context.Services.AddHostedService<ErpMaterialSyncJob>(); // 暂时注释掉，等类型问题解决后再启用
        //context.Services.AddHostedService<ErpPickOrderSyncJob>();
        //context.Services.AddHostedService<ErpRecheckNotifierSyncJob>();
        //context.Services.AddHostedService<ErpStockAftChkSyncJob>();
        //context.Services.AddHostedService<ErpDepartmentSyncJob>();
        //context.Services.AddHostedService<ErpStateChgSyncJob>();
        //context.Services.AddHostedService<ErpBarcodeSyncJob>();
        //context.Services.AddHostedService<ErpMoveSyncJob>();
        //context.Services.AddHostedService<PickListBackGroundService>();
    }
}
