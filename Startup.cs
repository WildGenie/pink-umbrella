using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using seattle.Models;
using seattle.Repositories;
using seattle.Services;
using seattle.Services.NoSql;
using seattle.Services.Sql;
using seattle.Services.Sql.React;
using seattle.Services.Sql.Search;

namespace seattle
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
            services.AddDbContext<SimpleDbContext>(options => options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));
            
            services.AddIdentity<UserProfileModel, UserGroupModel>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<SimpleDbContext>()
                .AddDefaultTokenProviders();

            services.AddSingleton<MIPMapRepository>();
            services.AddSingleton<StringRepository>();

            services.AddScoped<IMIPMapService, MIPMapService>();
            services.AddScoped<IArchiveService, ArchiveService>();
            services.AddScoped<IGeoLocationService, GeoLocationService>();
            services.AddScoped<IUserProfileService, UserProfileService>();
            services.AddScoped<ISimpleResourceService, SimpleResourceService>();
            services.AddScoped<ISimpleInventoryService, SimpleInventoryService>();
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<IFeedService, FeedService>();
            services.AddScoped<IShopService, ShopService>();
            
            services.AddScoped<ISearchableService, SearchPostsService>();
            services.AddScoped<ISearchableService, SearchProfilesService>();
            services.AddScoped<ISearchableService, SearchShopsService>();
            services.AddScoped<ISearchableService, SearchArchiveService>();
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
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                // options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(1);

                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            services.AddControllersWithViews(options => {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            }).AddRazorRuntimeCompilation();//.AddNewtonsoftJson().SetCompatibilityVersion(CompatibilityVersion.Version_3_1);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
