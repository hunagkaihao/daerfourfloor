using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TuTa.Wms.StockConsolidations;

/// <summary>
/// 库存整理调度服务的依赖注入注册扩展。
/// 调度服务把运行Task、取消令牌和页面状态保存在实例字段中，因此不能依赖应用服务的
/// 默认Transient注册；所有入口必须显式指向同一个StockConsolidationService单例。
/// </summary>
public static class StockConsolidationServiceCollectionExtensions
{
    /// <summary>
    /// 注册库存整理共享单例。
    /// 先移除ABP约定注册可能产生的接口和实现类型描述，再分别注册唯一实现对象及接口别名，
    /// 确保Controller、每日HostedService以及不同HTTP请求作用域解析到完全相同的对象引用。
    /// </summary>
    public static IServiceCollection AddStockConsolidationScheduling(this IServiceCollection services)
    {
        services.RemoveAll<StockConsolidationService>();
        services.RemoveAll<IStockConsolidationService>();

        // 实现类型只创建一次；接口注册通过工厂返回该实例，禁止再创建第二个状态容器。
        services.AddSingleton<StockConsolidationService>();
        services.AddSingleton<IStockConsolidationService>(serviceProvider =>
            serviceProvider.GetRequiredService<StockConsolidationService>());

        return services;
    }
}
