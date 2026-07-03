using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TuTa.Wms;

public class Program
{
    public async static Task<int> Main(string[] args)
    {
        // 获取应用程序基目录
        var baseDirectory = AppContext.BaseDirectory;
        // 构建日志目录路径（应用程序目录下的logs文件夹）
        var logDirectory = Path.Combine(baseDirectory, "logs");
        // 确保日志目录存在
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }
        var logFilePath = Path.Combine(logDirectory, "log-.txt");

        // 配置Serilog
        Serilog.Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            // 过滤掉 EF Core 的数据库命令日志
            .Filter.ByExcluding(evt => 
                evt.Properties.TryGetValue("SourceContext", out var sourceContext) && 
                sourceContext.ToString().Contains("Microsoft.EntityFrameworkCore.Database.Command"))
            .WriteTo.Console()
            .WriteTo.File(logFilePath, 
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        try
        {
            var builder = WebApplication.CreateBuilder(args);
            //builder.Host.AddAppSettingsSecretsJson();
            builder.Host.UseAutofac();
            builder.Host.UseSystemd();
            builder.Host.UseSerilog(); // 使用Serilog
            await builder.AddApplicationAsync<WmsHttpApiHostModule>();

            var configuration = builder.Services.GetConfiguration();
            string[] urls = configuration["Wms:BaseUrl"].Split(",", System.StringSplitOptions.RemoveEmptyEntries);
            builder.WebHost.UseUrls(urls);

            var app = builder.Build();
            await app.InitializeApplicationAsync();
            await app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            if (ex is HostAbortedException)
            {
                throw;
            }
            Console.ReadLine();
            return 1;
        }
    }
}
