using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Estuary.Core;
using Estuary.Objects;
using Estuary.Streams;
using Estuary.Streams.Json;
using Estuary.Util;

namespace Estuary.Services.Boxes
{
    public class BaseActivityStreamBox : IActivityStreamBox
    {
        public static readonly Regex IndexRegex = new Regex(@"\w{1,20}");

        public CustomJsonSerializer _serializer { get; } = new CustomJsonSerializer();
        
        public List<BaseObjectStreamWriter> Writers { get; } = new List<BaseObjectStreamWriter>();

        public ActivityStreamFilter filter { get; }

        public IActivityStreamRepository ctx { get; }
        
        public List<ActivityStreamFilter> Indexes { get; } = new List<ActivityStreamFilter>();

        public BaseActivityStreamBox(ActivityStreamFilter filter, IActivityStreamRepository ctx)
        {
            this.filter = filter;
            this.ctx = ctx;
            //Writers.Add(new ObjectStreamWriter(File.Open(PathOf(null), FileMode.Append, FileAccess.Write, FileShare.Read), _serializer, ctx));
            Writers.Add(new ObjectIdStreamWriter(OpenWrite(filter.ToPath(null)), ctx));
        }

        public void CreateIndexOn(ActivityStreamFilter filter)
        {
            filter = this.filter.Extend(filter);
            var path = filter.ToUri(null);
            Writers.Add(new FilteredObjectIdStreamWriter(filter, OpenWrite(path), ctx));
            Indexes.Add(filter);
        }

        private string Localize(string path)
        {
            path = $"Upload/activitystreams/{path}";
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            return path;
        }

        public FileStream OpenWrite(string path)
        {
            path = Localize(path);
            return File.Open(path, FileMode.Append, FileAccess.Write, FileShare.Read);
        }

        public async Task<CollectionObject> Get(ActivityStreamFilter filter)
        {
            var ret = new List<BaseObject>();
            var pipe = this.ctx.GetPipe();
            var ctx = new ActivityDeliveryContext
            {
                IsReading = true,
                context = this.ctx, box = this, Filter = filter
            };
            var tryCount = 0;
            using (var reader = await OpenReader(filter))
            {
                while (true)
                {
                    var item = await reader.Read();
                    if (item == null)
                    {
                        if (tryCount == 0)
                        {
                            return null;
                        }
                        break;
                    }
                    else if (item is Error err)
                    {
                        ret.Add(item);
                    }
                    else if (item is ActivityObject activity)
                    {
                        if (filter.IsMatch(activity))
                        {
                            ctx.item = activity;
                            var res = await pipe.Pipe(ctx) ?? ctx.item;
                            if (res != null)
                            {
                                ret.Add(res);
                            }
                        }
                    }
                    else
                    {
                        throw new Exception($"Invalid type {item.type}");
                    }
                    tryCount++;
                }
            }
            if (ctx.Undos.Count > 0)
            {
                ret = ret.Where(r => !ctx.Undos.Contains(r.id)).ToList();
            }
            return ret.ToCollection();
        }

        public async Task<BaseObject> Write(BaseObject item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            else if (string.IsNullOrWhiteSpace(item.id))
            {
                throw new ArgumentNullException("item.id");
            }

            item.published = item.published ?? DateTime.UtcNow;

            var storePath = Localize(filter.ToPath(item.PublicId));
            if (!File.Exists(storePath))
            {
                using (var storeWriter = new ObjectStreamWriter(File.Open(storePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read), _serializer, ctx))
                {
                    await storeWriter.Write(item);
                }

                foreach (var writer in Writers)
                {
                    await writer.Write(item);
                }
            }
            return item;
        }

        protected virtual Task<BaseObjectStreamReader> OpenReader(ActivityStreamFilter filter)
        {
            BaseObjectStreamReader ret = null;
            string path = null;
            foreach (var index in Indexes)
            {
                if (index.Contains(filter))
                {
                    path = Localize(index.ToUri(null));
                    if (System.IO.File.Exists(path))
                    {
                        break;
                    }
                    else
                    {
                        path = null;
                    }
                }
            }
            
            if (path == null)
            {
                path = Localize(filter.ToUri(null));
                if (!System.IO.File.Exists(path))
                {
                    path = Localize(filter.ToPath(null));
                }
            }

            if (System.IO.File.Exists(path))
            {
                switch (System.IO.Path.GetExtension(path))
                {
                    case ".index":
                    {
                        var s = System.IO.File.OpenRead(path);
                        if (filter.reverse)
                        {
                            ret = new ReverseObjectIdStreamReader(s, id =>
                            {
                                var p = Localize(filter.ToPath(id));
                                if (System.IO.File.Exists(p))
                                {
                                    return System.IO.File.OpenRead(p);
                                }
                                else
                                {
                                    return null;
                                }
                            }, _serializer);
                        }
                        else
                        {
                            ret = new ObjectIdStreamReader(s, id => System.IO.File.OpenRead(Localize(filter.ToPath(id))), _serializer);
                        }
                    }
                    break;
                    case ".json":
                    ret = new ObjectStreamReader(System.IO.File.OpenRead(path), _serializer);
                    break;
                    default:
                    throw new Exception($"Invalid activity stream type: {path}");
                }
            }
            else
            {
                ret = new EmptyObjectStreamReader();
            }
            return Task.FromResult(ret);
        }

        // protected virtual string PathOf(PublicId id)
        // {
        //     var path = $"Upload/activitystreams";
        //     if (filter.peerId.HasValue)
        //     {
        //         path = $"{path}/peers/{filter.peerId.Value}";
        //     }

        //     if (filter.userId.HasValue)
        //     {
        //         path = $"{path}/users/{filter.userId.Value}";
        //     }

        //     if (string.IsNullOrWhiteSpace(filter.index))
        //     {
        //         throw new ArgumentNullException("filter.index");
        //     }
        //     else if (!IndexRegex.IsMatch(filter.index))
        //     {
        //         throw new Exception("Index contains invalid characters or is an invalid length");
        //     }

        //     if (id != null && id.IsGuid)
        //     {
        //         path = $"{path}/activities/{id.AsGuid()}.json";
        //     }
        //     else
        //     {
        //         path = $"{path}/indices/{filter.index}.index";
        //     }

        //     var parentFolder = System.IO.Directory.GetParent(path);
        //     if (!parentFolder.Exists)
        //     {
        //         parentFolder.Create();
        //     }

        //     return path;
        // }
    }
}