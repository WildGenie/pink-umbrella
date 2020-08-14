using Estuary.Actors;
using Estuary.Core;

namespace PinkUmbrella.ViewModels
{
    public class BaseViewModel
    {
        public ActorObject MyProfile { get; set; }

        public BaseObject Item { get; set; }

        public NavigationViewModel RootNav { get; set; }

        public NavigationViewModel ItemNav { get; set; }
    }
}