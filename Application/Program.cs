using Application.Jobs;
using Application.Utils;
using Blockchain.Contexts;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Hangfire;
using Hangfire.Storage.SQLite;
using Networking.Hubs;
using Networking.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.WebHost.UseElectron(args);

builder.Services.AddSignalR();
builder.Services.AddElectron();
builder.Services.AddHangfireServer();
builder.Services.AddControllersWithViews();

builder.Services.AddHangfire(configuration => configuration
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSQLiteStorage(Sequal.ConnectionString()));

builder.Services.AddHostedService<HangfireJobs>();
builder.Services.AddHostedService<HubHostedService>();

builder.Services.AddTransient<Context>();
builder.Services.AddTransient<PublicContext>();

builder.Services.AddTransient<SocketService>();
builder.Services.AddTransient<HubService>();

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

app.MapHub<SyncHub>("/sync");
app.MapHub<LockHub>("/lock");

app.Run();

async void ElectronBootstrap()
{
    BrowserWindowOptions options = new BrowserWindowOptions
    {
        Show = false
    };
    BrowserWindow mainWindow = await Electron.WindowManager.CreateWindowAsync();
    mainWindow.OnReadyToShow += () =>
    {
        mainWindow.Show();
    };
    mainWindow.OnClosed += () =>
    {
        Electron.App.Exit();
    };
    mainWindow.SetTitle("Application");

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