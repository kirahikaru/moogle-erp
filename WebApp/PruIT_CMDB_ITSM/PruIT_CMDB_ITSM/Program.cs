using CurrieTechnologies.Razor.SweetAlert2;
using DataLayer.Infrastructure;
using DataLayer.Repos;
using MudBlazor.Services;
using MudExtensions.Services;
using PruIT_CMDB_ITSM.Client.Pages;
using PruIT_CMDB_ITSM.Components;
using Toolbelt.Blazor.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents()
	.AddInteractiveWebAssemblyComponents();

builder.Services.AddSweetAlert2();
builder.Services.AddHotKeys2();
builder.Services.Configure<DatabaseConfig>("PruITSqlConnection", builder.Configuration.GetSection("DatabaseConnectionConfig:PruITSqlConnection"));
builder.Services.Configure<DatabaseConfig>("PruITPostgreSqlConnection", builder.Configuration.GetSection("DatabaseConnectionConfig:PruITPostgreSqlConnection"));
builder.Services.AddSingleton<IUowPruIT, UowPruIT>();
builder.Services.AddMudServices();
builder.Services.AddMudExtensions();

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
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode()
	.AddInteractiveWebAssemblyRenderMode()
	.AddAdditionalAssemblies(typeof(PruIT_CMDB_ITSM.Client._Imports).Assembly);

app.Run();
