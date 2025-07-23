using ApiClient;
using DataBase.Collections.Castles;
using Microsoft.EntityFrameworkCore;
using Shared.Api;
using Shared.Tools;
using Shared.Tools.SimpleLogger;
using Swashbuckle.AspNetCore.Swagger;
using System.Reflection;
namespace Api.System;
public static class Setup
{
    private static readonly Events? _eventBus = new();
    private static bool _swaggerEnabled = false;
    public static void LogAndFlush(IServiceProvider services)
    {
        ILogger logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Setup");
        
        List <Event> events = _eventBus!.Get();
        foreach (Event? entry in events)
        {
            logger.LogInformation(string.Format("[{0}]  {1}", entry.Method, entry.Message), entry.Parameters);
        }

        _eventBus?.Flush();
    }   
    public static void Logger(WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        var configSection = builder.Configuration.GetSection("Logging:SimpleLogger");
        var simpleLoggerConfig = new Configuration();
        configSection.Bind(simpleLoggerConfig);

        var provider = new Provider(simpleLoggerConfig);
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(provider);

        // 🔥 THIS sets the global minimum log level
        builder.Logging.SetMinimumLevel(simpleLoggerConfig.MinimumLogLevel);
   
    }
    public static void DebugStartup(WebApplicationBuilder builder)
    {
        builder.WebHost.CaptureStartupErrors(true);
        builder.Logging.AddConsole();
    }
    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        // Get all types in the current assembly that implement IRouteHandler
        List<Type> types = Assembly.GetExecutingAssembly()
                                     .GetTypes()
                                     .Where(t => typeof(IEndPoint).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                                     .ToList();

        foreach (Type type in types)
        {
            try
            {
                // Create an instance of the route handler
                IEndPoint? request = Activator.CreateInstance(type) as IEndPoint;

                // Register the routes for the handler
                request?.ConfigureRoutes(app);

                _eventBus?.AddEvent
                (
                    nameof(Setup),
                    nameof(MapEndpoints),
                    LogLevel.Debug,
                    string.Format("/{0} [Namespace={1}]", request!.Path, type.Namespace!)
                );
            }
            catch (Exception ex)
            {
                _eventBus?.AddEvent
                (
                    nameof(Setup),
                    nameof(MapEndpoints),
                    LogLevel.Error,
                    string.Format("/{0} [Namespace={1}]: {2}", type.Name, type.Namespace!, ex.Message)
                );
            }
        }
    }
    public static void AddHttpLogging(WebApplicationBuilder builder)
    {
        builder.Services.AddHttpLogging(logging =>
        {
            logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;         
        });
    }
    public static void AddHttpClient(WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient<IRestClient, RestClient>("InternalApi", client => {});
    }
    public static void RegisterDatabase(WebApplicationBuilder builder)
    {
        string connection = ConnectionBuilder.DecryptOrGetDefault(builder.Configuration.GetConnectionString("DefaultConnection"));
        builder.Services.AddDbContext<CastleContext>(options =>
            options.UseSqlServer(connection));

        ServiceDescriptor? database = builder.Services
          .FirstOrDefault(service => service.ServiceType == typeof(CastleContext));

        _eventBus?.AddEvent
        (
            nameof(Setup),
            nameof(RegisterDatabase),
            LogLevel.Debug,
            string.Format("Db={0}=>{1}", database!.ServiceType.Name, database != null ? "OK" : "NotOk")
        );
    }   
    public static void ConfigureSwagger(WebApplicationBuilder builder)
    {
        if (!builder.Environment.IsDevelopment()) return;

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        ServiceDescriptor? swaggerService = builder.Services.FirstOrDefault(service => service.ServiceType == typeof(ISwaggerProvider));
        _swaggerEnabled = swaggerService != null;
    }
    public static void StartSwagger(WebApplication app)
    {
        if (!app.Environment.IsDevelopment() || !_swaggerEnabled) return;

        try
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        catch (Exception ex)
        {
            _eventBus?.AddEvent
            (
                nameof(Setup),
                nameof(StartSwagger),
                LogLevel.Error,
                string.Format("Swagger UI Error: {0}", ex.Message)
            );
        }
    }
    public static void UseHttpLogging(WebApplication app)
    {
        app.UseHttpLogging();
    }
}