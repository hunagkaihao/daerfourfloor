using Microsoft.Extensions.DependencyInjection;
using TuTa.Wms.Erp.Repositories;
using Volo.Abp.Modularity;

namespace TuTa.Wms.Erp
{
    /// <summary>
    /// ERP模块
    /// </summary>
    public class ErpModule : AbpModule
    {
            public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 仓储注册将在EntityFrameworkCore模块中处理
    }
    }
}
