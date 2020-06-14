using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using PinkUmbrella.Models;

namespace PinkUmbrella.Repositories
{
    public class SimpleDbContext: IdentityDbContext<UserProfileModel, UserGroupModel, int>
    {
        public DbSet<SimpleResourceModel> Resources { get; set; }
        public DbSet<SimpleInventoryModel> Inventories { get; set; }
        public DbSet<PostModel> Posts { get; set; }
        public DbSet<PostTagModel> PostTags { get; set; }
        public DbSet<ShopModel> Shops { get; set; }

        public DbSet<ReactionModel> PostReactions { get; set; }
        public DbSet<ReactionModel> ShopReactions { get; set; }
        public DbSet<ReactionModel> ProfileReactions { get; set; }
        public DbSet<ReactionModel> ArchivedMediaReactions { get; set; }
        

        public DbSet<ArchivedMediaModel> ArchivedMedia { get; set; }
        public DbSet<MentionModel> Mentions { get; set; }


        public SimpleDbContext(DbContextOptions<SimpleDbContext> options): base(options) {

        }
    }
}