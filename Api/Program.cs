using Api.System;

Setup.SetConsoleLogger();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
Setup.RegisterDatabases(builder);
Setup.ConfigureSwagger(builder);

WebApplication app = builder.Build();
Setup.MapEndpoints(app);
Setup.StartSwagger(app);

app.Run();