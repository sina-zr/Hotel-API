using EFDataAccessLibrary.DataAccess;
using HotelAPI.Controllers.v2.BookingServices;
using HotelAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

namespace HotelAPI.StartupConfig;

public static class ServicesExtensions
{
    /// <summary>
    /// Adding versioning to API Controllers
    /// </summary>
    /// <param name="builder"></param>
    internal static void AddVersioning(this WebApplicationBuilder builder)
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
    internal static void AddStandardServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.AddSwaggerServices();
    }
    private static void AddSwaggerServices(this WebApplicationBuilder builder)
    {
        var securityScheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "JWT Authorization header info using bearer tokens",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        };

        var securityRequirement = new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "bearerAuth"
                        }
                    },
                    new string[] {}
                }
            };

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

            opts.AddSecurityDefinition("bearerAuth", securityScheme);
            opts.AddSecurityRequirement(securityRequirement);
        });
    }

    /// <summary>
    /// Adding our DbContext to Dependency Injection system
    /// And Configuring it to use SqlServer
    /// </summary>
    /// <param name="builder"></param>
    internal static void AddEfDbContext(this WebApplicationBuilder builder)
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
    internal static void AddDependencyInjections(this WebApplicationBuilder builder)
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

    internal static void AddAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new()
                {
                    // First we tell what we want to check if is valid
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,

                    // Then we give it the already valid thing for comparing to user's token
                    ValidIssuer = builder.Configuration.GetValue<string>("Authentication:Issuer"),
                    ValidAudience = builder.Configuration.GetValue<string>("Authentication:Audience"),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
                        builder.Configuration.GetValue<string>("Authentication:SecretKey")))
                };
            });

        builder.Services.AddAuthorization(opts =>
        {
            opts.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
        });
    }
}