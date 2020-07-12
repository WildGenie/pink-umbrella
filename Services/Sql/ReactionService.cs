using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Models.Public;
using PinkUmbrella.Repositories;
using PinkUmbrella.Services.Elastic;

namespace PinkUmbrella.Services.Sql
{
    public class ReactionService : BaseElasticService, IReactionService
    {
        private readonly Dictionary<ReactionSubject, IReactableService> _reactables;
        private readonly SimpleDbContext _dbContext;

        public ReactionService(IEnumerable<IReactableService> reactables, SimpleDbContext dbContext)
        {
            _reactables = reactables.ToDictionary(k => k.Subject, v => v);
            _dbContext = dbContext;
        }

        public Task<List<ReactionModel>> Get(ReactionSubject subject, PublicId id, int? viewerId)
        {
            var reactions = GetReactionsList(subject);
            return reactions.Where(r => r.ToId == id.Id && r.ToPeerId == id.PeerId && r.UserId == viewerId.Value).ToListAsync();
        }

        public Task<int> GetCount(PublicId toId, ReactionType type, ReactionSubject subject) => _reactables[subject].GetCount(toId, type);

        public async Task<bool> HasBlockedViewer(ReactionSubject subject, PublicId id, int? viewerId)
        {
            if (0 == id.PeerId)
            {
                var reactions = GetReactionsList(subject);
                return (await reactions.FirstOrDefaultAsync(r => r.ToId == viewerId.Value && r.ToPeerId == 0 && r.UserId == id.Id && (r.Type == ReactionType.Block || r.Type == ReactionType.Report))) != null;
            }
            else
            {
                var client = GetClient();
                var response = await client.GetAsync<ReactionModel>(id.ToString());
                if (response.IsValid)
                {
                    var r = response.Source;
                    return r.ToId == viewerId.Value && r.ToPeerId == 0 && r.UserId == id.Id && (r.Type == ReactionType.Block || r.Type == ReactionType.Report);
                }
                else
                {
                    return false;
                }
            }
        }

        public async Task<int> React(int userId, PublicId toId, ReactionType type, ReactionSubject subject)
        {
            if (subject == ReactionSubject.Profile && userId == toId.Id)
            {
                throw new Exception("Cannot react to own profile");
            }

            var reaction = new ReactionModel() {
                UserId = userId,
                ToId = toId.Id,
                ToPeerId = toId.PeerId,
                Type = type,
                WhenReacted = DateTime.UtcNow,
            };

            var reactions = GetReactionsList(subject);
            
            reactions.Add(reaction);
            await _dbContext.SaveChangesAsync();
            await _reactables[subject].RefreshStats(toId);

            return reaction.Id;
        }

        public async Task UnReact(int userId, PublicId toId, ReactionType type, ReactionSubject subject)
        {
            var reactions = GetReactionsList(subject);
            ReactionModel reaction = await reactions.Where(r => r.UserId == userId && r.ToId == toId.Id && r.ToPeerId == toId.PeerId && r.Type == type).FirstOrDefaultAsync();
            
            if (reaction != null)
            {
                reactions.Remove(reaction);
                await _dbContext.SaveChangesAsync();
                await _reactables[subject].RefreshStats(toId);
            }
        }

        private DbSet<ReactionModel> GetReactionsList(ReactionSubject subject)
        {
            switch (subject)
            {
                case ReactionSubject.Post:
                return _dbContext.PostReactions;
                case ReactionSubject.Shop:
                return _dbContext.ShopReactions;
                case ReactionSubject.Profile:
                return _dbContext.ProfileReactions;
                default:
                throw new Exception();
            }
        }
    }
}