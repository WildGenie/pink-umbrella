using System.Collections.Generic;
using Tides.Core;

namespace PinkUmbrella.ViewModels.Shop
{
    public class IndexViewModel : BaseViewModel
    {
        public Dictionary<int, CollectionObject> ShopsByCategory { get; set; }
        
        public CollectionObject Categories { get; set; }
        
        public CollectionObject ShopsList { get; set; }
    }
}