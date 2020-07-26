using System;
using System.Threading.Tasks;
using Tides.Core;

namespace Tides.Streams
{
    public abstract class BaseObjectStreamWriter: BaseObjectStream
    {
        public abstract Task Write(BaseObject item);

        public string NameOf(Type type)
        {
            if (type.Name.EndsWith("Object"))
            {
                return type.Name.Substring("Object".Length + 1);
            }
            else
            {
                return type.Name;
            }
        }
    }
}