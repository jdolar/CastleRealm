using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
namespace Shared.Tools.SimpleLogger;
public static class Configure
{
    private const string _confFileName = "appsettings.json";
    private const string _loggerName = "Logging:SimpleLogger";
    public static void ConfigureLogging(ILoggingBuilder loggingBuilder, IConfiguration configuration)
    {
        Configuration config = new();
        configuration.GetSection(_loggerName).Bind(config);

        loggingBuilder.ClearProviders();
        loggingBuilder.AddProvider(new Provider(config));
        loggingBuilder.SetMinimumLevel(config.MinimumLogLevel);
      
    }

    public static IConfigurationRoot BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .AddJsonFile(_confFileName, optional: false, reloadOnChange: true)
            .Build();
    }
}
