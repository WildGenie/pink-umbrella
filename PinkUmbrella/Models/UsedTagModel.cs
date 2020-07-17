namespace PinkUmbrella.Models
{
    public class UsedTagModel
    {
        public TagModel Tag { get; set; }
        
        public long UseCount { get; set; }
    }
}