using System;
using System.IO;
using System.Threading.Tasks;
using Estuary.Core;
using Estuary.Services;

namespace Estuary.Streams
{
    public class FilteredObjectIdStreamWriter : ObjectIdStreamWriter
    {
        private readonly ActivityStreamFilter _filter;

        public FilteredObjectIdStreamWriter(ActivityStreamFilter filter, Stream stream, IActivityStreamRepository ctx) : base(stream, ctx)
        {
            _filter = filter ?? throw new ArgumentNullException(nameof(filter));
        }

        public override Task Write(BaseObject item)
        {
            if (_filter.IsMatch(item))
            {
                return base.Write(item);
            }
            else
            {
                return Task.CompletedTask;
            }
        }
    }
}