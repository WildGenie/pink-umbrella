using System;
using System.Collections.Generic;
using System.Linq;
using PinkUmbrella.ViewModels.Shared;

namespace PinkUmbrella.ViewModels
{
    public class NavigationViewModel: LinkViewModel
    {
        public NavigationViewModel()
        {
            this.Classes = "";
            HtmlClass += "nav-link text-dark text-decoration-underline";
        }

        public enum NavType
        {
            None = -1,
            Space = 0,
            Separator,
            Link,
        }
        
        public NavType Type { get; set; }

        public NavigationViewModel[] Nodes { get; set; }

        public bool Selected
        {
            get => Classes.Contains("selected");
            set
            {
                Classes = value ? string.Join(' ', Classes.Split(' ').Where(c => c != "selected")) : Classes + " selected";
            }
        }

        public string Icon { get; set; }

        public string Classes { get; set; }
    }
}