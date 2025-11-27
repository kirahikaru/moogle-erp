using CurrieTechnologies.Razor.SweetAlert2;
using DataLayer.Infrastructure;
using DataLayer.Repository;
using MudBlazor.Services;
using MudExtensions.Services;
using TechAdminERP.Client.Pages;
using TechAdminERP.Components;
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

builder.Services.Configure<DatabaseConfig>("ITAdminERPSqlConnection", builder.Configuration.GetSection("DatabaseConnectionConfig:ITAdminERPSqlConnection"));
builder.Services.AddSingleton<IUowErp, UowErp>();


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
    .AddAdditionalAssemblies(typeof(TechAdminERP.Client._Imports).Assembly);

app.Run();
