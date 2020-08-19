using System.Threading.Tasks;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;
using Tides.Models.Auth;
using Tides.Models.Public;

namespace PinkUmbrella.Services.Redis
{
    public class RedisRateLimitService : IRateLimitService
    {
        private static readonly ActorRateLimitModel Zero = new ActorRateLimitModel();
        private static readonly ActorRateLimitModel DefaultSingleUserLimit = new ActorRateLimitModel()
        {
            UploadImages = 150,
            UploadVideos = 10,
            ApiCall = 18000,
        }.SetActions(250).SetReactions(1000);

        private readonly RedisRepository _redis;

        public RedisRateLimitService(RedisRepository redis)
        {
            _redis = redis;
        }

        public Task<ActorRateLimitModel> GetAllLimitsForGroup(string group)
        {
            ActorRateLimitModel ret = null;
            switch (group)
            {
                case "dev": ret = new ActorRateLimitModel().SetAll(int.MaxValue);
                break;
                case "admin": ret = new ActorRateLimitModel
                {

                };
                break;
            }
            return Task.FromResult(ret);
        }

        public async Task<ActorRateLimitModel> GetAllLimitsForIP(IPAddressModel ip)
        {
            var res = await _redis.Get<ActorRateLimitModel>(ip, "limit");
            return res ?? DefaultSingleUserLimit;
        }

        public async Task<ActorRateLimitModel> GetAllLimitsForUser(PublicId userId)
        {
            var res = await _redis.Get<ActorRateLimitModel>(userId, "limit");
            return res ?? DefaultSingleUserLimit;
        }

        public async Task<ActorRateLimitModel> GetAllRatesForIP(IPAddressModel ip)
        {
            var res = await _redis.Get<ActorRateLimitModel>(ip, "rate");
            return res ?? Zero;
        }

        public async Task<ActorRateLimitModel> GetAllRatesForUser(PublicId userId)
        {
            var res = await _redis.Get<ActorRateLimitModel>(userId, "rate");
            return res ?? Zero;
        }

        public async Task<int> GetLimitForGroup(string group, string property)
        {
            var field = await _redis.FieldGet<ReactionsSummaryModel>(group, property, "limit");
            if (field == null)
            {
                var all = await GetAllLimitsForGroup(group);
                field = all.GetType().GetProperty(property).GetValue(all)?.ToString() ?? "0";
            }
            return int.Parse(field);
        }

        public async Task<int> GetLimitForIP(IPAddressModel ip, string property)
        {
            return int.Parse((await _redis.FieldGet<ReactionsSummaryModel>(ip, property, "limit")) ?? "0");
        }

        public async Task<int> GetLimitForUser(PublicId userId, string property)
        {
            return int.Parse((await _redis.FieldGet<ReactionsSummaryModel>(userId, property, "limit")) ?? "0");
        }

        public async Task<int> GetRateForIP(IPAddressModel ip, string property)
        {
            return int.Parse((await _redis.FieldGet<ReactionsSummaryModel>(ip, property, "rate")) ?? "0");
        }

        public async Task<int> GetRateForUser(PublicId userId, string property)
        {
            return int.Parse((await _redis.FieldGet<ReactionsSummaryModel>(userId, property, "rate")) ?? "0");
        }

        public async Task SetLimitForGroup(string group, string property, int? limit)
        {
            if (limit.HasValue)
            {
                await _redis.FieldSet<ReactionsSummaryModel>(group, property, limit.Value.ToString(), "limit");
            }
            else
            {
                await _redis.FieldDelete<ReactionsSummaryModel>(property, group, "limit");
            }
        }

        public async Task SetLimitForIP(IPAddressModel ip, string property, int? limit)
        {
            if (limit.HasValue)
            {
                await _redis.FieldSet<ReactionsSummaryModel>(ip, property, limit.Value.ToString(), "limit");
            }
            else
            {
                await _redis.FieldDelete<ReactionsSummaryModel>(property, ip, "limit");
            }
        }

        public async Task SetLimitForUser(PublicId userId, string property, int? limit)
        {
            if (limit.HasValue)
            {
                await _redis.FieldSet<ReactionsSummaryModel>(userId, property, limit.Value.ToString(), "limit");
            }
            else
            {
                await _redis.FieldDelete<ReactionsSummaryModel>(property, userId, "limit");
            }
        }

        public Task TryActorToId(PublicId userId, PublicId toId, string property)
        {
            return Task.CompletedTask;
        }

        public Task TryIP(IPAddressModel ip, string property)
        {
            return Task.CompletedTask;
        }
    }
}