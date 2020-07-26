using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;
using Tides.Core;
using Tides.Models;
using Tides.Models.Public;
using Tides.Services;

namespace PinkUmbrella.Services.Sql
{
    public class FeedService: IFeedService
    {
        private readonly SimpleDbContext _dbContext;
        private readonly IPostService _posts;
        private readonly IActivityStreamRepository _activityStreams;

        public FeedService(SimpleDbContext dbContext, IPostService posts, IActivityStreamRepository activityStreams)
        {
            _dbContext = dbContext;
            _posts = posts;
            _activityStreams = activityStreams;
        }


        // TODO: Fix bugs regarding CanView and user
        public async Task<CollectionObject> GetFeedForUser(PublicId userId, int? viewerId, bool includeReplies, PaginationModel pagination)
        {
            // .ProfileReactions.Where(r => r.UserId == userId.Id && r.ToPeerId == userId.PeerId && r.Type == ReactionType.Follow).Select(r => r.ToId).ToListAsync();
            //var followerIds = await _activityStreams.GetFollowers(new ActivityStreamFilter { id = userId.Id, peerId = userId.PeerId });
            var followerIds = new int[] {};
            //var posts = await _dbContext.Posts.Where(p => p.IsReply == includeReplies && followerIds.Contains(p.UserId)).OrderByDescending(p => p.WhenCreated).ToListAsync();
            var posts = await _activityStreams.GetInbox(new ActivityStreamFilter
            {
                includeReplies = includeReplies, userId = userId.Id, peerId = userId.PeerId
            });
            //var keepers = new List<PostModel>();

            // return new FeedModel() {
            //     Items = keepers.Skip(pagination.start).Take(pagination.count).ToList(),
            //     Pagination = pagination,
            //     RepliesIncluded = includeReplies,
            //     UserId = userId,
            //     ViewerId = viewerId,
            //     Total = keepers.Count()
            // };
            return posts;
        }
    }
}