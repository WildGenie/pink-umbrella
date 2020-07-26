using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PinkUmbrella.Repositories;
using Tides.Core;
using Tides.Models;
using Tides.Models.Public;
using Tides.Services;
using static Tides.Objects.Common;

namespace PinkUmbrella.Services.Sql.ActivityStreamPipes
{
    public class BindMentionsToActivityStream : IActivityStreamPipe
    {
        private readonly IPublicProfileService _users;
        private readonly StringRepository _strings;

        public BindMentionsToActivityStream(IPublicProfileService users, StringRepository strings)
        {
            _users = users;
            _strings = strings;
        }

        public async Task<BaseObject> Pipe(BaseObject value, bool isReading)
        {
            if (isReading)
            {
                // var mentions = await _dbContext.Mentions.Where(m => m.PostId == p.Id).ToListAsync();
                // foreach (var m in mentions)
                // {
                //     m.MentionedPublicUser = await _users.GetUser(new PublicId(m.MentionedUserId, m.MentionedUserPeerId), value.ViewerId);
                // }
                // value.Mentions = mentions.Where(m => m.MentionedPublicUser != null).ToList();
            }
            else
            {
                if (value.visibility != Visibility.HIDDEN)
                {
                    await ExtractMentions(value);
                    // if (post.Mentions.Any())
                    // {
                    //     _dbContext.Mentions.AddRange(post.Mentions);
                    //     postInsertChanges = true;new MentionModel() {
                        //     WhenMentioned = DateTime.UtcNow,
                        //     PostId = p.Id,
                        //     PostPeerId = p.PeerId,
                        //     Post = p,
                        //     MentionedUserId = user.UserId,
                        //     MentionedUserPeerId = user.PeerId,
                        //     MentionedPublicUser = user
                        // }
                    // }
                }
            }
            return value;
        }

        private async Task ExtractMentions(BaseObject p)
        {
            var handles = _strings.ExtractMentionsRegex.Matches(p.content).Cast<Match>().Select(m => m.Value.Substring(1)).ToList();
            var mentionedUserIds = new List<PublicId>();

            foreach (var handle in handles) {
                var user = await _users.GetUser(handle, p.UserId);
                if (user != null) {
                    if (!mentionedUserIds.Contains(user.PublicId))
                    {
                        mentionedUserIds.Add(user.PublicId);
                        p.to.Add(user);
                    }
                }
            }
        }
    }
}