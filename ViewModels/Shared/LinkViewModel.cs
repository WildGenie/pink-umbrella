using System.Collections.Generic;

namespace PinkUmbrella.ViewModels.Shared
{
    public class LinkViewModel
    {
        public bool Enabled { get; set; }

        public bool NewTab { get; set; }

        public string Text { get; set; }

        public string HtmlClass { get; set; }

        public string HtmlId { get; set; }

        public string Url { get; set; }

        public string Action { get; set; }

        public string Controller { get; set; }

        public Dictionary<string, string> RouteData { get; set; }

        public string RouteDataJson { get; set; }
    }
}