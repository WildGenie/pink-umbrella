using System.Collections.Generic;
using System.Threading.Tasks;
using Tides.Core;

namespace Tides.Services
{
    public class ActivityStreamPipe: List<IActivityStreamPipe>, IHazActivityStreamPipe, IActivityStreamPipe
    {
        public ActivityStreamPipe()
        {
        }

        public ActivityStreamPipe(IEnumerable<IActivityStreamPipe> collection) : base(collection)
        {
        }

        public ActivityStreamPipe(int capacity) : base(capacity)
        {
        }

        public ActivityStreamPipe Get() => this;

        public async Task<BaseObject> Pipe(BaseObject obj, bool isReading)
        {
            if (isReading)
            {
                foreach (var handler in this)
                {
                    obj = await handler.Pipe(obj, isReading);
                    if (obj == null)
                    {
                        return null;
                    }
                }
            }
            else
            {
                for (int i = this.Count - 1; i >= 0; i--)
                {
                    obj = await this[i].Pipe(obj, isReading);
                    if (obj == null)
                    {
                        return null;
                    }
                }
            }
            return obj;
        }
    }
}