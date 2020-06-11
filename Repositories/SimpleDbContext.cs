using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using seattle.Models;

namespace seattle.Repositories
{
    public class SimpleDbContext: IdentityDbContext<UserProfileModel, UserGroupModel, int>
    {
        public DbSet<SimpleResourceModel> Resources { get; set; }
        public DbSet<SimpleInventoryModel> Inventories { get; set; }
        public DbSet<PostModel> Posts { get; set; }
        public DbSet<PostTagModel> PostTags { get; set; }
        public DbSet<ShopModel> Shops { get; set; }


        public SimpleDbContext(DbContextOptions<SimpleDbContext> options): base(options) {

        }
    }
}