using System;

namespace PinkUmbrella.Util
{
    public class InputPlaceHolderAttribute: Attribute
    {
        public string Text { get; set; }
        
        public InputPlaceHolderAttribute(string text)
        {
            Text = text;
        }
    }
}