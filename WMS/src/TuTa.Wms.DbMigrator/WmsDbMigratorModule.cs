using TuTa.Wms.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Modularity;

namespace TuTa.Wms.DbMigrator
{
    [DependsOn(
        typeof(AbpAutofacModule),
        typeof(WmsEntityFrameworkCoreModule),
        typeof(WmsApplicationContractsModule)
        )]
    public class WmsDbMigratorModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpBackgroundJobOptions>(options => options.IsJobExecutionEnabled = false);
        }
    }
}
