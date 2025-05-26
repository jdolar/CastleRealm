using DataBase.Collections.Castles;
using Microsoft.EntityFrameworkCore;
using Shared.Requests;
using Shared.Tools;
using Swashbuckle.AspNetCore.Swagger;
using System.Reflection;
namespace Api.System;
public static class Setup
{
    private static bool swaggerEnabled = false;
    private static readonly string? appName = string.Format("[{0}]", typeof(Setup).FullName);
    public static void ConfigureLogger(WebApplicationBuilder builder)
    {
        ConsoleLogger.Debug("{0} ConfigureLogger invoked", appName!);
        
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        builder.Services.AddHttpLogging(logging =>
        {
            logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
        });

        ConsoleLogger.Debug("{0} ConfigureLogger invoked finished with", appName!);
    }
    public static void EnableLogger(WebApplication app)
    {
        ConsoleLogger.Debug("{0} EnableLogger invoked", appName!);
        
        app.UseHttpLogging();

        ConsoleLogger.Debug("{0} EnableLogger invoked finished with", appName!);
    }
    public static void RegisterDatabases(WebApplicationBuilder builder)
    {
        ConsoleLogger.Debug("{0} RegisterDatabases invoked", appName!);

        string connection = ConnectionBuilder.DecryptOrGetDefault(builder.Configuration.GetConnectionString("DefaultConnection"));
        builder.Services.AddDbContext<CastleContext>(options =>
            options.UseSqlServer(connection));

        ServiceDescriptor? dbServices = builder.Services
            .FirstOrDefault(service => service.ServiceType == typeof(CastleContext));

        ConsoleLogger.Debug("{0} RegisterDatabases finished with: {1}=>{2}", appName!, dbServices!.ServiceType.Name, dbServices != null ? "OK" : "NotOk");
    }
    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        ConsoleLogger.Debug("{0} MapEndpoints invoked", appName!);

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
                ConsoleLogger.Debug("{0} Mapping /{1} [{2}]", appName!, request!.Path, type.Namespace!);
                request?.ConfigureRoutes(app);
            }
            catch (Exception ex)
            {
                ConsoleLogger.Error("{0} Mapping {1} [{2}] failed with message:\n{3}", appName!, type.Name, type.Namespace!, ex.Message);
            }
        }

        ConsoleLogger.Debug("{0} MapEndpoints finished with: EndPointsCount={1}", appName!, types.Count);
    }
    public static void SetConsoleLogger(bool? enableDebug = true, bool? enableSeverity = true, bool? enableLogStamps = true)
    {
        if (enableDebug.HasValue && (bool)enableDebug) ConsoleLogger.DebugEnable();

        ConsoleLogger.Debug("{0} SetConsoleLogger invoked", appName!);

        if (enableSeverity.HasValue && (bool)enableSeverity) ConsoleLogger.LogSeverity();
        if (enableLogStamps.HasValue && (bool)enableLogStamps) ConsoleLogger.LogTimeStamp();

        ConsoleLogger.Debug("{0} SetConsoleLogger finished with:\n- DebugEnabled: {1}\n- LogSeverityEnabled: {2}\n- LogTimeStampEnabled: {3}",
            appName!, ConsoleLogger.DebugEnabled(), ConsoleLogger.LogSeverityEnabled(), ConsoleLogger.LogTimeStampEnabled());
    }
    public static void ConfigureSwagger(WebApplicationBuilder builder)
    {
        if (!builder.Environment.IsDevelopment()) return;

        ConsoleLogger.Debug("{0} AddSwagger invoked", appName!);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        ServiceDescriptor? swaggerService = builder.Services
                .FirstOrDefault(service => service.ServiceType == typeof(ISwaggerProvider));

        swaggerEnabled = swaggerService != null;

        ConsoleLogger.Debug("{0} AddSwagger finished with: {1}", appName!, swaggerEnabled ? "OK" : "NotOk");
    }
    public static void StartSwagger(WebApplication app)
    {
        if (!app.Environment.IsDevelopment() || !swaggerEnabled) return;

        ConsoleLogger.Debug("{0} StartSwagger invoked", appName!);

        app.UseSwagger();
        app.UseSwaggerUI();

        ConsoleLogger.Debug("{0} StartSwagger finished.", appName!);
    }
}