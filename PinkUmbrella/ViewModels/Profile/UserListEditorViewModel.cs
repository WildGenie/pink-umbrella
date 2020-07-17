using System.Collections.Generic;
using PinkUmbrella.Models;

namespace PinkUmbrella.ViewModels.Profile
{
    public class UserListEditorViewModel
    {
        public string HtmlId { get; set; }
        public string HtmlClasses { get; set; }
        public string Placeholder { get; set; }
        public string InputName { get; set; }
        public int MaxCount { get; set; }
        public List<UserProfileModel> Items { get; set; }
    }
}