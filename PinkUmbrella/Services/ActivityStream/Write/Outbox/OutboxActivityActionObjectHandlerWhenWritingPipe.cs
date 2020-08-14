using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Estuary.Core;
using Estuary.Objects;
using Estuary.Pipes;
using Estuary.Services;
using Estuary.Util;
using PinkUmbrella.Repositories;
using Tides.Models.Public;
using static Estuary.Activities.Common;
using static Estuary.Actors.Common;
using static Estuary.Objects.Common;

namespace PinkUmbrella.Services.ActivityStream.Write.Outbox
{
    public class OutboxActivityActionObjectHandlerWhenWritingPipe: ActivityActionObjectHandlerAdapterPipe
    {
        private readonly StringRepository _strings;
        private readonly ITagService _tags;

        public OutboxActivityActionObjectHandlerWhenWritingPipe(StringRepository strings, ITagService tags)
        {
            _strings = strings;
            _tags = tags;
        }

        public override Task<BaseObject> Pipe(ActivityDeliveryContext ctx)
        {
            if (ctx.IsWriting && ctx.box.filter.index == "outbox")
            {
                return base.Pipe(ctx);
            }
            else
            {
                return Task.FromResult<BaseObject>(null);
            }
        }

        private async Task ExtractMentions(BaseObject p)
        {
            var handles = _strings.ExtractMentionsRegex.Matches(p.content).Cast<Match>().Select(m => m.Value.Substring(1)).ToList();
            var mentionedUserIds = new List<PublicId>();

            foreach (var handle in handles)
            {
                // var user = await _users.GetUser(handle, p.UserId);
                // if (user != null)
                // {
                //     if (!mentionedUserIds.Contains(user.PublicId))
                //     {
                //         mentionedUserIds.Add(user.PublicId);
                //         p.to.Add(user);
                //     }
                // }
            }
        }

        public Task<BaseObject> BlockPerson(ActivityDeliveryContext ctx, Block block, Person person)
        {
            BaseObject ret = null;
            if (block.GetPublisher().PublicId == person.PublicId)
            {
                ret = new Error { statusCode = 403, errorCode = 403, summary = "Cannot block yourself" };
            }
            return Task.FromResult(ret);
        }

        public async Task<BaseObject> CreateNote(ActivityDeliveryContext ctx, Create create, Note note)
        {
            if (ctx.HasWritten)
            {
                if (create.to.items.Any())
                {
                    // _dbContext.Mentions.AddRange(create.to);
                    // new MentionModel()
                    // {
                    //     WhenMentioned = DateTime.UtcNow,
                    //     PostId = p.Id,
                    //     PostPeerId = p.PeerId,
                    //     Post = p,
                    //     MentionedUserId = user.UserId,
                    //     MentionedUserPeerId = user.PeerId,
                    //     MentionedPublicUser = user
                    // };
                }
//             // if (ctx.box.name != "sharedOutbox" && ctx.box.name != "sharedOutbox" &&
//             //     (ctx.item.visibility ?? Visibility.VISIBLE_TO_WORLD) == Visibility.VISIBLE_TO_WORLD)
//             // {
//             //     ctx.Forward(ctx.item, new Tides.Models.ActivityStreamFilter
//             //     {
//             //         index = "sharedOutbox",
//             //     });
//             // }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(note.content))
                {
                    await ExtractMentions(note);
                    await ExtractTags(create, note);
                }
            }
            return null;
        }

        private async Task ExtractTags(Create create, Note note)
        {
            var handles = _strings.ExtractTagsRegex.Matches(note.content).Cast<Match>().Select(m => m.Value.Substring(1)).ToList();
            var tagIds = new HashSet<string>();
            var tags = new List<BaseObject>();
            foreach (var handle in handles)
            {
                var tag = await _tags.TryGetOrCreateTag(new Note { content = handle }, create.GetPublisher().objectId);
                if (tag != null)
                {
                    if (!tagIds.Contains(tag.content))
                    {
                        tags.Add(tag);
                        tagIds.Add(tag.content);
                    }
                }
            }

            if (tags.Any())
            {
                if (note.tag == null)
                {
                    note.tag = tags.ToCollection();
                }
                else
                {
                    note.tag.items.AddRange(tags);
                }
            }
        }

        public async Task<BaseObject> CreateOrganization(ActivityDeliveryContext ctx, Create create, Organization org)
        {
            return null;
        }
    }
}