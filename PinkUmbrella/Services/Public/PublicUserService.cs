using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PinkUmbrella.Models;
using PinkUmbrella.Services.Local;
using Estuary.Core;
using Tides.Models.Public;
using Estuary.Actors;
using Estuary.Util;

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

        public async Task<BaseObject> GetUser(PublicId id, int? viewerId)
        {   
            var user = id.PeerId == 0 ? await _locals.GetUser(id.Id.Value, viewerId) : null;
            return await Transform(user, id.PeerId.Value, viewerId);
        }

        public async Task<BaseObject> GetUser(string handle, int? viewerId)
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

        public bool CanView(BaseObject user, int? viewerId)
        {
            if (viewerId.HasValue && user.PeerId == 0 && user.UserId == viewerId.Value)
            {
                return true;
            }
            else if (user.HasBeenBlockedOrReportedByPublisher)
            {
                return false;
            }
            return true;
        }

        public async Task<ActorObject> Transform(UserProfileModel privateModel, long peerId, int? viewerId)
        {
            if (privateModel != null)
            {
                if (peerId > 0)
                {
                    throw new NotSupportedException();
                }

                var asPublic = new Common.Person();
                asPublic.PublicId.Copy(privateModel.PublicId);
                asPublic.PeerId = peerId;
                asPublic.BanExpires = privateModel.BanExpires;
                asPublic.BanReason = privateModel.BanReason;
                asPublic.summary = privateModel.Bio;
                // asPublic.BioVisibility = privateModel.BioVisibility;
                asPublic.name = privateModel.DisplayName;
                asPublic.Email = privateModel.Email;
                // this.EmailVisibility = user.EmailVisibility;
                asPublic.Handle = privateModel.Handle;
                // asPublic.audience = privateModel.Visibility.ToAudience(asPublic).IntoNewList<BaseObject>().ToCollection();
                asPublic.published = privateModel.WhenCreated;
                asPublic.WhenLastLoggedIn = privateModel.WhenLastLoggedIn;
                // asPublic.WhenLastLoggedInVisibility = privateModel.WhenLastLoggedInVisibility;
                asPublic.WhenLastOnline = privateModel.WhenLastOnline;
                // asPublic.WhenLastOnlineVisibility = privateModel.WhenLastOnlineVisibility;
                asPublic.updated = privateModel.WhenLastUpdated;
                
                asPublic.PrivateModel = privateModel;

                await Task.Delay(1);
                return asPublic;
            }
            return null;
        }

        public async Task<CollectionObject> GetFollowers(PublicId id, int? viewerId)
        {
            if (id.PeerId == 0)
            {
                return (await Task.WhenAll((await _locals.GetFollowers(id.Id.Value, viewerId)).Select(u => Transform(u, id.PeerId.Value, viewerId)))).ToCollection();
            }
            else
            {
                return null;
            }
        }

        public async Task<CollectionObject> GetFollowing(PublicId id, int? viewerId)
        {
            if (id.PeerId == 0)
            {
                return (await Task.WhenAll((await _locals.GetFollowing(id.Id.Value, viewerId)).Select(u => Transform(u, id.PeerId.Value, viewerId)))).ToCollection();
            }
            else
            {
                return null;
            }
        }

        public async Task<CollectionObject> GetAllLocal()
        {
            var users = await _locals.GetAll(_WhenLastSynced);
            var ret = new List<BaseObject>();
            foreach (var user in users)
            {
                ret.Add(await Transform(user, 0, 0));
            }
            _WhenLastSynced = DateTime.UtcNow;
            return ret.ToCollection();
        }
    }
}