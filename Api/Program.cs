using Api.System;

//Setup.SetConsoleLogger();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
Setup.ConfigureLogger(builder);

Setup.RegisterDatabases(builder);
Setup.ConfigureSwagger(builder);

WebApplication app = builder.Build();
Setup.EnableHttpLogging(app);
Setup.MapEndpoints(app);
Setup.StartSwagger(app);
Setup.LogAndFlush(app.Services);
app.Run();