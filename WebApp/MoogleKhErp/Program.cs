using CurrieTechnologies.Razor.SweetAlert2;
using DataLayer.Infrastructure;
using DataLayer.Repos;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MoogleKhErp.Client;
using MoogleKhErp.Client.Pages;
using MoogleKhErp.Components;
using MudBlazor.Services;
using MudExtensions.Services;
using Toolbelt.Blazor.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

//<BEGIN> Localization
builder.Services.AddControllers();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
//<-END-> Localization

//Add from https://github.com/Basaingeal/Blazor.SweetAlert2
builder.Services.AddSweetAlert2();

// <begin> MudBlazor 
builder.Services.AddMudServices();
builder.Services.AddMudExtensions();
// <end> MudBlazor

// ToolBelt.Hotkey
builder.Services.AddHotKeys2();
builder.Services.TryAddScoped<IWebAssemblyHostEnvironment, ServerHostEnvironment>();

builder.Services.Configure<DatabaseConfig>("MoogleKhErpSqlConnection", builder.Configuration.GetSection("DatabaseConnectionConfig:MoogleKhErpSqlConnection"));
builder.Services.Configure<DatabaseConfig>("MoogleKhErpPostgreSqlConnection", builder.Configuration.GetSection("DatabaseConnectionConfig:MoogleKhErpPostgreSqlConnection"));
builder.Services.AddSingleton<IUowMoogleKhErp, UowMoogleKhErp>();
builder.Services.AddSingleton<IUowMoogleKhErpPg, UowMoogleKhErpPg>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(MoogleKhErp.Client._Imports).Assembly);

app.Run();
