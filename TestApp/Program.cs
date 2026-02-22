using Application.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            var pdf = scope.ServiceProvider.GetRequiredService<IPdfService>();
            var raw = pdf.GenerateReport(
                new Dictionary<string, bool>()
                {
                    { "Test", true },
                    { "Test2", true },
                    { "Test3", true },
                    { "Test4", true },
                    { "Test5", true },
                    { "Test6", true },
                    { "Test7", true },
                    { "Test8", true },
                    { "Test9", true },
                    { "Test10", true },
                    { "Test11", true },
                    { "Test12", true },
                    { "Test13", true },
                    { "Test14", true },
                    { "Test15", true },
                    { "Test16", true },
                },
                "Test",
                "02/03/2025"
            );
            File.WriteAllBytes("test.pdf", raw);
            Console.WriteLine("File Saved");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error Occured When Initializing Database \n" + ex.Message);
        }
    }
    #endregion
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}
finally { }
