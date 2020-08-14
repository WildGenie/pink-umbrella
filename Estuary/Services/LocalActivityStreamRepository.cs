using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Estuary.Actors;
using Estuary.Core;
using Estuary.Objects;

namespace Estuary.Services
{
    public class LocalActivityStreamRepository : BaseActivityStreamRepository
    {
        private readonly List<IActivityStreamPipe> _handlers;
        private readonly List<IActivityStreamBoxProvider> _boxes;

        public LocalActivityStreamRepository(IEnumerable<IActivityStreamPipe> handlers, IEnumerable<IActivityStreamBoxProvider> boxes)
        {
            _handlers = handlers.ToList();
            _boxes = boxes.ToList();
        }

        public override async Task<CollectionObject> GetAll(ActivityStreamFilter filter) => await GetBox(filter).Get(filter);

        public override async Task<BaseObject> Post(string index, ActivityObject item)
        {
            var publisher = item.actor?.items?.FirstOrDefault();
            var filter = new ActivityStreamFilter(index)
            {
                userId = publisher?.UserId,
                peerId = publisher?.PeerId,
            };
            return await Set(item, filter);
        }

        public override async Task<BaseObject> Set(ActivityObject item, ActivityStreamFilter filter)
        {
            var box = GetBox(filter);
            var ctx = new ActivityDeliveryContext { context = this, box = box, item = item, Filter = filter };
            var ret = await OnBeforeDelivery(ctx);
            if (ret == null)
            {
                ret = await box.Write(item);
                if (ret == null)
                {
                    throw new Exception("Box returned null");
                }
                else if (ret is Error err)
                {
                    return ret;
                }
                else if (ret is ActivityObject activity)
                {
                    ctx.item = activity;
                    ret = await OnAfterDelivery(ctx);
                }
                else
                {
                    throw new ArgumentException($"Box returned invalid type {ret.type ?? ret.GetType().FullName}");
                }
            }
            
            return ret;
        }

        public override IActivityStreamBox GetBox(ActivityStreamFilter filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }
            if (string.IsNullOrWhiteSpace(filter.index))
            {
                throw new ArgumentNullException(nameof(filter.index));
            }
            else
            {
                foreach (var boxProvider in _boxes)
                {
                    var box = boxProvider.Resolve(filter, this);
                    if (box != null)
                    {
                        return box;
                    }
                }
                var boxes = typeof(ActorObject).GetProperties().Where(p => p.PropertyType.IsSubclassOf(typeof(CollectionObject))).Select(p => p.Name).ToList();
                throw new ArgumentOutOfRangeException($"Invalid index {filter.index} (must be one of {string.Join(", ", boxes)})");
            }
        }

        private async Task<BaseObject> OnAfterDelivery(ActivityDeliveryContext ctx)
        {
            ctx.IsReading = false;
            ctx.IsWriting = true;
            ctx.HasWritten = true;
            return await GetPipe().Pipe(ctx);
        }

        private async Task<BaseObject> OnBeforeDelivery(ActivityDeliveryContext ctx)
        {
            ctx.IsReading = false;
            ctx.IsWriting = true;
            ctx.HasWritten = false;
            return await GetPipe().Pipe(ctx);
        }

        public override ActivityStreamPipe GetPipe() => new ActivityStreamPipe(this._handlers);
    }
}