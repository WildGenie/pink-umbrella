using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using PinkUmbrella.Models;
using System;
using System.Threading.Tasks;

namespace PinkUmbrella.Repositories
{
    public class SimpleDbContext: IdentityDbContext<UserProfileModel, UserGroupModel, int>
    {
        public DbSet<SimpleResourceModel> Resources { get; set; }
        public DbSet<SimpleInventoryModel> Inventories { get; set; }
        public DbSet<PostModel> Posts { get; set; }
        public DbSet<ShopModel> Shops { get; set; }

        public DbSet<ReactionModel> PostReactions { get; set; }
        public DbSet<ReactionModel> ShopReactions { get; set; }
        public DbSet<ReactionModel> ProfileReactions { get; set; }
        public DbSet<ReactionModel> ArchivedMediaReactions { get; set; }

        public DbSet<TagModel> AllTags { get; set; }
        public DbSet<TaggedModel> PostTags { get; set; }
        public DbSet<TaggedModel> ShopTags { get; set; }
        public DbSet<TaggedModel> ProfileTags { get; set; }
        public DbSet<TaggedModel> ArchivedMediaTags { get; set; }

        public DbSet<FollowingTagModel> FollowingPostTags { get; set; }
        

        public DbSet<ArchivedMediaModel> ArchivedMedia { get; set; }
        public DbSet<MentionModel> Mentions { get; set; }

        public DbSet<GroupAccessCodeModel> GroupAccessCodes { get; set; }

        public SimpleDbContext(DbContextOptions<SimpleDbContext> options): base(options) {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ShopModel>()
                .HasIndex(e => e.Handle).IsUnique();

            modelBuilder.Entity<UserProfileModel>()
                .HasIndex(e => e.Handle).IsUnique();
        }
    }
}