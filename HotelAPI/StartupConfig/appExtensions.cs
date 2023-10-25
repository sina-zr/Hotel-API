using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using WatchDog;

namespace HotelAPI.StartupConfig
{
    internal static class appExtensions
    {
        internal static void UseCustomedWatchDog(this WebApplication app)
        {
            string MyBlacklist = "health,healthchecks-ui";
            MyBlacklist += ",healthchecks-api,healthchecks-api/ui-settings";
            MyBlacklist += ",ui/resources,ui/resources/healthchecks-bundle.js";
            MyBlacklist += ",ui/resources/healthchecksui-min.css,ui/resources/vendors-dll.js";

            app.UseWatchDog(opts =>
            {
                opts.Blacklist = MyBlacklist; // Exclude multiple routes
                opts.WatchPageUsername = app.Configuration.GetValue<string>("WatchDog:UserName");
                opts.WatchPagePassword = app.Configuration.GetValue<string>("WatchDog:Password");
            });
        }

        internal static void UseHealtChecks(this WebApplication app)
        {
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            }).AllowAnonymous();
            app.MapHealthChecksUI();
        }
    }
}
