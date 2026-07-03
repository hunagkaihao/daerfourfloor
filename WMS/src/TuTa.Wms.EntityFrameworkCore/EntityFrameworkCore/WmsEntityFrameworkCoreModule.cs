using Microsoft.Extensions.DependencyInjection;
using TuTa.Wms.Erp.Repositories;
using TuTa.Wms.EntityFrameworkCore.Repositories.Erp;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.MySQL;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace TuTa.Wms.EntityFrameworkCore;

[DependsOn(
    typeof(WmsDomainModule),
    typeof(AbpIdentityEntityFrameworkCoreModule),
    typeof(AbpOpenIddictEntityFrameworkCoreModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpTenantManagementEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreMySQLModule),
    //typeof(AbpBackgroundJobsEntityFrameworkCoreModule),
    typeof(AbpAuditLoggingEntityFrameworkCoreModule)
    )]
public class WmsEntityFrameworkCoreModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        WmsEfCoreEntityExtensionMappings.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<WmsDbContext>(options =>
        {
                /* Remove "includeAllEntities: true" to create
                 * default repositories only for aggregate roots */
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        context.Services.AddAbpDbContext<ErpDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        // 注册ERP工位叫料任务仓储
        context.Services.AddTransient<IErpWorkstationMaterialRequestRepository, EfCoreErpWorkstationMaterialRequestRepository>();
        
        // 注册ERP工位收料仓储
                    context.Services.AddTransient<IErpWorkstationMaterialReceiptRepository, EfCoreErpWorkstationMaterialReceiptRepository>();
            context.Services.AddTransient<IErpWorkshopMaterialTransferRepository, EfCoreErpWorkshopMaterialTransferRepository>();

        Configure<AbpDbContextOptions>(options =>
        {
            /* The main point to change your DBMS.
             * See also WmsMigrationsDbContextFactory for EF Core tooling. */
//#if DEBUG
//            options.UseMySQL();
//#else
            options.UseMySQL<WmsDbContext>();
            options.UseMySQL<ErpDbContext>();
            options.UseMySQL<PermissionManagementDbContext>();
//#endif
        });

        Configure<AbpDbContextOptions>(options =>
        {
            options.UseMySQL();
        });

    }
}
