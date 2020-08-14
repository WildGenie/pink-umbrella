using System;
using System.IO;
using System.Threading.Tasks;
using Estuary.Services;
using Hangfire;
using Hangfire.Storage.SQLite;
using Markdig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using PinkUmbrella.Models;
using PinkUmbrella.Models.Auth;
using PinkUmbrella.Repositories;
using PinkUmbrella.Services;
using PinkUmbrella.Services.Elastic.Search;
using PinkUmbrella.Services.Jobs;
using PinkUmbrella.Services.Local;
using PinkUmbrella.Services.NoSql;
using PinkUmbrella.Services.Public;
using PinkUmbrella.Services.Search;
using PinkUmbrella.Services.Sql;
using PinkUmbrella.Util;
using StackExchange.Redis;
using Tides.Models.Auth;
using Tides.Models.Auth.Types;

namespace PinkUmbrella
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddFeatureManagement();
            services.AddDistributedMemoryCache();
            
            services.AddDbContext<SimpleDbContext>(options => options.UseSqlite(Configuration.GetConnectionString("MainConnection")));
            services.AddDbContext<LogDbContext>(options => options.UseSqlite(Configuration.GetConnectionString("LogConnection")));
            services.AddDbContext<AhPushItDbContext>(options => options.UseSqlite(Configuration.GetConnectionString("NotificationConnection")));
            services.AddDbContext<AuthDbContext>(options => options.UseSqlite(Configuration.GetConnectionString("AuthConnection")));

            // https://stackexchange.github.io/StackExchange.Redis/Configuration
            services.AddSingleton<ConnectionMultiplexer>(ConnectionMultiplexer.Connect("127.0.0.1:6379")); // ,password=password
            services.AddSingleton<RedisRepository>();

            services.AddMiniProfiler(options =>
            {
                // (Optional) Path to use for profiler URLs, default is /mini-profiler-resources
                options.RouteBasePath = "/profiler";

                // (Optional) Control which SQL formatter to use, InlineFormatter is the default
                options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();

                // (Optional) To control authorization, you can use the Func<HttpRequest, bool> options:
                // (default is everyone can access profilers)
                //options.ResultsAuthorize = request => MyGetUserFunction(request).CanSeeMiniProfiler;
                //options.ResultsListAuthorize = request => MyGetUserFunction(request).CanSeeMiniProfiler;
                // Or, there are async versions available:
                //options.ResultsAuthorizeAsync = async request => (await MyGetUserFunctionAsync(request)).CanSeeMiniProfiler;
                //options.ResultsAuthorizeListAsync = async request => (await MyGetUserFunctionAsync(request)).CanSeeMiniProfilerLists;

                // (Optional) Use something other than the "light" color scheme.
                // (defaults to "light")
                options.ColorScheme = StackExchange.Profiling.ColorScheme.Auto;

                // The below are newer options, available in .NET Core 3.0 and above:

                // (Optional) You can disable MVC filter profiling
                // (defaults to true, and filters are profiled)
                options.EnableMvcFilterProfiling = true;
            
                // (Optional) listen to any errors that occur within MiniProfiler itself
                // options.OnInternalError = e => MyExceptionLogger(e);
            });
            
            services.AddIdentity<UserProfileModel, UserGroupModel>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<SimpleDbContext>()
                .AddEntityFrameworkStores<LogDbContext>()
                .AddEntityFrameworkStores<AhPushItDbContext>()
                .AddEntityFrameworkStores<AuthDbContext>()
                .AddDefaultTokenProviders();

            services.AddSingleton<IConfiguration>(_ => Configuration);

            services.AddHangfire(configuration => configuration
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSQLiteStorage()
                //.UseHeartbeatPage(checkInterval: TimeSpan.FromSeconds(30))
                );// .UseJobsLogger()

            services.AddSingleton<StringRepository>();
            services.AddSingleton<CategorizedLinksRepository>();

            services.AddSingleton<IAuthTypeHandler, RSAAuthHandlerMsft>();
            services.AddSingleton<IAuthTypeHandler, RSAAuthHandlerBouncy>();
            services.AddSingleton<IAuthTypeHandler, OpenPGPAuthHandler>();
            services.AddSingleton<IElasticService, ElasticService>();
            services.AddSingleton<SiteKeyManager>();

            services.AddSingleton<ISettingsService, SettingsService>();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IPeerService, PeerService>();
            services.AddScoped<IObjectReferenceService, SqlSqlObjectReferenceService>();

            services.AddSingleton<MarkdownPipeline>(new MarkdownPipelineBuilder().UseAdvancedExtensions().Build());

            services.UseActivityStreamBoxProviders();
            services.UseActivityStreamReadPipes();
            services.UseActivityStreamWritePipes();

            services.AddScoped<IHazActivityStreamPipe, ActivityStreamPipe>();
            services.AddScoped<IActivityStreamRepository, LocalActivityStreamRepository>();

            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<ITagService, TagService>();
            services.AddScoped<IArchiveService, ArchiveService>();
            services.AddScoped<IGeoLocationService, GeoLocationService>();
            services.AddScoped<IInvitationService, InvitationService>();
            services.AddScoped<IUserProfileService, UserProfileService>();
            services.AddScoped<IPublicProfileService, PublicUserService>();
            services.AddScoped<ISimpleResourceService, SimpleResourceService>();
            services.AddScoped<ISimpleInventoryService, SimpleInventoryService>();
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<IShopService, ShopService>();
            services.AddScoped<IDebugService, DebugService>();

            services.AddScoped<ISearchableService, ElasticSearchPostsService>();
            services.AddScoped<ISearchableService, ElasticSearchPeopleService>();
            services.AddScoped<ISearchableService, ElasticSearchShopsService>();
            services.AddScoped<ISearchableService, ElasticSearchArchivedPhotosService>();
            services.AddScoped<ISearchableService, ElasticSearchArchivedVideosService>();
            services.AddScoped<ISearchableService, ElasticSearchInventoryService>();

            services.AddScoped<ISearchService, SearchService>();

            services.AddScoped<IReactionService, ReactionService>();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters = StringRepository.AllowedUserNameChars;
                options.User.RequireUniqueEmail = true;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                // options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(1);

                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Error/403";
                options.SlidingExpiration = true;
            });

            services.AddFido2(options =>
            {
                options.ServerDomain = Configuration["fido2:serverDomain"];
                options.ServerName = "FIDO2 Test";
                options.Origin = Configuration["fido2:origin"];
                options.TimestampDriftTolerance = Configuration.GetValue<int>("fido2:timestampDriftTolerance");
                options.MDSAccessKey = Configuration["fido2:MDSAccessKey"];
                options.MDSCacheDirPath = Configuration["fido2:MDSCacheDirPath"];
            })
            .AddCachedMetadataService(config =>
            {
                //They'll be used in a "first match wins" way in the order registered
                config.AddStaticMetadataRepository();
                if (!string.IsNullOrWhiteSpace(Configuration["fido2:MDSAccessKey"]))
                {
                    config.AddFidoMetadataRepository(Configuration["fido2:MDSAccessKey"]);
                }
            });

            services.AddScoped<IsDevOrDebuggingOrElse404FilterAttribute>();
            services.AddScoped<IsAdminOrDebuggingOrElse404FilterAttribute>();
            services.AddScoped<ApiCallFilterAttribute>();

            
            services.AddApiVersioning();
            services.AddVersionedApiExplorer();

            services.AddControllersWithViews(options => {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            }).AddRazorRuntimeCompilation();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();

            SetupFiles(app);
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<IsAdminOrDevOrDebuggingOrElse404Middleware>();
            app.UseMiddleware<LogErrorRedirectProdMiddleware>();

            if (env.IsDevelopment())
            {
                app.UseMiniProfiler();
            }

            // UseMiddlewareForFeature

            app.UseHangfireServer();
            app.UseHangfireDashboard("/Admin/Hangfire");

            app.UseApiVersioning();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            ElasticJobs.Services = app.ApplicationServices;

            // RecurringJob.AddOrUpdate(() => ElasticJobs.SyncProfiles(null), Cron.Hourly);
            // RecurringJob.AddOrUpdate(() => ElasticJobs.SyncPosts(null), Cron.Hourly);
            // RecurringJob.AddOrUpdate(() => ElasticJobs.SyncMedia(null), Cron.Hourly);
            // RecurringJob.AddOrUpdate(() => ElasticJobs.SyncShops(null), Cron.Hourly);
            // RecurringJob.AddOrUpdate(() => ElasticJobs.SyncReactions(null), Cron.Hourly);
            // RecurringJob.AddOrUpdate(() => ElasticJobs.SyncMentions(null), Cron.Hourly);
            // RecurringJob.AddOrUpdate(() => ElasticJobs.SyncPeers(null), Cron.Hourly);
            // RecurringJob.AddOrUpdate(() => ElasticJobs.SyncInventories(null), Cron.Hourly);
            RecurringJob.AddOrUpdate(() => ElasticJobs.SyncObjects(null), Cron.Hourly);
        }

        private void SetupFiles(IApplicationBuilder app)
        {
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".ts"] = "application/javascript;application/typescript";

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/ts")),
                RequestPath = new PathString("/ts"),
                ContentTypeProvider = provider
            });

            // Tribute
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot-lib/tribute/dist")),
                RequestPath = new PathString("/lib/tribute"),
            });
            // app.UseDirectoryBrowser(new DirectoryBrowserOptions()
            // {
            //     FileProvider = new PhysicalFileProvider(
            //         Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\images")),
            //     RequestPath = new PathString("/MyImages")
            // });
        }
    }
}
