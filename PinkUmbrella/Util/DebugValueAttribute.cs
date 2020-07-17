using System;

namespace PinkUmbrella.Util
{
    public class DebugValueAttribute : Attribute
    {
        public string Value { get; set; }
        
        public DebugValueAttribute(string value)
        {
            Value = value;
        }
    }
}