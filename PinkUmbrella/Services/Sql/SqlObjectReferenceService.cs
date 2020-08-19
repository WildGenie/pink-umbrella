using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PinkUmbrella.Models;
using PinkUmbrella.Repositories;
using Estuary.Core;
using Tides.Models;
using Tides.Models.Public;
using Estuary.Actors;
using PinkUmbrella.Util;

namespace PinkUmbrella.Services.Sql
{
    public class SqlObjectReferenceService : IObjectReferenceService
    {
        protected readonly SimpleDbContext _db;
        
        public SqlObjectReferenceService(SimpleDbContext dbContext)
        {
            _db = dbContext;
        }
        
        protected DbSet<ObjectContentModel> GetHandleList(string type)
        {
            switch (type)
            {
                case "Organization": return _db.Shops;
                case "Article": return _db.Articles;
                case "Note": return _db.Notes;
            }
            return null;
        }

        public Task<ObjectContentModel> GetByHandle(string handle, string type) => GetHandleList(type)?.FirstOrDefaultAsync(h => h.Handle == handle);

        public async Task<ObjectContentModel> GetById(int id, string type)
        {
            if (type == "Actor")
            {
                var u = await _db.Users.FindAsync(id);
                return new ObjectContentModel
                {
                    Handle = u.Handle, Name = u.DisplayName, Summary = u.Bio, Id = u.Id,
                    Published = u.WhenCreated, Updated = u.WhenLastUpdated, Deleted = u.WhenDeleted,
                    // Visibility = u.Visibility
                };
            }
            else
            {
                return await (GetHandleList(type) ?? throw new ArgumentOutOfRangeException()).FindAsync(id);
            }
        }

        public async Task<List<ObjectContentModel>> GetCompletionsFor(string prefix, string type)
        {
            prefix = prefix.ToLower();
            return await GetHandleList(type)?.Where(t => t.Handle.ToLower().StartsWith(prefix)).Take(10).ToListAsync();
        }

        public async Task<bool> HandleExists(string handle, string type) => (await GetByHandle(handle, type)) != null;

        public async Task<ObjectContentModel> UploadObject(PublicId publisherId, BaseObject obj)
        {
            var newObj = new ObjectContentModel
            {
                UserId = publisherId.Id.Value,
                Published = DateTime.UtcNow,
                Content = obj.content, Summary = obj.summary, Name = obj.name,
                MediaType = obj.mediaType,
                Visibility = Enum.TryParse(typeof(Visibility), (obj.audience ?? obj.to)?.items?.OfType<ActorObject>()?.FirstOrDefault()?.Handle, out var vis) ? (Visibility) vis : Visibility.VISIBLE_TO_WORLD,
                Deleted = obj.deleted,
                Updated = obj.updated,
                IsMature = obj.IsMature ?? false,

                AttachmentCSV = obj.attachment.ToCsv(),
                AttributedToCSV = obj.attributedTo.ToCsv(),
                AudienceCSV = obj.audience.ToCsv(),
                ContextCSV = obj.context.ToCsv(),
                IconCSV = obj.icon.ToCsv(),
                ImageCSV = obj.image.ToCsv(),
                InReplyToCSV = obj.inReplyTo.ToCsv(),
                LocationCSV = obj.location.ToCsv(),
                RepliesCSV = obj.replies.ToCsv(),
                FromCSV = obj.from.ToCsv(),
                TagCSV = obj.tag.ToCsv(),
                ToCSV = obj.to.ToCsv(),
                BtoCSV = obj.bto.ToCsv(),
                CcCSV = obj.cc.ToCsv(),
                BccCSV = obj.bcc.ToCsv(),
            };
            var list = GetHandleList(obj.type) ?? throw new ArgumentOutOfRangeException(nameof(obj.type), obj.type, "Invalid type");
            await list.AddAsync(newObj);
            await _db.SaveChangesAsync();
            obj.objectId = newObj.Id;
            obj.PeerId = publisherId.PeerId;
            obj.PublicId.UserId = publisherId.Id;
            obj.published = newObj.Published;
            return newObj;
        }
    }
}