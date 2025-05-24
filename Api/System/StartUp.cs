using Api.System;

Setup.SetConsoleLogger();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
Setup.RegisterDatabases(builder);
Setup.ConfigureSwagger(builder);

WebApplication app = builder.Build();
Setup.MapEndpoints(app);
Setup.StartSwagger(app);

//string host = "DESKTOP-2ORAPH8";
//string instance = "SQLHOME11";
//string database = "PerfSvc";

//const string host = "localhost";
//const string instance = "test";
//const string database = "testDb";

//string encrypted = ConnectionBuilder.EncodeConnectionString(host, instance, database);
//string Decrypted = ConnectionStringBuilder.DecodeConnectionStringOrGetDefault(encrypted);;
//Crypto.Aes encrypt = new();




app.Run();