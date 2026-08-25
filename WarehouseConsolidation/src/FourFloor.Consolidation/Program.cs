using FourFloor.Consolidation.Api;
using FourFloor.Consolidation.Clients;
using FourFloor.Consolidation.Configuration;
using FourFloor.Consolidation.Execution;
using FourFloor.Consolidation.Persistence;
using FourFloor.Consolidation.Planning;
using FourFloor.Consolidation.Services;
using FourFloor.Consolidation.Snapshot;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsService(options => options.ServiceName = "FourFloorWarehouseConsolidation");

builder.Services.Configure<WmsApiOptions>(builder.Configuration.GetSection(WmsApiOptions.SectionName));
builder.Services.Configure<ConsolidationOptions>(builder.Configuration.GetSection(ConsolidationOptions.SectionName));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var configuredConnectionString = builder.Configuration.GetConnectionString("Consolidation")
                                 ?? "Data Source=data/consolidation.db";
if (string.Equals(configuredConnectionString, "Data Source=data/consolidation.db", StringComparison.OrdinalIgnoreCase))
{
    configuredConnectionString = $"Data Source={Path.Combine(builder.Environment.ContentRootPath, "data", "consolidation.db")}";
}
builder.Services.AddDbContext<ConsolidationDbContext>(options =>
    options.UseSqlite(configuredConnectionString));

builder.Services.AddHttpClient<WmsStockClient>(ConfigureWmsHttpClient);
builder.Services.AddHttpClient<WmsCellClient>(ConfigureWmsHttpClient);
builder.Services.AddHttpClient<WmsBoxClient>(ConfigureWmsHttpClient);
builder.Services.AddHttpClient<WmsAgvTaskClient>(ConfigureWmsHttpClient);

builder.Services.AddSingleton<CellCodeParser>();
builder.Services.AddSingleton<SShapeCellOrderBuilder>();
builder.Services.AddSingleton<PalletGroupBuilder>();
builder.Services.AddSingleton<ConsolidationPlanner>();
builder.Services.AddSingleton<PlanSimulator>();
builder.Services.AddScoped<WarehouseSnapshotBuilder>();
builder.Services.AddScoped<ConsolidationOrchestrator>();
builder.Services.AddScoped<PlanExecutionService>();
builder.Services.AddHostedService<ConsolidationExecutionWorker>();

var app = builder.Build();

Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "data"));
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ConsolidationDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception exception)
    {
        if (context.Response.HasStarted)
        {
            throw;
        }

        context.Response.StatusCode = exception switch
        {
            KeyNotFoundException => StatusCodes.Status404NotFound,
            ConsolidationPlanningException => StatusCodes.Status409Conflict,
            WmsApiException => StatusCodes.Status502BadGateway,
            InvalidOperationException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
        await context.Response.WriteAsJsonAsync(new { error = exception.Message });
    }
});

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api/consolidation") &&
        !context.Request.Path.Equals("/api/consolidation/config"))
    {
        var options = context.RequestServices.GetRequiredService<IOptions<ConsolidationOptions>>().Value;
        if (!string.IsNullOrWhiteSpace(options.OperatorKey))
        {
            var suppliedKey = context.Request.Headers["X-Consolidation-Key"].FirstOrDefault();
            if (!string.Equals(suppliedKey, options.OperatorKey, StringComparison.Ordinal))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = "整理操作密钥无效。" });
                return;
            }
        }
    }

    await next();
});

app.MapConsolidationEndpoints();
app.MapGet("/health", () => Results.Ok(new { status = "ok", utc = DateTime.UtcNow }));

app.Run();

static void ConfigureWmsHttpClient(IServiceProvider serviceProvider, HttpClient client)
{
    var options = serviceProvider.GetRequiredService<IOptions<WmsApiOptions>>().Value;
    if (string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        throw new InvalidOperationException("WmsApi:BaseUrl 未配置。");
    }

    client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
    client.Timeout = TimeSpan.FromSeconds(Math.Max(1, options.RequestTimeoutSeconds));
}
