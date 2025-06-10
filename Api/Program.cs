using Api.System;
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
Setup.Logger(builder);
Setup.AddHttpClient(builder);
Setup.AddHttpLogging(builder);
Setup.RegisterDatabase(builder);
Setup.ConfigureSwagger(builder);

WebApplication app = builder.Build();
Setup.UseHttpLogging(app);
Setup.MapEndpoints(app);
Setup.StartSwagger(app);
Setup.LogAndFlush(app.Services);

app.Run();