using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using seattle.Models;
using seattle.Repositories;
using System.Collections.Generic;

namespace seattle.Services.Sql
{
    public class FeedService
    {
        private readonly SimpleDbContext _dbContext;
        private readonly IPostService _posts;

        public FeedService(SimpleDbContext dbContext, IPostService posts) {
            _dbContext = dbContext;
            _posts = posts;
        }
    }
}