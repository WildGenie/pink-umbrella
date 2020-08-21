using System;
using Estuary.Util;
using Tides.Models.Public;

namespace PinkUmbrella.Models
{
    // todo: displaynames, descriptions
    public class ActorRateLimitModel: IHazComputedId
    {
        public PublicId UserId { get; set; }

        [RedisValue]
        public int Like { get; set; }

        [RedisValue]
        public int Dislike { get; set; }

        [RedisValue]
        public int Upvote { get; set; }

        [RedisValue]
        public int Downvote { get; set; }

        [RedisValue]
        public int Ignore { get; set; }

        [RedisValue]
        public int Block { get; set; }

        [RedisValue]
        public int Flag { get; set; }

        [RedisValue]
        public int Accept { get; set; }

        [RedisValue]
        public int Reject { get; set; }




        [RedisValue]
        public int Add { get; set; }

        [RedisValue]
        public int Announce { get; set; }

        [RedisValue]
        public int Arrive { get; set; }

        [RedisValue]
        public int Create { get; set; }

        [RedisValue]
        public int Delete { get; set; }

        [RedisValue]
        public int Follow { get; set; }

        [RedisValue]
        public int Invite { get; set; }

        [RedisValue]
        public int Join { get; set; }

        [RedisValue]
        public int Leave { get; set; }

        [RedisValue]
        public int Listen { get; set; }

        [RedisValue]
        public int Move { get; set; }

        [RedisValue]
        public int Offer { get; set; }

        [RedisValue]
        public int Question { get; set; }

        [RedisValue]
        public int Read { get; set; }

        [RedisValue]
        public int Remove { get; set; }

        [RedisValue]
        public int Travel { get; set; }

        [RedisValue]
        public int Update { get; set; }

        [RedisValue]
        public int Undo { get; set; }

        [RedisValue]
        public int View { get; set; }



        [RedisValue]
        public int ApiCall { get; set; }

        [RedisValue]
        public int UploadImages { get; set; }

        [RedisValue]
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