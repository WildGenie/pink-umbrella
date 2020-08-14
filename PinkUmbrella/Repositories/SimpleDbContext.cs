using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using PinkUmbrella.Models;
using System.Threading.Tasks;
using Tides.Models.Public;

namespace PinkUmbrella.Repositories
{
    public class SimpleDbContext: IdentityDbContext<UserProfileModel, UserGroupModel, int>
    {
        public DbSet<ObjectContentModel> Articles { get; set; }
        public DbSet<ObjectContentModel> Events { get; set; }
        public DbSet<ObjectContentModel> Places { get; set; }
        public DbSet<ObjectContentModel> Notes { get; set; }
        public DbSet<ObjectContentModel> Shops { get; set; }

        // public DbSet<ObjectContentModel> Inventories { get; set; }
        public DbSet<SimpleResourceModel> Resources { get; set; }
        
        public DbSet<TagModel> AllTags { get; set; }

        public DbSet<FollowingTagModel> FollowingPostTags { get; set; }
        
        public DbSet<ObjectShadowBanModel> ObjectShadowBans { get; set; }

        public DbSet<ArchivedMediaModel> ArchivedMedia { get; set; }

        public DbSet<GroupAccessCodeModel> GroupAccessCodes { get; set; }

        public SimpleDbContext(DbContextOptions<SimpleDbContext> options): base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ObjectContentModel>()
                .HasIndex(e => e.Handle).IsUnique();
        }

        public async Task UpdateShadowBanStatus(PublicId id, string type, bool status)
        {
            if (id.PeerId == 0)
            {
                var p = await ObjectShadowBans.FirstOrDefaultAsync(sb => sb.ObjectId == id.Id && sb.PeerId == id.PeerId && sb.Type == type);
                if (status)
                {
                    if (p == null)
                    {
                        ObjectShadowBans.Add(new ObjectShadowBanModel { ObjectId = id.Id.Value, PeerId = id.PeerId.Value, Type = type });
                    }
                }
                else if (p != null)
                {
                    ObjectShadowBans.Remove(p);
                }
                await SaveChangesAsync();
            }
        }
    }
}