using System.Collections.Generic;
using PinkUmbrella.Models;
using Tides.Core;

namespace PinkUmbrella.ViewModels.Shared
{
    public class TagEditorViewModel
    {
        public string HtmlId { get; set; }
        public string HtmlClasses { get; set; }
        public string Placeholder { get; set; }
        public string InputName { get; set; }
        public int MaxCount { get; set; }
        public List<TagModel> DebugValue { get; set; }
        public CollectionObject Tags { get; set; }
    }
}