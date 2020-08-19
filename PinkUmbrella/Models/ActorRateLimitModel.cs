using System;
using Estuary.Util;
using Tides.Models.Public;

namespace PinkUmbrella.Models
{
    // todo: displaynames, descriptions
    public class ActorRateLimitModel: IHazComputedId
    {
        public PublicId UserId { get; set; }

        public int Like { get; set; }
        public int Dislike { get; set; }
        public int Upvote { get; set; }
        public int Downvote { get; set; }
        public int Ignore { get; set; }
        public int Block { get; set; }
        public int Flag { get; set; }
        public int Accept { get; set; }
        public int Reject { get; set; }




        public int Add { get; set; }
        public int Announce { get; set; }
        public int Arrive { get; set; }
        public int Create { get; set; }
        public int Delete { get; set; }
        public int Follow { get; set; }
        public int Invite { get; set; }
        public int Join { get; set; }
        public int Leave { get; set; }
        public int Listen { get; set; }
        public int Move { get; set; }
        public int Offer { get; set; }
        public int Question { get; set; }
        public int Read { get; set; }
        public int Remove { get; set; }
        public int Travel { get; set; }
        public int Update { get; set; }
        public int Undo { get; set; }
        public int View { get; set; }



        public int ApiCall { get; set; }

        public int UploadImages { get; set; }
        public int UploadVideos { get; set; }


        public string ComputedId => UserId?.ToString();

        public ActorRateLimitModel SetReactions(int v)
        {
            Like = v;
            Dislike = v;
            Upvote = v;
            Downvote = v;
            Ignore = v;
            Block = v;
            Flag = v;
            Accept = v;
            Reject = v;
            return this;
        }

        public ActorRateLimitModel SetActions(int v)
        {
            Add = v;
            Announce = v;
            Arrive = v;
            Create = v;
            Delete = v;
            Follow = v;
            Invite = v;
            Join = v;
            Leave = v;
            Listen = v;
            Move = v;
            Offer = v;
            Question = v;
            Read = v;
            Remove = v;
            Travel = v;
            Update = v;
            Undo = v;
            View = v;
            return this;
        }

        internal ActorRateLimitModel SetAll(int v)
        {
            SetActions(v);
            SetReactions(v);
            return this;
        }
    }
}