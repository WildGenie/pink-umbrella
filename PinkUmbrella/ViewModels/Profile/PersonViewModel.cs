using Estuary.Core;

namespace PinkUmbrella.ViewModels.Person
{
    public class PersonViewModel: Account.LocalAccountViewModel
    {
        public BaseObject Profile { get; set; }
    }
}