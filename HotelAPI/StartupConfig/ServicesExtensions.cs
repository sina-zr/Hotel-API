using EFDataAccessLibrary.DataAccess;
using HotelAPI.Controllers.v1.BookingServices;
using HotelAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace HotelAPI.StartupConfig;

public static class ServicesExtensions
{
    /// <summary>
    /// Adding versioning to API Controllers
    /// </summary>
    /// <param name="builder"></param>
    public static void AddVersioning(this WebApplicationBuilder builder)
    {
        builder.Services.AddVersionedApiExplorer(opts =>
        {
            opts.GroupNameFormat = "'v'VVV";
            opts.SubstituteApiVersionInUrl = true;
        });

        builder.Services.AddApiVersioning(opts =>
        {
            opts.AssumeDefaultVersionWhenUnspecified = true;
            opts.DefaultApiVersion = new(1, 0);
            opts.ReportApiVersions = true;
        });
    }

    /// <summary>
    /// Add Controllers, EndpointsApiExplorer, SwaggerServices
    /// </summary>
    /// <param name="builder"></param>
    public static void AddStandardServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.AddSwaggerServices();
    }
    private static void AddSwaggerServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen(opts =>
        {
            var title = "HOTEL APP Demo";
            var description = "Hotel Management App MVP with API";
            var terms = new Uri("https://localhost:7127/terms");
            var license = new OpenApiLicense()
            {
                Name = "This is my license info"
            };
            var contact = new OpenApiContact()
            {
                Name = "Sina",
                Email = "sinazare1381@gmail.com",
                Url = new Uri("https://sina-zr.ir")
            };

            opts.SwaggerDoc("v2", new OpenApiInfo()
            {
                Version = "v2",
                Title = $"{title} v2",
                Description = description,
                License = license,
                TermsOfService = terms,
                Contact = contact
            });

            opts.SwaggerDoc("v1", new OpenApiInfo()
            {
                Version = "v1",
                Title = $"{title} v1",
                Description = description,
                License = license,
                TermsOfService = terms,
                Contact = contact
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            opts.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
        });
    }

    /// <summary>
    /// Adding our DbContext to Dependency Injection system
    /// And Configuring it to use SqlServer
    /// </summary>
    /// <param name="builder"></param>
    public static void AddEfDbContext(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<HotelContext>(opts =>
        {
            opts.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
        });
    }

    /// <summary>
    /// This methods adds Instances Adhering DI
    /// You can add your own Instances too.
    /// </summary>
    /// <param name="builder"></param>
    public static void AddDependencyInjections(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IHotelContext, HotelContext>();
        builder.Services.AddTransient<IRoomService, RoomService>();
        builder.Services.AddTransient<IGuestService, GuestService>();
        builder.Services.AddTransient<IBookingService, BookingService>();

        // Adding Dependency for Microsoft Identity
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<HotelContext>()
            .AddDefaultTokenProviders();
    }
}
