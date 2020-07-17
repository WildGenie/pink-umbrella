using System;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Hangfire.Storage.SQLite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;
using PinkUmbrella.Services;
using PinkUmbrella.Services.Elastic.Search;
using PinkUmbrella.Services.Jobs;
using PinkUmbrella.Services.Local;
using PinkUmbrella.Services.NoSql;
using PinkUmbrella.Services.Peer;
using PinkUmbrella.Services.Public;
using PinkUmbrella.Services.Search;
using PinkUmbrella.Services.Sql;
using PinkUmbrella.Services.Sql.React;
using PinkUmbrella.Services.Sql.Search;
using PinkUmbrella.Util;
using Poncho.Models.Auth;
using Poncho.Models.Auth.Types;
using Poncho.Models.Peer;
using StackExchange.Profiling.Storage;

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

            services.AddMiniProfiler(options =>
            {
                // All of this is optional. You can simply call .AddMiniProfiler() for all defaults

                // (Optional) Path to use for profiler URLs, default is /mini-profiler-resources
                options.RouteBasePath = "/profiler";

                // (Optional) Control storage
                // (default is 30 minutes in MemoryCacheStorage)
                //(options.Storage as MemoryCacheStorage).CacheDuration = TimeSpan.FromMinutes(60);

                // (Optional) Control which SQL formatter to use, InlineFormatter is the default
                options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();

                // (Optional) To control authorization, you can use the Func<HttpRequest, bool> options:
                // (default is everyone can access profilers)
                //options.ResultsAuthorize = request => MyGetUserFunction(request).CanSeeMiniProfiler;
                //options.ResultsListAuthorize = request => MyGetUserFunction(request).CanSeeMiniProfiler;
                // Or, there are async versions available:
                //options.ResultsAuthorizeAsync = async request => (await MyGetUserFunctionAsync(request)).CanSeeMiniProfiler;
                //options.ResultsAuthorizeListAsync = async request => (await MyGetUserFunctionAsync(request)).CanSeeMiniProfilerLists;

                // (Optional)  To control which requests are profiled, use the Func<HttpRequest, bool> option:
                // (default is everything should be profiled)
                //options.ShouldProfile = request => MyShouldThisBeProfiledFunction(request);

                // (Optional) Profiles are stored under a user ID, function to get it:
                // (default is null, since above methods don't use it by default)
                //options.UserIdProvider =  request => MyGetUserIdFunction(request);

                // (Optional) Swap out the entire profiler provider, if you want
                // (default handles async and works fine for almost all applications)
                //options.ProfilerProvider = new MyProfilerProvider();

                // (Optional) You can disable "Connection Open()", "Connection Close()" (and async variant) tracking.
                // (defaults to true, and connection opening/closing is tracked)
                //options.TrackConnectionOpenClose = true;

                // (Optional) Use something other than the "light" color scheme.
                // (defaults to "light")
                options.ColorScheme = StackExchange.Profiling.ColorScheme.Auto;

                // The below are newer options, available in .NET Core 3.0 and above:

                // (Optional) You can disable MVC filter profiling
                // (defaults to true, and filters are profiled)
                options.EnableMvcFilterProfiling = true;
                // ...or only save filters that take over a certain millisecond duration (including their children)
                // (defaults to null, and all filters are profiled)
                // options.MvcFilterMinimumSaveMs = 1.0m;

                // (Optional) You can disable MVC view profiling
                // (defaults to true, and views are profiled)
                options.EnableMvcViewProfiling = true;
                // ...or only save views that take over a certain millisecond duration (including their children)
                // (defaults to null, and all views are profiled)
                // options.MvcViewMinimumSaveMs = 1.0m;
            
                // (Optional) listen to any errors that occur within MiniProfiler itself
                // options.OnInternalError = e => MyExceptionLogger(e);

                // (Optional - not recommended) You can enable a heavy debug mode with stacks and tooltips when using memory storage
                // It has a lot of overhead vs. normal profiling and should only be used with that in mind
                // (defaults to false, debug/heavy mode is off)
                //options.EnableDebugMode = true;
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
            services.AddSingleton<ExternalDbOptions>((_) => new ExternalDbOptions() {
                ExtractDbHandle = ExternalDbOptions.ExtractDomain,
                OpenDbContext = (handle) => {
                    System.IO.Directory.CreateDirectory($"Peers/{handle}");
                    var options = new DbContextOptionsBuilder<SimpleDbContext>();
                    options.UseSqlite($"Peers/{handle}/in.db; Read Only=True;");
                    return Task.FromResult<DbContext>(new SimpleDbContext(options.Options));
                }
            });

            services.AddSingleton<IAuthTypeHandler, RSAAuthHandlerMsft>();
            services.AddSingleton<IAuthTypeHandler, RSAAuthHandlerBouncy>();
            services.AddSingleton<IAuthTypeHandler, OpenPGPAuthHandler>();
            services.AddSingleton<IElasticService, ElasticService>();

            services.AddSingleton<IPeerConnectionType, RESTPeerClientType>();

            services.AddSingleton<ISettingsService, SettingsService>();

            services.AddScoped<IExternalDbContext, ExternalDbContext>();
            services.AddScoped<IPeerConnectionTypeResolver, PeerConnectionTypeResolver>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IPeerService, PeerService>();

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
            services.AddScoped<IFeedService, FeedService>();
            services.AddScoped<IDebugService, DebugService>();
            
            services.AddScoped<ISearchableService, SqlSearchPostsService>();
            services.AddScoped<ISearchableService, SqlSearchProfilesService>();
            services.AddScoped<ISearchableService, SqlSearchShopsService>();
            services.AddScoped<ISearchableService, SqlSearchArchivedPhotosService>();
            services.AddScoped<ISearchableService, SqlSearchArchivedVideosService>();
            services.AddScoped<ISearchableService, SqlSearchInventoryService>();

            services.AddScoped<ISearchableService, ElasticSearchPostsService>();
            services.AddScoped<ISearchableService, ElasticSearchProfilesService>();
            services.AddScoped<ISearchableService, ElasticSearchShopsService>();
            services.AddScoped<ISearchableService, ElasticSearchArchivedPhotosService>();
            services.AddScoped<ISearchableService, ElasticSearchArchivedVideosService>();
            services.AddScoped<ISearchableService, ElasticSearchInventoryService>();

            services.AddScoped<ISearchService, SearchService>();

            services.AddScoped<IReactableService, PostReactionService>();
            services.AddScoped<IReactableService, ShopReactionService>();
            services.AddScoped<IReactableService, ProfileReactionService>();
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
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<IsAdminOrDevOrDebuggingOrElse404Middleware>();
            app.UseMiddleware<LogErrorRedirectProdMiddleware>();
            app.UseMiddleware<ExternalDbMiddleware>();

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

            RecurringJob.AddOrUpdate(() => ElasticJobs.SyncProfiles(null), Cron.Hourly);
            RecurringJob.AddOrUpdate(() => ElasticJobs.SyncPosts(null), Cron.Hourly);
            RecurringJob.AddOrUpdate(() => ElasticJobs.SyncMedia(null), Cron.Hourly);
            RecurringJob.AddOrUpdate(() => ElasticJobs.SyncShops(null), Cron.Hourly);
            RecurringJob.AddOrUpdate(() => ElasticJobs.SyncReactions(null), Cron.Hourly);
            RecurringJob.AddOrUpdate(() => ElasticJobs.SyncMentions(null), Cron.Hourly);
            RecurringJob.AddOrUpdate(() => ElasticJobs.SyncPeers(null), Cron.Hourly);
            RecurringJob.AddOrUpdate(() => ElasticJobs.SyncInventories(null), Cron.Hourly);
        }
    }
}
