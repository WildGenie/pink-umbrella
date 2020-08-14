using System;
using System.Collections.Generic;

namespace PinkUmbrella.ViewModels.Shared
{
    public class LinkViewModel
    {
        public bool NewTab { get; set; }

        public string Text { get; set; }

        public string HtmlClass { get; set; }

        public string HtmlId { get; set; }

        public string Url { get; set; }

        public string Action { get; set; }

        public string Controller { get; set; }

        public Func<BaseViewModel, Dictionary<string, string>> GetRouteData { get; set; }
    }
}