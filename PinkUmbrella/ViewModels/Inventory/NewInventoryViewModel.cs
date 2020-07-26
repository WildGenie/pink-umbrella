using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using PinkUmbrella.Util;

namespace PinkUmbrella.ViewModels.Inventory
{
    public class NewInventoryViewModel : BaseViewModel
    {
        public long PeerId { get; set; }
        public int? Id { get; set; }
        public int OwnerUserId { get; set; }

        [Required, StringLength(100), DisplayName("Display Name"), Description("What is the inventory called? This can be shown to other users."), InputPlaceHolder("My Backpack"), DebugValue("My Backpack")]
        public string DisplayName { get; set; }

        [Required, StringLength(1000), Description("How big is this inventory? How much can it hold?"), InputPlaceHolder("3 pockets and 2 water bottle holders. Can carry up to 50 lbs."), DebugValue("3 pockets and 2 water bottle holders. Can carry up to 50 lbs.")]
        public string Description { get; set; }
    }
}