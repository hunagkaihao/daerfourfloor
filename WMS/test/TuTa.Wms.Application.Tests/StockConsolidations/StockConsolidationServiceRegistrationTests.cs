using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace TuTa.Wms.StockConsolidations;

/// <summary>
/// 库存整理调度服务注册方式的纯依赖注入测试。
/// 本测试不启动ABP测试宿主、数据库、Worker或RCS，只验证生产模块使用的注册扩展能否
/// 让启动接口、状态接口、停止接口和每日调度在不同作用域中获得同一个服务对象。
/// </summary>
public class StockConsolidationServiceRegistrationTests
{
    /// <summary>
    /// 从根容器和两个独立请求作用域分别解析服务，要求三次解析的对象引用完全相同。
    /// 如果未来有人把服务改成Transient或Scoped，本测试会直接失败，防止再次出现
    /// “后台正在运行但/status返回未启动、StartedAt为空”的状态丢失问题。
    /// </summary>
    [Fact]
    public void Should_Resolve_One_Shared_Instance_From_Root_And_Request_Scopes()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        // 使用与WmsApplicationModule完全相同的生产注册入口，避免测试复制一份伪实现。
        services.AddStockConsolidationScheduling();

        using var serviceProvider = services.BuildServiceProvider();
        var rootInstance = serviceProvider.GetRequiredService<IStockConsolidationService>();

        using var firstScope = serviceProvider.CreateScope();
        var firstScopedInstance = firstScope.ServiceProvider
            .GetRequiredService<IStockConsolidationService>();

        using var secondScope = serviceProvider.CreateScope();
        var secondScopedInstance = secondScope.ServiceProvider
            .GetRequiredService<IStockConsolidationService>();

        ReferenceEquals(rootInstance, firstScopedInstance).ShouldBeTrue(
            "根容器与HTTP请求作用域必须共享同一个库存整理调度服务实例");
        ReferenceEquals(firstScopedInstance, secondScopedInstance).ShouldBeTrue(
            "不同HTTP请求作用域必须共享同一个库存整理调度服务实例");
    }
}
