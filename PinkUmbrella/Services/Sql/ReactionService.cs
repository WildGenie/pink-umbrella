using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;
using PinkUmbrella.Services.Elastic;
using Tides.Actors;
using Tides.Core;
using Tides.Models;
using Tides.Models.Public;
using Tides.Services;
using Tides.Util;

namespace PinkUmbrella.Services.Sql
{
    public class ReactionService : BaseElasticService, IReactionService
    {
        //private readonly SimpleDbContext _dbContext;
        private readonly IActivityStreamRepository _activityStream;

        public ReactionService(IActivityStreamRepository activityStream) // SimpleDbContext dbContext
        {
            //_dbContext = dbContext;
            _activityStream = activityStream;
        }

        public Task DeleteSummary(ReactionsSummaryModel summary, ReactionSubject subject)
        {
            throw new NotImplementedException();
        }

        public Task<CollectionObject> Get(ReactionSubject subject, PublicId id, int? viewerId)
        {
            //var reactions = GetReactionsList(subject);
            //return reactions.Where(r => r.ToId == id.Id && r.ToPeerId == id.PeerId && r.UserId == viewerId.Value).ToListAsync();
            return null;
        }

        public async Task<int> GetCount(PublicId toId, ReactionType type, ReactionSubject subject)
        {
            var res = await _activityStream.GetAll(new ActivityStreamFilter
            {
                publicId = toId, types = new string[] { type.ToString() }, targetTypes = new string[] { subject.ToString() }
            });
            if (res is CollectionObject collection)
            {
                return collection.totalItems;
            }
            else
            {
                return -1;
            }
        }

        public Task<List<ReactionsSummaryModel>> GetMostBlocked(ReactionSubject subject, int? viewerId)
        {
            throw new NotImplementedException();
        }

        public Task<List<ReactionsSummaryModel>> GetMostDisliked(ReactionSubject subject, int? viewerId)
        {
            throw new NotImplementedException();
        }

        public Task<List<ReactionsSummaryModel>> GetMostFollowed(ReactionSubject subject, int? viewerId)
        {
            throw new NotImplementedException();
        }

        public Task<List<ReactionsSummaryModel>> GetMostLiked(ReactionSubject subject, int? viewerId)
        {
            throw new NotImplementedException();
        }

        public Task<List<ReactionsSummaryModel>> GetMostReported(ReactionSubject subject, int? viewerId)
        {
            throw new NotImplementedException();
        }

        public Task<ReactionsSummaryModel> GetSummary(int toId, ReactionSubject subject, int? viewerId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> HasBlockedViewer(ReactionSubject subject, PublicId id, int? viewerId)
        {
            if (0 == id.PeerId)
            {
                //var reactions = GetReactionsList(subject);
                return false; //(await reactions.FirstOrDefaultAsync(r => r.ToId == viewerId.Value && r.ToPeerId == 0 && r.UserId == id.Id && (r.Type == ReactionType.Block || r.Type == ReactionType.Report))) != null;
            }
            else
            {
                var client = GetClient();
                var response = await client.GetAsync<BaseObject>(id.ToString());
                if (response.IsValid)
                {
                    var r = response.Source;
                    return false;//r.ToId == viewerId.Value && r.ToPeerId == 0 && r.UserId == id.Id && (r.Type == ReactionType.Block || r.Type == ReactionType.Report);
                }
                else
                {
                    return false;
                }
            }
        }

        public Task<ReactionsSummaryModel> PutSummary(ReactionsSummaryModel summary, ReactionSubject subject)
        {
            throw new NotImplementedException();
        }

        public async Task<int> React(int userId, PublicId toId, ReactionType type, ReactionSubject subject)
        {
            await Task.Delay(1);
            if (subject == ReactionSubject.Profile && userId == toId.Id)
            {
                throw new Exception("Cannot react to own profile");
            }

            var reaction = new ActivityObject {
                actor = new List<BaseObject> {
                    new Common.Person {
                        objectId = userId
                    }
                }.ToCollection(),
                to = new List<BaseObject> {
                    new Common.Person {
                        objectId = toId.Id,
                        PeerId = toId.PeerId
                    }
                }.ToCollection(),
                type = type.ToString(),
                published = DateTime.UtcNow,
            };

            //var reactions = GetReactionsList(subject);
            
            //reactions.Add(reaction);
            // await _dbContext.SaveChangesAsync();
            // await _reactables[subject].RefreshStats(toId);

            return reaction.objectId.Value;
        }

        public async Task UnReact(int userId, PublicId toId, ReactionType type, ReactionSubject subject)
        {
            await Task.Delay(1);
            //var reactions = GetReactionsList(subject);
            //BaseObject reaction = await reactions.Where(r => r.UserId == userId && r.ToId == toId.Id && r.ToPeerId == toId.PeerId && r.Type == type).FirstOrDefaultAsync();
            
            // if (reaction != null)
            // {
            //     reactions.Remove(reaction);
            //     await _dbContext.SaveChangesAsync();
            //     await _reactables[subject].RefreshStats(toId);
            // }
        }

        // private DbSet<BaseObject> GetReactionsList(ReactionSubject subject)
        // {
        //     switch (subject)
        //     {
        //         case ReactionSubject.Post:
        //         return _dbContext.PostReactions;
        //         case ReactionSubject.Shop:
        //         return _dbContext.ShopReactions;
        //         case ReactionSubject.Profile:
        //         return _dbContext.ProfileReactions;
        //         default:
        //         throw new Exception();
        //     }
        // }
    }
}