using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TuTa.Wms.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class ErpDbContextFactory : IDesignTimeDbContextFactory<ErpDbContext>
{
    public ErpDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
#if DEBUG
        var builder = new DbContextOptionsBuilder<ErpDbContext>()
            .UseMySql(configuration.GetConnectionString("ErpConn"), MySqlServerVersion.LatestSupportedServerVersion);
        return new ErpDbContext(builder.Options);
#else
        var builder = new DbContextOptionsBuilder<ErpDbContext>()
            .UseSqlServer(configuration.GetConnectionString("ErpConn"));
        return new ErpDbContext(builder.Options); 
#endif
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../TuTa.Wms.HttpApi.Host/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
