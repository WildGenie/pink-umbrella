using System;

namespace PinkUmbrella.Util
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