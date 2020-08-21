using System;
using System.Linq;
using System.Threading.Tasks;
using Estuary.Actors;
using Estuary.Core;
using Estuary.Objects;
using Estuary.Pipes;
using Estuary.Services;
using Estuary.Util;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;
using Tides.Models;
using static Estuary.Activities.Common;

namespace PinkUmbrella.Services.ActivityStream.Write.Outbox
{
    public class OutboxActivityActionHandlerWhenWritingPipe: ActivityActionHandlerAdapterPipe
    {
        private readonly IObjectReferenceService _ids;
        private readonly IObjectReferenceService _handles;
        private readonly StringRepository _strings;
        private readonly ITagService _tags;
        private readonly RedisRepository _redis;

        public OutboxActivityActionHandlerWhenWritingPipe(
            IObjectReferenceService ids,
            IObjectReferenceService handles,
            StringRepository strings,
            ITagService tags,
            RedisRepository redis)
        {
            _ids = ids;
            _handles = handles;
            _strings = strings;
            _tags = tags;
            _redis = redis;
        }

        public override async Task<BaseObject> Pipe(ActivityDeliveryContext ctx)
        {
            if (ctx.IsWriting && ctx.box.filter.index == "outbox")
            {
                if (ctx.HasWritten)
                {
                    if (ctx.item.to != null && ctx.item.to.items.Count > 0)
                    {
                        var vis = ctx.item.to.items.Concat(ctx.item.obj.audience.items).
                                    OfType<ActorObject>().Where(a => a.BaseType == "Actor" && a.type == "Group").
                                    Select(a => a.Handle).ToArray();
                        
                        // deliver to other outboxes
                        if (vis.Contains("followers"))
                        {
                            var publisher = ctx.item.GetPublisher();
                            var followers = await ctx.context.GetAll(new ActivityStreamFilter("followers") { id = publisher.PublicId });
                            if (followers != null && followers.items.Count > 0)
                            {
                                // Send to followers
                                foreach (var follower in followers.items.OfType<ActorObject>())
                                {
                                    await ctx.context.Set(ctx.item, new ActivityStreamFilter("inbox") { id = publisher.PublicId });
                                }
                            }
                        }
                        if (vis.Contains("registered"))
                        {
                            await ctx.context.Set(ctx.item, new ActivityStreamFilter("sharedOutbox") { id = ctx.Filter.id });
                        }

                        // await ctx.context.Post("sharedOutbox", ctx.item);
                    }
                    else
                    {
                        await ctx.context.Set(ctx.item, new ActivityStreamFilter("sharedOutbox") { id = ctx.Filter.id });
                    }
                    
                    // if (!string.IsNullOrWhiteSpace(ctx.item.obj?.id))
                    // {
                    //     await ctx.context.Set(ctx.item, new ActivityStreamFilter(ctx.item.obj.id));
                    // }
                    // if (ctx.Filter.index == "sharedOutbox")
                    // {
                    //     if (vis == null)
                    //     {
                    //         await ctx.context.Set(ctx.item, new ActivityStreamFilter("globalOutbox"));
                    //     }
                    // }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(ctx.item.id))
                    {
                        ctx.item.id = Guid.NewGuid().ToString();
                        ctx.item.published = DateTime.UtcNow;
                    }
                }
                return await base.Pipe(ctx);
            }
            else
            {
                return null;
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
                var publisher = create.GetPublisher();
                if (publisher != null && publisher.PublicId.IsLocal && ctx.item.obj != null && ctx.item.obj.PublicId.IsLocal && ctx.item.obj.tag != null && ctx.item.obj.tag.items.Any())
                {
                    var tags = ctx.item.obj.tag.items.Select(t => new TagModel { Tag = t.content }).ToList();
                    await _tags.Save(tags, publisher.UserId.Value, ctx.item.obj.PublicId);
                }
            }
            else
            {
                var publisherId = create.GetPublisher()?.PublicId;
                if (publisherId != null)
                {
                    if (create.obj.objectId == null && (create.obj.PeerId == null || create.obj.PeerId == 0))
                    {
                        create.published = DateTime.UtcNow;
                        await _ids.UploadObject(publisherId, create.obj);
                    }
                }
                else
                {
                    throw new ArgumentNullException(nameof(publisherId));
                }

                if (create.obj != null && create.obj is ActorObject actor && !string.IsNullOrWhiteSpace(actor.preferredUsername))
                {
                    if (await _handles.HandleExists(actor.preferredUsername, actor.type))
                    {
                        throw new ArgumentException($"{actor.type} preferredUsername taken", nameof(actor.preferredUsername));
                    }
                }
            }
            return null;
        }

        public async Task<BaseObject> Like(ActivityDeliveryContext ctx, Like like) => await ReactAndGetSummary(ctx, like, ReactionType.Like);

        public async Task<BaseObject> Dislike(ActivityDeliveryContext ctx, Dislike dislike) => await ReactAndGetSummary(ctx, dislike, ReactionType.Dislike);

        public async Task<BaseObject> Upvote(ActivityDeliveryContext ctx, Upvote upvote) => await ReactAndGetSummary(ctx, upvote, ReactionType.Upvote);

        public async Task<BaseObject> Downvote(ActivityDeliveryContext ctx, Downvote downvote) => await ReactAndGetSummary(ctx, downvote, ReactionType.Downvote);

        public async Task<BaseObject> Ignore(ActivityDeliveryContext ctx, Ignore ignore) => await ReactAndGetSummary(ctx, ignore, ReactionType.Ignore);

        public async Task<BaseObject> Report(ActivityDeliveryContext ctx, Report report) => await ReactAndGetSummary(ctx, report, ReactionType.Report);

        public async Task<BaseObject> Block(ActivityDeliveryContext ctx, Block block) => await ReactAndGetSummary(ctx, block, ReactionType.Block);

        public async Task<BaseObject> Follow(ActivityDeliveryContext ctx, Follow follow) => await ReactAndGetSummary(ctx, follow, ReactionType.Follow);

        private async Task<BaseObject> ReactAndGetSummary(ActivityDeliveryContext ctx, ActivityObject reaction, ReactionType reactionType)
        {
            if (ctx.HasWritten)
            {
                foreach (var target in ctx.item.target.items)
                {
                    await _redis.Increment<ReactionsSummaryModel>($"{reactionType}Count", target.id, null);
                }
            }
            return null;
        }

        public async Task<BaseObject> Undo(ActivityDeliveryContext ctx, Undo undo)
        {
            if (ctx.HasWritten)
            {
                switch (undo.obj.type)
                {
                    case nameof(Like):
                    await UnReact(undo.obj as Like, ctx);
                    break;
                    case nameof(Dislike):
                    await UnReact(undo.obj as Dislike, ctx);
                    break;
                    case nameof(Upvote):
                    await UnReact(undo.obj as Upvote, ctx);
                    break;
                    case nameof(Downvote):
                    await UnReact(undo.obj as Downvote, ctx);
                    break;
                    case nameof(Ignore):
                    await UnReact(undo.obj as Ignore, ctx);
                    // todo: remove from ignore list
                    break;
                    case nameof(Report):
                    await UnReact(undo.obj as Report, ctx);
                    // todo: remove from report list
                    break;
                    case nameof(Flag):
                    await UnReact(undo.obj as Flag, ctx);
                    // todo: remove from flag list
                    break;
                    case nameof(Block):
                    await UnReact(undo.obj as Block, ctx);
                    // todo: remove from block list
                    break;
                    case nameof(Follow):
                    await UnReact(undo.obj as Follow, ctx);
                    // todo: send unfollow
                    break;
                    default:
                    return new Error { statusCode = 403, errorCode = 403, summary = $"Cannot undo {undo.obj.type}" };
                }
            }
            return null;
        }

        private async Task UnReact(ActivityObject reaction, ActivityDeliveryContext ctx)
        {
            foreach (var target in (reaction ?? throw new ArgumentNullException(nameof(reaction))).target.items)
            {
                await _redis.Decrement<ReactionsSummaryModel>($"{reaction.type}Count", target.id, null);
            }
        }
    }
}