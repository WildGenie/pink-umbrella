using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models.Auth;
using PinkUmbrella.Models.Auth.Permissions;

namespace PinkUmbrella.Repositories
{
    public class AuthDbContext: DbContext
    {
        public DbSet<AuthKey> AuthKeys { get; set; }

        public DbSet<IPAddressModel> IPs { get; set; }

        public DbSet<AuthSitePermissionModel> SitePermissions { get; set; }
        
        public AuthDbContext(DbContextOptions<AuthDbContext> options): base(options) {

        }
    }
}