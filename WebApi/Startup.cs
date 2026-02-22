using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;
using Application.Infrastructures;
using Application.Interfaces;
using Application.Services.Users.Queries;
using Domain.Entities;
using FluentValidation;
using Infrastructure.Options;
using Infrastructure.Services;
using MediatR;
using MediatR.Pipeline;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NSwag;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using Persistence.Context;
using WebApi.Authorization;
using WebApi.Extension;
using WebApi.Filters;
using WebApi.Formatters;
using WebApi.Hubs;
using WebApi.Services;
using WebApi.Services.Background;

namespace WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.ConfigureWarnings(warning =>
                {
                    warning.Ignore(RelationalEventId.PendingModelChangesWarning);
                });
                var connectionString = Configuration["ConnectionStrings:DefaultConnection"];
                options
                    .UseNpgsql(
                        connectionString,
                        b =>
                        {
                            b.MigrationsAssembly("Persistence");
                        }
                    )
                    .UseCamelCaseNamingConvention();
                //options.UseInMemoryDatabase("ChatAppInMemoryDb");
                options.UseOpenIddict();
            });

            services
                .AddIdentityCore<User>(options => // Using default IdentityRole
                {
                    options.User.RequireUniqueEmail = true;
                    options.Password.RequireDigit = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequiredLength = 4;
                    options.Lockout.MaxFailedAccessAttempts = 3;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();
            //services
            //    .AddIdentity<User, Role>(options =>
            //    {
            //        options.User.RequireUniqueEmail = true;
            //        options.Password.RequireDigit = true;
            //        options.Password.RequireNonAlphanumeric = false;
            //        options.Password.RequireUppercase = false;
            //        options.Password.RequireLowercase = false;
            //        options.Password.RequiredLength = 4;
            //        options.Lockout.MaxFailedAccessAttempts = 3;
            //        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
            //    })
            //    .AddEntityFrameworkStores<ApplicationDbContext>()
            //    .AddDefaultTokenProviders();
            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromDays(2);
            });

            services
                .AddOpenIddict()
                .AddCore(options =>
                {
                    options.UseEntityFrameworkCore().UseDbContext<ApplicationDbContext>();
                })
                .AddServer(options =>
                {
                    options
                        .UseAspNetCore()
                        .EnableTokenEndpointPassthrough()
                        .DisableTransportSecurityRequirement();
                    options.SetTokenEndpointUris("/connect/token");
                    options.AllowPasswordFlow();
                    options.AllowRefreshTokenFlow();
                    options.AcceptAnonymousClients();
                    // options.DisableHttpsRequirement(); // Note: Comment this out in production
                    options.RegisterScopes(
                        OpenIddictConstants.Scopes.OpenId,
                        OpenIddictConstants.Scopes.Email,
                        OpenIddictConstants.Scopes.Phone,
                        OpenIddictConstants.Scopes.Profile,
                        OpenIddictConstants.Scopes.OfflineAccess,
                        OpenIddictConstants.Scopes.Roles
                    );
                    string certificate = "Certificates/e-cert.pfx";
                    X509Certificate2 cert = X509CertificateLoader.LoadPkcs12FromFile(
                        certificate,
                        "SecureString",
                        X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.EphemeralKeySet
                    );
                    options.AddEncryptionCertificate(cert);
                    certificate = "Certificates/s-cert.pfx";
                    cert = X509CertificateLoader.LoadPkcs12FromFile(
                        certificate,
                        "SecureString",
                        X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.EphemeralKeySet
                    );
                    options.AddSigningCertificate(cert);
                })
                .AddValidation(options =>
                {
                    options.UseLocalServer();
                    options.UseAspNetCore();
                    string certificate = "Certificates/e-cert.pfx";
                    X509Certificate2 cert = X509CertificateLoader.LoadPkcs12FromFile(
                        certificate,
                        "SecureString",
                        X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.EphemeralKeySet
                    );
                    options.AddEncryptionCertificate(cert);
                }); //Only compatible with the default token format. For JWT tokens, use the Microsoft JWT bearer handler.

            services.AddAuthentication(options =>
            {
                // options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                // options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                // options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                // options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme =
                    OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme =
                    OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                options.DefaultForbidScheme =
                    OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            });
            services.AddCors(options =>
            {
                var origins = Configuration.GetSection("AllowedCors")?.Value;
                options.AddPolicy(
                    "ProdCorsPolicy",
                    builder =>
                    {
                        if (origins != null)
                        {
                            builder
                                .SetIsOriginAllowed(origin =>
                                    IsOriginAllowed(origin, origins.Split(","))
                                )
                                .AllowAnyMethod()
                                .AllowCredentials()
                                .AllowAnyHeader();
                        }
                    }
                );
            });

            // Add MediatR
            services.AddHttpClient();
            services.AddHttpContextAccessor();
            services.AddTransient(
                typeof(IPipelineBehavior<,>),
                typeof(RequestPreProcessorBehavior<,>)
            );
            services.AddTransient(
                typeof(IPipelineBehavior<,>),
                typeof(RequestPerformanceBehaviour<,>)
            );
            services.AddTransient(
                typeof(IPipelineBehavior<,>),
                typeof(RequestValidationBehavior<,>)
            );
            services.AddTransient<ISessionService, SessionService>();
            services.AddSingleton<IBackgroundTaskQueueService, BackgroundTaskQueueService>();
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(
                    typeof(GetUsersRequestModel).GetTypeInfo().Assembly
                );
            });
            services
                .AddControllers(options =>
                {
                    options.Filters.Add(typeof(CustomAuthorizeFilter));
                    options.Filters.Add(typeof(CustomExceptionFilterAttribute));
                    options.Filters.Add(typeof(ValidationActionFilter));
                    options.Filters.Add(typeof(ObjectConversionFilter));
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition =
                        JsonIgnoreCondition.WhenWritingNull;
                    options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
                    options.JsonSerializerOptions.Converters.Add(new TimeSpanConverter());
                    options.JsonSerializerOptions.Converters.Add(new DateOnlyConverter());
                });
            //services.AddValidatorsFromAssemblyContaining<GetUsersRequestModel>();
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });
            // services.AddEndpointsApiExplorer();
            var swagger = Configuration.GetSection("Swagger")?.Value == "true";
            if (swagger)
            {
                services.AddOpenApiDocument(c =>
                {
                    var scheme = new OpenApiSecurityScheme()
                    {
                        Description =
                            "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                        Name = "Authorization",
                        In = OpenApiSecurityApiKeyLocation.Header,
                        Type = OpenApiSecuritySchemeType.ApiKey,
                    };
                    c.AddSecurity("Bearer", scheme);
                    //c.AddSecurity(new OpenApiSecurityRequirement()
                    // {
                    //     {scheme, new string[] { }}
                    // });
                    //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    //c.UseXmlDocumentation = true;
                    //c.IncludeXmlComments(xmlPath);
                });
                //services.AddSwaggerGen(c =>
                //{
                //    c.SwaggerDoc("v1", new OpenApiInfo() { Title = "TheFork API", Description = Environment.GetEnvironmentVariable("BuildNumber"), Version = "v1" });
                //    var scheme = new OpenApiSecurityScheme()
                //    {
                //        Description =
                //            "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                //        Name = "Authorization",
                //        In = ParameterLocation.Header,
                //        Type = SecuritySchemeType.ApiKey
                //    };
                //    c.AddSecurityDefinition("Bearer", scheme);
                //    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                //{
                //     {scheme, new string[] { }}
                //});
                //    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                //    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                //    c.IncludeXmlComments(xmlPath);
                //});
            }

            services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
            services.AddAuthorization(options =>
            {
                options.AddPermissionPolicies();
            });
            services.Configure<SmtpOption>(Configuration.GetSection("Smtp"));
            services.Configure<GoogleOption>(Configuration.GetSection("Google"));
            services.Configure<AppleOption>(Configuration.GetSection("Apple"));
            services.Configure<StripeOption>(Configuration.GetSection("Stripe"));
            services.Configure<S3Option>(Configuration.GetSection("S3"));
            services.AddSingleton<IImageService, ImageService>();
            services.AddSingleton<ISmtpService, SmtpService>();
            services.AddSingleton<ICsvService, CsvService>();
            services.AddSingleton<IWordService, WordService>();
            services.AddSingleton<IExcelService, ExcelService>();
            services.AddSingleton<IAlertService, AlertService>();
            services.AddSingleton<IPdfService, PdfService>();
            services.AddSignalR();
            services.AddLogging(conf =>
            {
                conf.AddConfiguration(Configuration);
            });
            services.AddHostedService<QueuedHostedService>();
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseCors("ProdCorsPolicy");
            }
            else
            {
                app.UseCors("ProdCorsPolicy");
            }
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

            var swagger = Configuration.GetSection("Swagger")?.Value == "true";
            if (swagger)
            {
                app.UseOpenApi();

                app.UseSwaggerUi();
            }
            app.UseForwardedHeaders(
                new ForwardedHeadersOptions
                {
                    ForwardedHeaders =
                        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
                }
            );
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<NotificationHub>("/notification");
                endpoints.MapHub<ChatHub>("/chat");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}"
                );
            });
        }

        private static bool IsOriginAllowed(
            string origin,
            IReadOnlyCollection<string> allowedOrigins
        )
        {
            try
            {
                return true;
                var uri = new Uri(origin);
                var isAllowed = false;
                if (
                    allowedOrigins.Any(p =>
                        p.Contains("https://*")
                        && uri.Host.EndsWith(
                            p.Replace("https://*", ""),
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                )
                {
                    isAllowed = true;
                }
                else if (
                    allowedOrigins.Any(p => uri.Host.Equals(p, StringComparison.OrdinalIgnoreCase))
                )
                {
                    isAllowed = true;
                }
                return isAllowed;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
