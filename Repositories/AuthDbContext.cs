using Fido2NetLib.Development;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models.Auth;
using PinkUmbrella.Models.Auth.Permissions;

namespace PinkUmbrella.Repositories
{
    public class AuthDbContext: DbContext
    {
        public DbSet<PublicKey> PublicKeys { get; set; }
        
        public DbSet<PrivateKey> PrivateKeys { get; set; }

        public DbSet<IPAddressModel> IPs { get; set; }

        public DbSet<AuthSitePermissionModel> SitePermissions { get; set; }

        public DbSet<IPBlockModel> IPBlocks { get; set; }

        public DbSet<UserAuthKeyModel> UserAuthKeys { get; set; }

        public DbSet<FIDOCredential> FIDOCredentials { get; set; }

        public DbSet<UserLoginMethodModel> UserLoginMethods { get; set; }

        public DbSet<KeyChallengeModel> KeyChallenges { get; set; }
        
        public AuthDbContext(DbContextOptions<AuthDbContext> options): base(options) {

        }
    }
}