using System;

namespace Poncho.Util
{
    public class NameAttribute : Attribute
    {
        public string Name { get; set; }
        
        public NameAttribute(string name)
        {
            Name = name;
        }
    }
}