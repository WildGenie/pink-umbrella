using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PinkUmbrella.Repositories;
using Estuary.Core;
using Tides.Models;
using Tides.Models.Public;
using Estuary.Services;

namespace PinkUmbrella.Services.ActivityStream.Read
{
    public class BindMentionsToActivityStreamWhenReading : IActivityStreamPipe
    {
        private readonly IPublicProfileService _users;
        private readonly StringRepository _strings;

        public BindMentionsToActivityStreamWhenReading(IPublicProfileService users, StringRepository strings)
        {
            _users = users;
            _strings = strings;
        }

        public async Task<BaseObject> Pipe(ActivityDeliveryContext ctx)
        {
            if (ctx.IsReading)
            {
                // var mentions = await _dbContext.Mentions.Where(m => m.PostId == p.Id).ToListAsync();
                // foreach (var m in mentions)
                // {
                //     m.MentionedPublicUser = await _users.GetUser(new PublicId(m.MentionedUserId, m.MentionedUserPeerId), value.ViewerId);
                // }
                // value.Mentions = mentions.Where(m => m.MentionedPublicUser != null).ToList();
            }
            return null;
        }
    }
}