// <copyright file="Program.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API;
using MartialBase.API.Data;

using Utilities = MartialBase.API.Data.Utilities;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

#if DEBUG
IConfiguration configuration = Utilities.GetConfiguration("appsettings.Debug.json");
#else
IConfiguration configuration = Utilities.GetConfiguration("appsettings.json");
#endif

builder.Configuration.AddConfiguration(configuration);

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

WebApplication app = builder.Build();

startup.Configure(app, builder.Environment);

using (IServiceScope scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MartialBaseDbContext>();

#if DEBUG
    dbContext.Initialize(true);
#else
    dbContext.Initialize(false);
#endif
}

app.Run();
