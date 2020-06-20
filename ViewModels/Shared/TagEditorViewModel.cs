using System.Collections.Generic;
using PinkUmbrella.Models;

namespace PinkUmbrella.ViewModels.Shared
{
    public class TagEditorViewModel
    {
        public string HtmlId { get; set; }
        public string HtmlClasses { get; set; }
        public string Placeholder { get; set; }
        public string InputName { get; set; }
        public List<TagModel> DebugValue { get; set; }
        public List<TagModel> Tags { get; set; }
    }
}