using Estuary.Core;

namespace PinkUmbrella.Models
{
    public class UsedTagModel
    {
        public BaseObject Tag { get; set; }
        
        public long UseCount { get; set; }
    }
}