using System;

namespace Estuary.Util
{
    public class RedisValueAttribute : Attribute
    {
        public bool IsRequired { get; }

        public RedisValueAttribute(bool isRequired = false)
        {
            IsRequired = isRequired;
        }
    }
}