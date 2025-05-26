using Api.System;

Setup.SetConsoleLogger();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
Setup.ConfigureLogger(builder);
Setup.RegisterDatabases(builder);
Setup.ConfigureSwagger(builder);

WebApplication app = builder.Build();
Setup.EnableLogger(app);
Setup.MapEndpoints(app);
Setup.StartSwagger(app);

app.Run();