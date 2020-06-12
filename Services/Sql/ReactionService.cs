using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task React(int userId, int toId, ReactionType type, ReactionSubject subject)
        {
            var reaction = new ReactionModel() {
                UserId = userId,
                ToId = toId,
                Type = type,
                WhenReacted = DateTime.UtcNow,
            };

            switch (subject) {
                case ReactionSubject.Post:
                _dbContext.PostReactions.Add(reaction);
                break;
                case ReactionSubject.Shop:
                _dbContext.ShopReactions.Add(reaction);
                break;
                case ReactionSubject.Profile:
                _dbContext.ProfileReactions.Add(reaction);
                break;
            }
            await _dbContext.SaveChangesAsync();

            await _reactables[subject].RefreshStats(toId);
        }
    }
}