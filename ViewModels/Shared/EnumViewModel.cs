using System;

namespace seattle.ViewModels.Shared
{
    public class EnumViewModel
    {
        public bool Enabled { get; set; }
        public Type Type { get; set; }
        public object Selected { get; set; }
    }
}