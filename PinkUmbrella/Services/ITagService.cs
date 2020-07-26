using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models;
using Tides.Core;
using Tides.Models;

namespace PinkUmbrella.Services
{
    public interface ITagService
    {
        Task BindReferences(TaggedModel tag, int? viewerId);

        Task BindReferences(TagModel tag);

        Task<BaseObject> GetTag(string text, int? viewerId);
        
        Task<BaseObject> GetTag(int id, int? viewerId);

        Task<CollectionObject> GetMostBlockedTags();

        Task<CollectionObject> GetMostLikedTags();

        Task<CollectionObject> GetMostDislikedTags();

        Task<CollectionObject> GetMostUsedTagsForSubject(ReactionSubject subject);

        Task<CollectionObject> GetMostBlockedTagsForSubject(ReactionSubject subject);

        Task<CollectionObject> GetMostLikedTagsForSubject(ReactionSubject subject);

        Task<CollectionObject> GetMostDislikedTagsForSubject(ReactionSubject subject);

        Task<CollectionObject> GetMostUsedTags();

        Task<CollectionObject> GetTagsForSubject(ReactionSubject subject, int? viewerId);

        Task<CollectionObject> GetTagsFor(int toId, ReactionSubject subject, int? viewerId);

        Task<BaseObject> TryGetOrCreateTag(BaseObject tag, int? viewerId);

        Task<CollectionObject> GetCompletionsForTag(string prefix);

        Task Save(ReactionSubject subject, List<TagModel> tags, int userId, int toId);

        Task<BaseObject> Transform(TagModel tag);

        Task<BaseObject> Transform(TaggedModel tag);
    }
}