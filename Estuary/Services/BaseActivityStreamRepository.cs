using System.Collections.Generic;
using System.Threading.Tasks;
using Estuary.Core;
using Estuary.Objects;
using Estuary.Util;
using Tides.Models.Public;
using static Estuary.Activities.Common;

namespace Estuary.Services
{
    public abstract class BaseActivityStreamRepository : IActivityStreamRepository
    {
        public Task<BaseObject> Get()
        {
            throw new System.NotImplementedException();
        }

        public Task<BaseObject> GetActor(ActivityStreamFilter filter) => GetFirstOrError(GetActors(filter));

        public Task<CollectionObject> GetActors(ActivityStreamFilter filter)
        {
            filter.types = filter.types ?? new string[] { "Actor" };
            return GetAll(filter);
        }

        public Task<BaseObject> Get(ActivityStreamFilter filter)
        {
            return GetFirstOrError(GetAll(filter));
        }

        public abstract Task<CollectionObject> GetAll(ActivityStreamFilter filter);

        public Task<CollectionObject> GetFollowers(PublicId publicId, int? viewerId) => GetActors(new ActivityStreamFilter("followers") { publicId = publicId, viewerId = viewerId });

        public Task<CollectionObject> GetFollowing(PublicId publicId, int? viewerId) => GetActors(new ActivityStreamFilter("following") { publicId = publicId, viewerId = viewerId });

        // public Task<CollectionObject> GetInventories(ActivityStreamFilter filter)
        // {
        //     filter.types = new string[] { "Inventory" };
        //     return GetAll(filter);
        // }

        // public Task<BaseObject> GetInventory(ActivityStreamFilter filter) => GetFirstOrError(GetInventories(filter));

        //public Task<BaseObject> GetMedia(ActivityStreamFilter filter) => GetFirstOrError(GetMedias(filter));

        public Task<CollectionObject> GetMedias(ActivityStreamFilter filter)
        {
            filter.types = new string[] { "Document" };
            return GetAll(filter);
        }

        // public Task<CollectionObject> GetMentions(ActivityStreamFilter filter)
        // {
        //     filter.types = new string[] { "Mention" };
        //     filter.index = "inbox";
        //     return GetAll(filter);
        // }

        public Task<BaseObject> GetPeer(ActivityStreamFilter filter) => GetFirstOrError(GetPeers(filter));

        public Task<CollectionObject> GetPeers(ActivityStreamFilter filter)
        {
            filter.types = new string[] { "Peer" };
            return GetAll(filter);
        }

        //public Task<BaseObject> GetPost(ActivityStreamFilter filter) => GetFirstOrError(GetPosts(filter));

        public Task<CollectionObject> GetPosts(ActivityStreamFilter filter)
        {
            //filter.objectTypes = new string[] { "Note", "Article", "Document" };
            return GetAll(filter);
        }

        public Task<CollectionObject> GetReplies(ActivityStreamFilter filter)
        {
            throw new System.NotImplementedException();
        }

        public Task<BaseObject> GetReply(ActivityStreamFilter filter) => GetFirstOrError(GetReplies(filter));

        public Task<BaseObject> GetResource(ActivityStreamFilter filter) => GetFirstOrError(GetResources(filter));

        public Task<CollectionObject> GetResources(ActivityStreamFilter filter)
        {
            filter.types = new string[] { "Resource" };
            return GetAll(filter);
        }

        public Task<BaseObject> GetShop(ActivityStreamFilter filter) => GetFirstOrError(GetShops(filter));

        public Task<CollectionObject> GetShops(ActivityStreamFilter filter)
        {
            filter.types = new string[] { "Organization" };
            return GetAll(filter);
        }

        private async Task<BaseObject> GetFirstOrError(Task<CollectionObject> task)
        {
            var res = await task;
            if (res.items != null && res.items.Count > 0)
            {
                return res.items[0];
            }
            else
            {
                return new Error { content = "Could not get first, list is empty" };
            }
        }

        public abstract IActivityStreamBox GetBox(ActivityStreamFilter filter);
        public abstract Task<BaseObject> Post(string index, ActivityObject item);
        public abstract ActivityStreamPipe GetPipe();
        public abstract Task<BaseObject> Set(ActivityObject item, ActivityStreamFilter filter);

        public async Task<BaseObject> Undo(ActivityStreamFilter filter)
        {
            var ret = new List<BaseObject>();
            var what = await GetAll(filter);
            if (what != null && (what.totalItems > 0 || what.items.Count > 0))
            {
                foreach (var w in what.items)
                {
                    var undo = new Undo
                    {
                        
                    };
                    ret.Add(await this.Post("outbox", undo));
                }
            }
            else
            {
                // could also return not found
                throw new FilterReturnedEmptyResultsException();
            }
            return ret.ToCollection();
        }
    }
}