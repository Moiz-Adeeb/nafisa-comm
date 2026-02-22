using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using NLog.Web;
using Persistence.Context;
using WebApi;
using WebApi.Initializer;

try
{
    var builder = WebApplication.CreateBuilder(args);

    var startUp = new Startup(builder.Configuration);
    // Add services to the container.

    #region Add To Service
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();
    var services = builder.Services;
    startUp.ConfigureServices(services);

    #endregion

    #region Configure Service

    // var urls = builder.Configuration.GetSection("Urls")?.Value;
    // if (urls != null && urls.IsNotNullOrWhiteSpace())
    // {
    //     builder.WebHost.UseUrls(urls);
    // }
    builder.WebHost.UseIIS().UseContentRoot(Directory.GetCurrentDirectory()).UseIISIntegration();
    var app = builder.Build();

    startUp.Configure(app, app.Environment);
    using (IServiceScope scope = app.Services.CreateScope())
    {
        try
        {
#if DEBUG
            // var m = scope.ServiceProvider.GetService<IMediator>();
            // await TestFunc(m);
#endif
            // await scope.ServiceProvider.GetService<IPassKitService>().SetPoint("5hBkED2Z7njrbOtpWqoYxn",100);
            // await scope.ServiceProvider.GetService<IPassKitService>().GetMemberById("3lSfbYqpBXfTSuvkBZBWBQ||EwLljfxm||1706046972008||14883108");
            // await scope.ServiceProvider.GetService<IPassKitService>().GetMemberById("3lSfbYqpBXfTSuvkBZBWBQ");
            //await scope.ServiceProvider.GetService<IPassKitService>().BurnPoint("5hBkED2Z7njrbOtpWqoYxn",1000);
            await scope.ServiceProvider.GetService<ApplicationDbContext>().Database.MigrateAsync();
            var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
            await DatabaseInitializer.Initialize(context);
            Console.WriteLine("Database initialized");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error Occured When Initializing Database \n" + ex.Message);
        }
    }
    await app.RunAsync();
    #endregion
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}
finally { }
