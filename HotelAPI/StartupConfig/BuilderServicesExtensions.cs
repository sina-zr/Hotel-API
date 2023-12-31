﻿using AspNetCoreRateLimit;
using EFDataAccessLibrary.DataAccess;
using HotelAPI.Controllers.v2.BookingServices;
using HotelAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.ServiceProcess;
using System.Text;

namespace HotelAPI.StartupConfig;

internal static class BuilderServicesExtensions
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
    internal static void AddEFCore(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<HotelContext>(opts =>
        {
            opts.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
        });
        
        // Adding Dependency for Microsoft Identity
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<HotelContext>()
            .AddDefaultTokenProviders();
    }

    /// <summary>
    /// This methods adds Instances Adhering DI
    /// You can add your own Instances too.
    /// </summary>
    /// <param name="builder"></param>
    internal static void AddEfLibraryDependencies(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IHotelContext, HotelContext>();
        builder.Services.AddTransient<IRoomService, RoomService>();
        builder.Services.AddTransient<IGuestService, GuestService>();
        builder.Services.AddTransient<IBookingService, BookingService>();
    }

    internal static void AddAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(auth =>
        {
            auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new()
                {
                    // First we tell what we want to check if is valid
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    RequireExpirationTime = true,

                    // Then we give it the already valid thing for comparing to user's token
                    ValidIssuer = builder.Configuration.GetValue<string>("Authentication:Issuer"),
                    ValidAudience = builder.Configuration.GetValue<string>("Authentication:Audience"),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
                        builder.Configuration.GetValue<string>("Authentication:SecretKey")))
                };
            });

        //builder.Services.AddAuthorization(opts =>
        //{
        //    opts.FallbackPolicy = new AuthorizationPolicyBuilder()
        //    .RequireAuthenticatedUser()
        //    .Build();
        //});
    }

    internal static void AddCustomHealthChecks(this WebApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            .AddSqlServer(builder.Configuration.GetConnectionString("Default")!);

        var startDate = DateTime.Now;
        var endDate = startDate.AddDays(1);
        builder.Services.AddHealthChecks()
            .AddUrlGroup(new Uri(
                $"https://localhost:7127/api/v2/SearchRooms?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}"),
                "API Health Check");

        builder.Services.AddHealthChecks()
            //check disk storage 1024 MB (1 GB) free minimum
            .AddDiskStorageHealthCheck(s => s.AddDrive("C:\\", 1024))
            //check 512 MB max allocated memory if exceeds
            .AddProcessAllocatedMemoryHealthCheck(512)
            //check if process is running
            .AddProcessHealthCheck("System", p => p.Length > 0)
            //check if windows service is running
            .AddWindowsServiceHealthCheck("MSSQLSERVER", s => s.Status == ServiceControllerStatus.Running);


        builder.Services.AddHealthChecksUI(opts =>
        {
            opts.AddHealthCheckEndpoint("api", "/health");
            opts.SetEvaluationTimeInSeconds(60);
            opts.SetMinimumSecondsBetweenFailureNotifications(180);
        }).AddInMemoryStorage();
    }

    internal static void AddRateLimiting(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<IpRateLimitOptions>(
            builder.Configuration.GetSection("IpRateLimiting"));

        builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
        builder.Services.AddInMemoryRateLimiting();
    }
}