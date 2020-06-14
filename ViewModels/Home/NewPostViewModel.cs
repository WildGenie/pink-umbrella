using System.ComponentModel;
using PinkUmbrella.Models;

namespace PinkUmbrella.ViewModels.Home
{
    public class NewPostViewModel
    {
        public string[] Content { get; set; }

        [Description("Who on the internet can view your post")]
        public Visibility Visibility { get; set; } = Visibility.VISIBLE_TO_WORLD;
    }
}