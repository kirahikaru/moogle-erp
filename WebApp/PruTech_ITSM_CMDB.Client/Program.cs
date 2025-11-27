using CurrieTechnologies.Razor.SweetAlert2;
using DataLayer.Infrastructure;
using DataLayer.Repos;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using MudExtensions.Services;
using Toolbelt.Blazor.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMudServices();
builder.Services.AddMudExtensions();
builder.Services.AddSweetAlert2();
builder.Services.AddHotKeys2();
builder.Services.Configure<DatabaseConfig>("PruITSqlConnection", builder.Configuration.GetSection("DatabaseConnectionConfig:PruITSqlConnection"));
builder.Services.Configure<DatabaseConfig>("PruITPostgreSqlConnection", builder.Configuration.GetSection("DatabaseConnectionConfig:PruITPostgreSqlConnection"));
builder.Services.AddSingleton<IUowPruIT, UowPruIT>();

await builder.Build().RunAsync();
