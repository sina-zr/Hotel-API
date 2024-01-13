using AspNetCoreRateLimit;
using HealthChecks.UI.Client;
using HotelAPI.StartupConfig;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using WatchDog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

builder.AddEfLibraryDependencies();
builder.AddEFCore();
builder.AddAuthentication();
builder.AddVersioning();
builder.AddStandardServices();
builder.AddCustomHealthChecks();
builder.Services.AddWatchDogServices();
builder.AddRateLimiting();

var app = builder.Build();

//var logger = app.Services.GetService<ILogger<Program>>();

//// Subscribe to the ProcessExit event for graceful shutdown
//AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
//{
//    logger?.LogCritical("Applilcation is stopping.");
//};
//// Subscribe to the UnhandledException event for unhandled exception
//AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
//{
//    if (e.ExceptionObject is Exception ex)
//    {
//        logger?.LogError(ex, "An Unhandled exception occurred: {ErrorMessage}", ex.Message);
//    }
//};


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(opts =>
    {
        opts.SwaggerEndpoint("/swagger/v2/swagger.json", "v2");
        opts.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    });
}

app.UseIpRateLimiting();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseCustomedWatchDog();

app.UseHealtChecks();

//logger?.LogCritical("Application Started.");

//app.CreatRoles();
//app.AddManagerRole();

app.Run();

// Testing connection of github