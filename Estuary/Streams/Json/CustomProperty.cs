using System.Reflection;

namespace Estuary.Streams.Json
{
    public class CustomProperty
    {
        public string Name { get; set; }
        public PropertyInfo Property { get; set; }
        public object Value { get; set; }
    }
}