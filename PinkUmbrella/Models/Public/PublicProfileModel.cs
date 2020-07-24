using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using Tides.Models;
using Tides.Models.Public;

namespace PinkUmbrella.Models.Public
{
    public class PublicProfileModel: IHazPublicId
    {
        public PublicProfileModel()
        {
        }

        public PublicProfileModel(UserProfileModel user, long peerId): base()
        {
            this.UserId = (user ?? throw new ArgumentNullException(nameof(user))).Id;
            if (peerId > 0)
            {
                throw new NotSupportedException();
            }
            if (user.Id > 0)
            {
                this.PeerId = peerId;
                this.SetIdFromCompound();
            }
            
            this.BanExpires = user.BanExpires;
            this.BanReason = user.BanReason;
            this.Bio = user.Bio;
            this.BioVisibility = user.BioVisibility;
            this.DisplayName = user.DisplayName;
            this.Email = user.Email;
            // this.EmailVisibility = user.EmailVisibility;
            this.Handle = user.Handle;
            this.Visibility = user.Visibility;
            this.WhenCreated = user.WhenCreated;
            this.WhenLastLoggedIn = user.WhenLastLoggedIn;
            this.WhenLastLoggedInVisibility = user.WhenLastLoggedInVisibility;
            this.WhenLastOnline = user.WhenLastOnline;
            this.WhenLastOnlineVisibility = user.WhenLastOnlineVisibility;
            this.WhenLastUpdated = user.WhenLastUpdated;

            this.Local = user;
        }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("authId")]
        public long AuthId { get; set; }

        [JsonPropertyName("peerId")]
        public long PeerId { get; set; }

        [JsonPropertyName("userId")]
        public int UserId { get; set; }

        [JsonPropertyName("handle")]
        public string Handle { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }
        
        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        [JsonPropertyName("bio")]
        public string Bio { get; set; }

        [JsonPropertyName("visibility")]
        public Visibility Visibility { get; set; }

        [JsonPropertyName("bioVisibility")]
        [DisplayName("Bio Visibility"), Description("Visibility of your bio to other users.")]
        public Visibility BioVisibility { get; set; }

        [JsonPropertyName("whenCreated")]
        [DisplayName("When Joined"), Description("The exact time and date when you joined.")]
        public DateTime WhenCreated { get; set; }

        [JsonPropertyName("whenUpdated")]
        [DisplayName("WhenLastUpdated"), Description("When your profile was last updated.")]
        public DateTime WhenLastUpdated { get; set; }

        [JsonPropertyName("whenLastLoggedIn")]
        [DisplayName("When Last Logged In"), Description("The exact time and date when you last logged in.")]
        public DateTime WhenLastLoggedIn { get; set; }

        [JsonPropertyName("whenLastLoggedInVisibility")]
        [DisplayName("When Last Logged In Visibility"), Description("Visibility of when you last logged in to other users.")]
        public Visibility WhenLastLoggedInVisibility { get; set; }

        [JsonPropertyName("whenLastOnline")]
        [DisplayName("When Last Online"), Description("The exact time and date when you last connected to the site.")]
        public DateTime WhenLastOnline { get; set; }

        [JsonPropertyName("whenLastOnlineVisibility")]
        [DisplayName("When Last Online Visibility"), Description("Visibility of when you last connected to other users.")]
        public Visibility WhenLastOnlineVisibility { get; set; }

        [JsonPropertyName("banExpires")]
        [DisplayName("You Are Allowed Back On"), Description("When your ban expires.")]
        public DateTime? BanExpires { get; set; }

        [JsonPropertyName("banReason")]
        [DisplayName("Ban Reason"), StringLength(1000), Description("The reasoning behind your ban.")]
        public string BanReason { get; set; }


        [NotMapped, JsonIgnore, Nest.Ignore]
        public UserProfileModel Local { get; set; }

        public void SetIdFromCompound()
        {
            if (PeerId > 0)
            {
                Id = $"{PeerId}-{UserId}";
            }
            else
            {
                Id = UserId.ToString();
            }
        }

        public void SetCompoundIdFromId()
        {
            var split = Id.Split('-');
            if (split.Length == 2)
            {
                PeerId = long.Parse(split[0]);
                UserId = int.Parse(split[1]);
            }
            else if (split.Length == 1)
            {
                PeerId = 0;
                UserId = int.Parse(split[0]);
            }
            else
            {
                throw new Exception();
            }
        }


        
        
        [NotMapped, JsonIgnore, Nest.Ignore]
        public List<ReactionModel> Reactions { get; set; } = new List<ReactionModel>();

        [NotMapped, JsonIgnore, Nest.Ignore]
        public List<TagModel> Tags { get; set; } = new List<TagModel>();

        [NotMapped, JsonPropertyName("tags"), Nest.PropertyName("tags")]
        public string[] TagStrings
        {
            get
            {
                return Tags.Select(t => t.Tag).ToArray();
            }
            set
            {
                Tags = value.Select(t => new TagModel() { Tag = t }).ToList();
            }
        }

        [NotMapped, Nest.Ignore]
        public List<int> FollowerIds { get; set; }


        [NotMapped, Nest.Ignore]
        public bool HasLiked { get; set; }
        
        [NotMapped, Nest.Ignore]
        public bool HasDisliked { get; set; }

        [NotMapped, Nest.Ignore]
        public bool HasFollowed { get; set; }
        
        [NotMapped, Nest.Ignore]
        public bool HasBlocked { get; set; }

        [NotMapped, Nest.Ignore]
        public bool HasReported { get; set; }

        [NotMapped, Nest.Ignore]
        public bool HasBeenBlockedOrReported { get; set; }

        [NotMapped, Nest.Ignore]
        public PublicId PublicId => new PublicId(Id);
    }
}