using HealthChecks.UI.Client;
using HotelAPI.Models;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
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

        internal static async void CreatRoles(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider
                    .GetRequiredService<RoleManager<IdentityRole>>();

                var roles = new[] { "Manager", "Receptionist", "Guest" };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        internal static async void AddManagerRole(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider
                    .GetRequiredService<UserManager<ApplicationUser>>();

                string username = "";
                string password = "";

                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

                try
                {
                    var user = await userManager.FindByNameAsync(username);

                    if (user != null && await userManager.CheckPasswordAsync(user, password))
                    {
                        if (userManager.GetRolesAsync(user).Result.Contains("Manager"))
                        {
                            logger.LogCritical("Manager {UserName} already exists", user.UserName);
                            return;
                        }

                        await userManager.AddToRoleAsync(user, "Manager");                        

                        logger.LogCritical("{UserName} is given Manager Role", user.UserName);
                    }
                    else
                    {
                        logger.LogCritical("Attempt to Add Manager." +
                            "{username} is Null or Password is wrong.", username);
                    }
                }
                catch (Exception)
                {
                    logger.LogCritical("Something went wrong." +
                        "Failed to add Manager {username}.", username);
                }
            }
        }
    }
}
