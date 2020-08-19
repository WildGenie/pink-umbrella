using System;
using System.Threading.Tasks;
using Estuary.Core;
using Estuary.Streams.Json;
using Estuary.Util;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Util;
using Tides.Models.Public;

namespace PinkUmbrella.Repositories
{
    public class SqlActivityStreamContentRepository : IActivityStreamContentRepository
    {
        private readonly SimpleDbContext _db;

        public SqlActivityStreamContentRepository(SimpleDbContext db)
        {
            _db = db;
        }

        public async Task<ObjectContentModel> GetContentModel(PublicId id)
        {
            if (id != null && id.IsLocal && id.Id.HasValue)
            {
                DbSet<ObjectContentModel> list = null;
                switch (id.Type)
                {
                    case "Note": list = _db.Notes; break;
                    case "Article": list = _db.Articles; break;
                    case "Event": list = _db.Events; break;
                    case "Place": list = _db.Places; break;
                    case "Shop": list = _db.Shops; break;
                }
                if (list != null)
                {
                    return await list.FindAsync(id.Id.Value);
                }
            }
            return null;
        }

        public async Task<BaseObject> Get(PublicId id, PublicId viewerId)
        {
            var target = (BaseObject) Activator.CreateInstance(CustomJsonSerializer.TypeOf(id.Type));
            target.PublicId = id;
            await BindSqlContent(target);
            return target;
        }

        public async Task BindSqlContent(BaseObject bindTo)
        {
            var content = await GetContentModel(bindTo.PublicId);
            //var publisher = await _users.GetUser(new PublicId(content.UserId, 0), null);
            if (content != null)
            {
                bindTo.name = content.Name;
                bindTo.summary = content.Summary;
                bindTo.content = content.Content;
                bindTo.mediaType = content.MediaType;
                //bindTo.vis = content.Visibility;
                bindTo.published = content.Published;
                bindTo.updated = content.Updated;
                bindTo.deleted = content.Deleted;
                bindTo.IsMature = content.IsMature;

                bindTo.attachment = content.AttachmentCSV.ParseIdUrlFromCSV()?.ToCollection();
                bindTo.attributedTo = content.AttributedToCSV.ParseIdUrlFromCSV()?.ToCollection();
                bindTo.audience = content.AudienceCSV.ParseIdUrlFromCSV()?.ToCollection();
                bindTo.context = content.ContextCSV.ParseIdUrlFromCSV()?.ToCollection();
                bindTo.icon = content.IconCSV.ParseIdUrlFromCSV()?.ToCollection();
                bindTo.image = content.ImageCSV.ParseIdUrlFromCSV()?.ToCollection();
                bindTo.inReplyTo = content.InReplyToCSV.ParseIdUrlFromCSV()?.ToCollection();
                bindTo.location = content.LocationCSV.ParseIdUrlFromCSV()?.ToCollection();
                bindTo.replies = content.RepliesCSV.ParseIdUrlFromCSV()?.ToCollection();
                bindTo.tag = content.TagCSV.ParseIdUrlFromCSV()?.ToCollection();
                bindTo.from = content.FromCSV.ParseIdUrlFromCSV()?.ToCollection();
                bindTo.to = content.ToCSV.ParseIdUrlFromCSV()?.ToCollection();
                bindTo.bto = content.BtoCSV.ParseIdUrlFromCSV()?.ToCollection();
                bindTo.cc = content.CcCSV.ParseIdUrlFromCSV()?.ToCollection();
                bindTo.bcc = content.BccCSV.ParseIdUrlFromCSV()?.ToCollection();
            }
        }
    }
}