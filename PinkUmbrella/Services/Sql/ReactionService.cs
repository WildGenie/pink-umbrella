using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models;
using PinkUmbrella.Services.Elastic;
using Estuary.Core;
using Tides.Models;
using Tides.Models.Public;
using Estuary.Services;
using Estuary.Actors;
using Estuary.Util;
using PinkUmbrella.Repositories;
using Estuary.Streams.Json;

// CommandFlags.FireAndForget
// var result = db.ScriptEvaluate(TransferScript,
//    new RedisKey[] { from, to }, new RedisValue[] { quantity });

namespace PinkUmbrella.Services.Sql
{
    public class ReactionService : BaseElasticService, IReactionService
    {
        private readonly IActivityStreamRepository _activityStream;
        private readonly IActivityStreamContentRepository _activityContent;
        private readonly IRateLimitService _rateLimitService;
        private readonly RedisRepository _redis;

        public ReactionService(
            IActivityStreamRepository activityStream,
            RedisRepository redis,
            IActivityStreamContentRepository activityContent,
            IRateLimitService rateLimitService)
        {
            _activityStream = activityStream;
            _redis = redis;
            _activityContent = activityContent;
            _rateLimitService = rateLimitService;
        }

        public Task DeleteSummary(ReactionsSummaryModel summary) => _redis.Delete(summary);

        public async Task<CollectionObject> Get(PublicId id, int? viewerId)
        {
            // var fromRedis = await _redis.Get<ReactionsSummaryModel>(id);
            // if (fromRedis == null)
            // {
            //     f
            // }
            //var reactions = GetReactionsList(subject);
            //return reactions.Where(r => r.ToId == id.Id && r.ToPeerId == id.PeerId && r.UserId == viewerId.Value).ToListAsync();
            return null;
        }

        public async Task<int> GetCount(PublicId toId, ReactionType type) => int.Parse((await _redis.FieldGet<ReactionsSummaryModel>(toId, $"{type}Count")) ?? "0");

        public Task<List<ReactionsSummaryModel>> GetMostBlocked(ActivityStreamFilter filter, int? viewerId)
        {
            throw new NotImplementedException();
        }

        public Task<List<ReactionsSummaryModel>> GetMostDisliked(ActivityStreamFilter filter, int? viewerId)
        {
            throw new NotImplementedException();
        }

        public Task<List<ReactionsSummaryModel>> GetMostFollowed(ActivityStreamFilter filter, int? viewerId)
        {
            throw new NotImplementedException();
        }

        public Task<List<ReactionsSummaryModel>> GetMostLiked(ActivityStreamFilter filter, int? viewerId)
        {
            throw new NotImplementedException();
        }

        public Task<List<ReactionsSummaryModel>> GetMostReported(ActivityStreamFilter filter, int? viewerId)
        {
            throw new NotImplementedException();
        }

        public Task<ReactionsSummaryModel> GetSummary(PublicId toId, int? viewerId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> HasBlockedViewer(PublicId id, int? viewerId)
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

        public Task<ReactionsSummaryModel> PutSummary(ReactionsSummaryModel summary)
        {
            throw new NotImplementedException();
        }

        public async Task<string> React(PublicId userId, PublicId toId, ReactionType type)
        {
            await _rateLimitService.TryActorToId(userId, toId, type.ToString());
            
            var reaction = (ActivityObject) Activator.CreateInstance(CustomJsonSerializer.TypeOf(type.ToString()));
            reaction.actor = new List<BaseObject> {
                new Common.Person {
                    PublicId = userId,
                }
            }.ToCollection();

            var target = await _activityContent.Get(toId, userId);
            if (target != null)
            {
                reaction.target = new List<BaseObject> { target }.ToCollection();

                var res = await _activityStream.Post("outbox", reaction);
                return reaction.id;
            }
            else
            {
                return null;
            }
        }

        // public async Task UnReact(PublicId userId, PublicId toId, ReactionType type) => 
        //         await _activityStream.Undo(new ActivityStreamFilter("outbox")
        //         {
        //             // publicId = toId,
        //             targetTypes = new string[] { toId.Type }
        //         }.FixType(type.ToString()));
    }
}