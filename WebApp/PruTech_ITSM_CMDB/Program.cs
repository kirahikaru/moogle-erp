using CurrieTechnologies.Razor.SweetAlert2;
using DataLayer.Infrastructure;
using DataLayer.Repos;
using MudBlazor.Services;
using MudExtensions.Services;
using PruTech_ITSM_CMDB.Components;
using Toolbelt.Blazor.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Add MudBlazor services
builder.Services.AddMudServices();
builder.Services.AddMudExtensions();
builder.Services.AddSweetAlert2();
builder.Services.AddHotKeys2();

builder.Services.Configure<DatabaseConfig>("PruITSqlConnection", builder.Configuration.GetSection("DatabaseConnectionConfig:PruITSqlConnection"));
builder.Services.AddSingleton<IUowPruIT, UowPruIT>();


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
    .AddAdditionalAssemblies(typeof(PruTech_ITSM_CMDB.Client._Imports).Assembly);

app.Run();
