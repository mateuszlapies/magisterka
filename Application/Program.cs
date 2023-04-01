using Application.Jobs;
using Application.Services;
using Application.Utils;
using Blockchain.Contexts;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Hangfire;
using Hangfire.Storage.SQLite;
using Microsoft.OpenApi.Models;
using Networking.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.WebHost.UseElectron(args);

builder.Services.AddElectron();
builder.Services.AddHangfireServer();
builder.Services.AddControllersWithViews();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Api", Version = "v1" });
});

builder.Services.AddHangfire(configuration => configuration
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSQLiteStorage(Sequal.ConnectionString()));

builder.Services.AddHostedService<HangfireJobs>();

builder.Services.AddTransient<CreateContext>();
builder.Services.AddTransient<LockContext>();
builder.Services.AddTransient<PublicContext>();
builder.Services.AddTransient<SyncContext>();

builder.Services.AddTransient<EndpointService>();
builder.Services.AddTransient<RSAService>();
builder.Services.AddTransient<UserService>();

var app = builder.Build();

if (HybridSupport.IsElectronActive)
{
    ElectronBootstrap();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
} else
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHangfireDashboard();
    app.MapHangfireDashboard();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.Run();

async void ElectronBootstrap()
{
    BrowserWindowOptions options = new()
    {
        Show = false,
        Width = 800,
        Height = 800
    };
    BrowserWindow mainWindow = await Electron.WindowManager.CreateWindowAsync(options);
    mainWindow.OnReadyToShow += () =>
    {
        mainWindow.Show();
    };
    mainWindow.OnClosed += () =>
    {
        Electron.App.Exit();
    };
    mainWindow.SetTitle("Magisterka");

    MenuItem[] menu = new MenuItem[]
    {
        new MenuItem
        {
            Label = "File",
            Submenu=new MenuItem[]
            {
                new MenuItem
                {
                    Label ="Exit",
                    Click =()=>{Electron.App.Exit();}
                }
            }
        }
    };

    Electron.Menu.SetApplicationMenu(menu);
}