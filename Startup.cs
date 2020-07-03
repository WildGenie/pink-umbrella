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
using PinkUmbrella.Models.Auth;
using PinkUmbrella.Models.Auth.Types;
using PinkUmbrella.Repositories;
using PinkUmbrella.Services;
using PinkUmbrella.Services.Jobs;
using PinkUmbrella.Services.NoSql;
using PinkUmbrella.Services.Peer;
using PinkUmbrella.Services.Sql;
using PinkUmbrella.Services.Sql.React;
using PinkUmbrella.Services.Sql.Search;
using PinkUmbrella.Util;

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

            services.AddSingleton<IAuthTypeHandler, RSAAuthHandler>();
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
            services.AddScoped<IUserProfileService, UserProfileService>();
            services.AddScoped<ISimpleResourceService, SimpleResourceService>();
            services.AddScoped<ISimpleInventoryService, SimpleInventoryService>();
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<IShopService, ShopService>();
            services.AddScoped<IFeedService, FeedService>();
            services.AddScoped<IDebugService, DebugService>();
            
            services.AddScoped<ISearchableService, SearchPostsService>();
            services.AddScoped<ISearchableService, SearchProfilesService>();
            services.AddScoped<ISearchableService, SearchShopsService>();
            services.AddScoped<ISearchableService, SearchArchivedPhotosService>();
            services.AddScoped<ISearchableService, SearchArchivedVideosService>();
            services.AddScoped<ISearchableService, SearchInventoryService>();
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

            app.UseHangfireServer();
            app.UseHangfireDashboard("/Admin/Hangfire");

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
        }
    }
}
