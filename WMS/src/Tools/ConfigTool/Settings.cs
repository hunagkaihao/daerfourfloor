using Microsoft.Extensions.Configuration;
using System;

namespace Wms.ConfigTool;

public static class Settings
{
    public static ConfigOptions Options = new ConfigOptions();
    public static Jwt JwtOptions = new Jwt();

    static Settings()
    {
        IConfigurationRoot root = new ConfigurationBuilder()
                .AddJsonFile($@"{AppDomain.CurrentDomain.BaseDirectory}appsettings.json", optional: false).Build();

        root.GetSection("Wms").Bind(Options);
        root.GetSection("Jwt").Bind(JwtOptions);
    }
}
