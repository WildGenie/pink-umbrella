using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models;

namespace PinkUmbrella.Services
{
    public interface ITagService
    {
        Task BindReferences(TaggedModel tag, int? viewerId);

        Task BindReferences(TagModel tag);

        bool CanView(TaggedModel tag, int? viewerId);

        Task<TagModel> GetTag(string text, int? viewerId);
        
        Task<TagModel> GetTag(int id, int? viewerId);

        Task<PaginatedModel<TagModel>> GetMostBlockedTags();

        Task<PaginatedModel<TagModel>> GetMostLikedTags();

        Task<PaginatedModel<TagModel>> GetMostDislikedTags();

        Task<PaginatedModel<UsedTagModel>> GetMostUsedTagsForSubject(ReactionSubject subject);

        Task<PaginatedModel<TagModel>> GetMostBlockedTagsForSubject(ReactionSubject subject);

        Task<PaginatedModel<TagModel>> GetMostLikedTagsForSubject(ReactionSubject subject);

        Task<PaginatedModel<TagModel>> GetMostDislikedTagsForSubject(ReactionSubject subject);

        Task<PaginatedModel<UsedTagModel>> GetMostUsedTags();

        Task<List<TagModel>> GetTagsForSubject(ReactionSubject subject, int? viewerId);

        Task<List<TagModel>> GetTagsFor(int toId, ReactionSubject subject, int? viewerId);

        Task<TagModel> TryGetOrCreateTag(TagModel tag, int? viewerId);

        Task<List<TagModel>> GetCompletionsForTag(string prefix);

        Task Save(ReactionSubject subject, List<TagModel> tags, int userId, int toId);
    }
}