using PinkUmbrella.Models;

namespace PinkUmbrella.ViewModels.Account
{
    public class StatsViewModel: BaseViewModel
    {
        public ActorRateLimitModel Rates { get; set; }
        public ActorRateLimitModel Limits { get; set; }
    }
}