using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using PinkUmbrella.Models.AhPushIt;

namespace PinkUmbrella.Repositories
{
    public class AhPushItDbContext: DbContext
    {
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationRecipient> Recipients { get; set; }
        public DbSet<NotificationMethodSetting> MethodSettings { get; set; }

        public AhPushItDbContext(DbContextOptions<AhPushItDbContext> options): base(options) {

        }
    }
}