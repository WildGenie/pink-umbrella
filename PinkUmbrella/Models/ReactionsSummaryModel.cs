using Tides.Core;
using Tides.Models;

namespace PinkUmbrella.Models
{
    public class ReactionsSummaryModel : IHazReactions
    {
        public int ToId { get; set; }

        public int LikeCount { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public int DislikeCount { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public int ReportCount { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public int BlockCount { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public int FollowCount { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        // NotMapped
        public bool HasLiked { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public bool HasDisliked { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public bool HasFollowed { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public bool HasBlocked { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public bool HasReported { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public bool HasBeenBlockedOrReportedByViewer { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public bool HasBeenBlockedOrReportedByPublisher { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public OrderedCollectionObject Reactions { get; set; }
    }
}