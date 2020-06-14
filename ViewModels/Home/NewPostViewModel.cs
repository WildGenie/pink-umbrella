using System.ComponentModel;
using seattle.Models;

namespace seattle.ViewModels.Home
{
    public class NewPostViewModel
    {
        public string[] Content { get; set; }

        [Description("Who on the internet can view your post")]
        public Visibility Visibility { get; set; } = Visibility.VISIBLE_TO_WORLD;
    }
}