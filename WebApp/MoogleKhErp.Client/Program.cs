using CurrieTechnologies.Razor.SweetAlert2;
using DataLayer.Infrastructure;
using DataLayer.Repos;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using MudExtensions.Services;
using Toolbelt.Blazor.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddSweetAlert2();
builder.Services.AddHotKeys2();
builder.Services.AddMudExtensions();
builder.Services.Configure<DatabaseConfig>("MoogleKhErpSqlConnection", builder.Configuration.GetSection("DatabaseConnectionConfig:MoogleKhErpSqlConnection"));
builder.Services.Configure<DatabaseConfig>("MoogleKhErpPostgreSqlConnection", builder.Configuration.GetSection("DatabaseConnectionConfig:MoogleKhErpPostgreSqlConnection"));
builder.Services.AddSingleton<IUowMoogleKhErp, UowMoogleKhErp>();
builder.Services.AddSingleton<IUowMoogleKhErpPg, UowMoogleKhErpPg>();

builder.Services.AddMudServices();

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

await builder.Build().RunAsync();
