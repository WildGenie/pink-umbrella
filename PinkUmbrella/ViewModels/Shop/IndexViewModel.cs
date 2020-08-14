using System.Collections.Generic;
using Estuary.Core;
using PinkUmbrella.ViewModels.Shared;

namespace PinkUmbrella.ViewModels.Shop
{
    public class IndexViewModel : BaseViewModel
    {
        public Dictionary<int, BaseObject> ShopsByCategory { get; set; }
        
        public BaseObject Categories { get; set; }
        
        public BaseObject ShopsList { get; set; }
        
        public ListViewModel ListView { get; set; }
    }
}