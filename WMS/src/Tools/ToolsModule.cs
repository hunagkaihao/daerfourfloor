using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Volo.Abp.Modularity;
using Wms.ConfigTool;
using Wms.RedisTool;

namespace Wms;

[DependsOn(
)]
public class ToolsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.Configure<ConfigOptions>(option =>
        {
            IConfigurationRoot root = new ConfigurationBuilder()
                .AddJsonFile($@"{AppDomain.CurrentDomain.BaseDirectory}appsettings.json", optional: false).Build();

            root.GetSection("Wms").Bind(option);
        });
        context.Services.AddTransient<IRedisClient, RedisClientByStaEx>();
    }
}
