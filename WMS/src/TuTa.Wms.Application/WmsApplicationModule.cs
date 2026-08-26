using Microsoft.Extensions.DependencyInjection;
using TuTa.Wms.Erp;
using TuTa.Wms.StockConsolidations;
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
        // 显式覆盖ABP应用服务的约定注册，保证启动、状态、停止和每日调度共享同一实例。
        // 必须在注册HostedService前完成，否则定时服务可能持有另一份独立的内存状态。
        context.Services.AddStockConsolidationScheduling();

        // Worker只在每次整理线程启动时由独立作用域创建，不在多个运行批次之间保存EF实体。
        context.Services.AddTransient<StockConsolidationWorker>();
        // 每日自动启动服务只负责读取配置并调用单例调度服务；移动端按钮仍调用同一个启动/停止入口。
        context.Services.AddHostedService<StockConsolidationScheduleService>();
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
