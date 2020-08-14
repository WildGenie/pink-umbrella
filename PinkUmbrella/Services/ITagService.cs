using System.Collections.Generic;
using System.Threading.Tasks;
using PinkUmbrella.Models;
using Estuary.Core;
using Tides.Models.Public;

namespace PinkUmbrella.Services
{
    public interface ITagService
    {
        Task BindReferences(TagModel tag);

        Task<BaseObject> GetTag(string text, int? viewerId);
        
        Task<BaseObject> GetTag(int id, int? viewerId);

        Task<CollectionObject> GetMostBlockedTags();

        Task<CollectionObject> GetMostLikedTags();

        Task<CollectionObject> GetMostDislikedTags();

        Task<CollectionObject> GetMostUsedTagsForSubject(string subject);

        Task<CollectionObject> GetMostBlockedTagsForSubject(string subject);

        Task<CollectionObject> GetMostLikedTagsForSubject(string subject);

        Task<CollectionObject> GetMostDislikedTagsForSubject(string subject);

        Task<CollectionObject> GetMostUsedTags();

        Task<CollectionObject> GetTagsForSubject(string subject, int? viewerId);

        Task<BaseObject> TryGetOrCreateTag(BaseObject tag, int? viewerId);

        Task<CollectionObject> GetCompletionsForTag(string prefix);

        Task Save(List<TagModel> tags, int userId, PublicId toId);
    }
}