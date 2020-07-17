using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PinkUmbrella.Models;
using PinkUmbrella.Models.Public;
using PinkUmbrella.Services.Local;
using Poncho.Models;
using Poncho.Models.Public;

namespace PinkUmbrella.Services.Public
{
    public class PublicUserService: IPublicProfileService
    {
        private static DateTime _WhenLastSynced = DateTime.MinValue;

        private readonly IReactionService _reactions;
        private readonly IUserProfileService _locals;
        private readonly ITagService _tags;

        public PublicUserService(IReactionService reactions, IUserProfileService localProfiles, ITagService tags)
        {
            _reactions = reactions;
            _locals = localProfiles;
            _tags = tags;
        }

        public async Task<PublicProfileModel> GetUser(PublicId id, int? viewerId)
        {   
            var user = id.PeerId == 0 ? await _locals.GetUser(id.Id, viewerId) : null;
            return await Transform(user, id.PeerId, viewerId);
        }

        public async Task<PublicProfileModel> GetUser(string handle, int? viewerId)
        {   
            var find = await _locals.GetUser(handle, viewerId);
            if (find != null)
            {
                return await Transform(find, 0, viewerId);
            }
            else
            {
                //find = await _externals.GetUser(handle, viewerId);
            }
            return null;
        }

        public bool CanView(PublicProfileModel user, int? viewerId)
        {
            if (viewerId.HasValue && user.PeerId == 0 && user.UserId == viewerId.Value)
            {
                return true;
            }
            else if (user.HasBeenBlockedOrReported)
            {
                return false;
            }

            switch (user.Visibility)
            {
                case Visibility.HIDDEN: return false;
                case Visibility.VISIBLE_TO_FOLLOWERS:
                if (!user.Reactions.Any(r => r.Type == ReactionType.Follow))
                {
                    return false;
                }
                break;
                case Visibility.VISIBLE_TO_REGISTERED:
                if (!viewerId.HasValue)
                {
                    return false;
                }
                break;
            }
            return true;
        }

        public async Task BindReferences(PublicProfileModel user, int? viewerId)
        {
            if (viewerId.HasValue)
            {
                var id = new PublicId(user.UserId, user.PeerId);
                user.Reactions = await _reactions.Get(ReactionSubject.Profile, id, viewerId);
                var reactionTypes = user.Reactions.Select(r => r.Type).ToHashSet();
                user.HasLiked = reactionTypes.Contains(ReactionType.Like);
                user.HasDisliked = reactionTypes.Contains(ReactionType.Dislike);
                user.HasFollowed = reactionTypes.Contains(ReactionType.Follow);
                user.HasBlocked = reactionTypes.Contains(ReactionType.Block);
                user.HasReported = reactionTypes.Contains(ReactionType.Report);

                var blockOrReport = await _reactions.HasBlockedViewer(ReactionSubject.Profile, id, viewerId);
                user.HasBeenBlockedOrReported = blockOrReport;
            }

            if (user.PeerId == 0)
            {
                user.Tags = await _tags.GetTagsFor(user.UserId, ReactionSubject.Profile, viewerId);
            }
        }

        public async Task<PublicProfileModel> Transform(UserProfileModel privateModel, long peerId, int? viewerId)
        {
            if (privateModel != null)
            {
                var asPublic = new PublicProfileModel(privateModel, peerId);
                await BindReferences(asPublic, viewerId);
                if (CanView(asPublic, viewerId))
                {
                    return asPublic;
                }
            }
            return null;
        }

        public async Task<PublicProfileModel[]> GetFollowers(PublicId id, int? viewerId)
        {
            if (id.PeerId == 0)
            {
                return await Task.WhenAll((await _locals.GetFollowers(id.Id, viewerId)).Select(u => Transform(u, id.PeerId, viewerId)));
            }
            else
            {
                return null;
            }
        }

        public async Task<PublicProfileModel[]> GetFollowing(PublicId id, int? viewerId)
        {
            if (id.PeerId == 0)
            {
                return await Task.WhenAll((await _locals.GetFollowing(id.Id, viewerId)).Select(u => Transform(u, id.PeerId, viewerId)));
            }
            else
            {
                return null;
            }
        }

        public async Task<List<PublicProfileModel>> GetAllLocal()
        {
            var users = await _locals.GetAll(_WhenLastSynced);
            var ret = new List<PublicProfileModel>();
            foreach (var user in users)
            {
                var asPublic = new PublicProfileModel(user, 0);
                await BindReferences(asPublic, 0);
                ret.Add(asPublic);
            }
            _WhenLastSynced = DateTime.UtcNow;
            return ret;
        }
    }
}