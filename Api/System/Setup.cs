using DataBase.Collections.Castles;
using Microsoft.EntityFrameworkCore;
using Shared.Requests;
using Shared.Tools.SimpleLogger;
using Swashbuckle.AspNetCore.Swagger;
using System.Reflection;
namespace Api.System;
public sealed class LogSetup
{
    public int Id;
    public string? ClassName;
    public string? Method;
    public LogLevel? LogLevel;
    public string? Message;
    public object? Parameters;
    public LogSetup(int id, string className, string method, LogLevel loglevel, string message, object? parameters = null)
    {
        Id = id;
        ClassName = className;
        Method = method;
        LogLevel = loglevel;
        Message = message;
        Parameters = parameters;
    }
    public override string ToString()
    {
        return string.Format("{0} [{1}] {2}.{3} | {4}{5}", Id, LogLevel, ClassName, Method, Message, Parameters);
    }
}
public static class Setup
{
    private static List<LogSetup>? logSetup = new();
    private static bool swaggerEnabled = false;
    private static readonly string? appName = string.Format("[{0}]", typeof(Setup).FullName);
    public static void LogAndFlush(IServiceProvider services)
    {
        ILogger logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Setup");
        logger.LogInformation("ba");
        foreach (var entry in logSetup!)
        {
            bool enabled= logger.IsEnabled((LogLevel)entry.LogLevel!);
            bool enabled2 = logger.IsEnabled(LogLevel.Information);
            logger.LogDebug(string.Format("{0}.{1}: {2}", entry.ClassName, entry.Method, entry.Message), entry.Parameters);
        }

        logSetup.Clear();
    }
    
    public static void ConfigureLogger(WebApplicationBuilder builder, bool? enableDebug = true, bool? enableSeverity = true, bool? enableLogStamps = true, bool? enableFileLogging = true)
    {
        Configuration logConfig = new()
        {
            // To-Do : read from appsettings.json
            MinLogLevel = enableDebug.HasValue && (bool)enableDebug ? LogLevel.Debug : LogLevel.Information,
            IncludeSeverity = enableSeverity.HasValue && (bool)enableSeverity,
            IncludeTimestamp = enableLogStamps.HasValue && (bool)enableLogStamps,
            EnableFileLogging = enableFileLogging.HasValue && (bool)enableFileLogging,
            LogFilePath = Path.Combine(AppContext.BaseDirectory, "logs.txt")
        };

        Provider provider = new(logConfig);

        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(provider);

        builder.Services.AddHttpLogging(logging =>
        {
            logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
        });

        logSetup?.Add(new LogSetup
        (
            logSetup!.Count,
            nameof(Setup),
            nameof(ConfigureLogger),
            LogLevel.Debug,
            string.Format("Logger: Provider={0} [Config={1}]", provider, logConfig)
        ));
    }
    public static void EnableHttpLogging(WebApplication app)
    {
        app.UseHttpLogging();

        logSetup?.Add(new LogSetup
        (
            logSetup!.Count,
            nameof(Setup),
            nameof(EnableHttpLogging),
            LogLevel.Debug,
            "HTTP Logging enabled"
        ));
    }
    public static void RegisterDatabases(WebApplicationBuilder builder)
    {
        string connection = ConnectionBuilder.DecryptOrGetDefault(builder.Configuration.GetConnectionString("DefaultConnection"));
        builder.Services.AddDbContext<CastleContext>(options =>
            options.UseSqlServer(connection));

        ServiceDescriptor? database = builder.Services
          .FirstOrDefault(service => service.ServiceType == typeof(CastleContext));
        
        logSetup?.Add(new LogSetup
        (
            logSetup!.Count,
            nameof(Setup),
            nameof(RegisterDatabases),
            LogLevel.Debug,
            string.Format("Db={0}=>{1}", database!.ServiceType.Name, database != null ? "OK" : "NotOk")
        ));
    }
    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        // Get all types in the current assembly that implement IRouteHandler
        List<Type> types = Assembly.GetExecutingAssembly()
                                     .GetTypes()
                                     .Where(t => typeof(IRequest).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                                     .ToList();

        foreach (Type type in types)
        {
            try
            {
                // Create an instance of the route handler
                IRequest? request = Activator.CreateInstance(type) as IRequest;

                // Register the routes for the handler
                request?.ConfigureRoutes(app);

                logSetup?.Add(new LogSetup
                (
                    logSetup!.Count,
                    nameof(Setup),
                    nameof(MapEndpoints),
                    LogLevel.Debug,
                    string.Format("/{0} [Namespace={1}]", request!.Path, type.Namespace!)
                ));
            }
            catch (Exception ex)
            {
                logSetup?.Add(new LogSetup
                (
                    logSetup!.Count,
                    nameof(Setup),
                    nameof(MapEndpoints),
                    LogLevel.Error,
                    string.Format("/{0} [Namespace={1}]: {2}", type.Name, type.Namespace!, ex.Message)
                ));
            }
        }
    }
    public static void ConfigureSwagger(WebApplicationBuilder builder)
    {
        if (!builder.Environment.IsDevelopment()) return;

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        ServiceDescriptor? swaggerService = builder.Services.FirstOrDefault(service => service.ServiceType == typeof(ISwaggerProvider));
        swaggerEnabled = swaggerService != null;

        logSetup?.Add(new LogSetup
        (
            logSetup!.Count,
            nameof(Setup),
            nameof(ConfigureSwagger),
            LogLevel.Debug,
            string.Format("Swagger [Enabled={0}]", swaggerEnabled)
        ));
    }
    public static void StartSwagger(WebApplication app)
    {
        if (!app.Environment.IsDevelopment() || !swaggerEnabled) return;

        app.UseSwagger();
        app.UseSwaggerUI();
    }
}