using System.Threading.Tasks;
using Estuary.Core;
using Estuary.Pipes;
using Estuary.Services;
using PinkUmbrella.Repositories;
using static Estuary.Activities.Common;

namespace PinkUmbrella.Services.ActivityStream.Write.Inbox
{
    public class InboxActivityActionHandlerWhenWritingPipe: ActivityActionHandlerAdapterPipe
    {
        private readonly IObjectReferenceService _ids;
        private readonly IObjectReferenceService _handles;
        private readonly StringRepository _strings;
        private readonly ITagService _tags;

        public InboxActivityActionHandlerWhenWritingPipe(
            IObjectReferenceService ids,
            IObjectReferenceService handles,
            StringRepository strings,
            ITagService tags)
        {
            _ids = ids;
            _handles = handles;
            _strings = strings;
            _tags = tags;
        }

        public override Task<BaseObject> Pipe(ActivityDeliveryContext ctx)
        {
            if (ctx.IsWriting && ctx.box.filter.index == "inbox")
            {
                return base.Pipe(ctx);
            }
            else
            {
                return Task.FromResult<BaseObject>(null);
            }
        }

        public async Task<BaseObject> Delete(ActivityDeliveryContext ctx, Delete delete)
        {
            //     var now = DateTime.UtcNow;
            //     shop.WhenDeleted = now;
            //     shop.LastUpdated = now;
            //     await _dbContext.SaveChangesAsync();
            return null;
        }

        public async Task<BaseObject> Create(ActivityDeliveryContext ctx, Create create)
        {
            if (ctx.HasWritten)
            {
                // deliver to other inboxes
                // var followers = await _users.GetFollowers(new PublicId(post.UserId, post.PeerId), post.UserId);
                // if (followers.Length > 0)
                // {
                //     await _notifications.Publish(new Models.AhPushIt.Notification() {
                //         FromUserId = post.UserId,
                //         SubjectId = post.Id,
                //         Priority = NotificationPriority.Normal,
                //         Subject = ReactionSubject.Post,
                //         Type = NotificationType.TEXT_POST_FOLLOWED_USER,
                //     }, followers.Select(u => u.PublicId).ToArray());
                // }
            }
            else
            {
                
            }
            return null;
        }
    }
}