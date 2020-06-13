using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using seattle.Models;
using seattle.Repositories;

namespace seattle.Services.Sql
{
    public class ReactionService : IReactionService
    {
        private readonly Dictionary<ReactionSubject, IReactableService> _reactables;
        private readonly SimpleDbContext _dbContext;

        public ReactionService(IEnumerable<IReactableService> reactables, SimpleDbContext dbContext)
        {
            _reactables = reactables.ToDictionary(k => k.Subject, v => v);
            _dbContext = dbContext;
        }

        public async Task<int> React(int userId, int toId, ReactionType type, ReactionSubject subject)
        {
            var reaction = new ReactionModel() {
                UserId = userId,
                ToId = toId,
                Type = type,
                WhenReacted = DateTime.UtcNow,
            };

            var reactions = GetReactionsList(subject);
            
            reactions.Add(reaction);
            await _dbContext.SaveChangesAsync();
            await _reactables[subject].RefreshStats(toId);

            return reaction.Id;
        }

        public async Task UnReact(int userId, int toId, ReactionType type, ReactionSubject subject)
        {
            var reactions = GetReactionsList(subject);
            ReactionModel reaction = null;
            switch (subject)
            {
                case ReactionSubject.Post:
                reaction = await _dbContext.PostReactions.Where(r => r.UserId == userId && r.ToId == toId && r.Type == type).FirstOrDefaultAsync();;
                break;
                case ReactionSubject.Shop:
                reaction = await _dbContext.ShopReactions.Where(r => r.UserId == userId && r.ToId == toId && r.Type == type).FirstOrDefaultAsync();;
                break;
                case ReactionSubject.Profile:
                reaction = await _dbContext.ProfileReactions.Where(r => r.UserId == userId && r.ToId == toId && r.Type == type).FirstOrDefaultAsync();;
                break;
                default:
                throw new Exception();
            }
            
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