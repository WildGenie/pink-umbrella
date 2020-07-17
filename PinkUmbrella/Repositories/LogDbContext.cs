using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using PinkUmbrella.Models;

namespace PinkUmbrella.Repositories
{
    public class LogDbContext: DbContext
    {
        public DbSet<LoggedExceptionModel> Exceptions { get; set; }


        public LogDbContext(DbContextOptions<LogDbContext> options): base(options) {

        }
    }
}