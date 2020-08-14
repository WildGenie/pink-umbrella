using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Estuary.Core;
using Tides.Models;
using Tides.Util;

namespace Estuary.Actors
{
    [IsDocumented]
    public class ActorObject: BaseObject
    {
        public ActorObject(string type) : base(type ?? throw new ArgumentNullException(nameof(type)), "Actor") { }

        public OrderedCollectionObject inbox { get; set; }
        public OrderedCollectionObject outbox { get; set; }
        public OrderedCollectionObject followers { get; set; }
        public OrderedCollectionObject following { get; set; }
        public OrderedCollectionObject liked { get; set; }
        public OrderedCollectionObject disliked { get; set; }
        public OrderedCollectionObject sharedInbox { get; set; }
        public OrderedCollectionObject sharedOutbox { get; set; }
        public Dictionary<string, OrderedCollectionObject> streams { get; set; }


        [JsonPropertyName("handle")]
        public string Handle { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        // [JsonPropertyName("bioVisibility")]
        // [DisplayName("Bio Visibility"), Description("Visibility of your bio to other users.")]
        // public Visibility BioVisibility { get; set; }

        // [JsonPropertyName("whenCreated")]
        // [DisplayName("When Joined"), Description("The exact time and date when you joined.")]
        // public DateTime WhenCreated { get; set; }

        [JsonPropertyName("whenLastLoggedIn")]
        [DisplayName("When Last Logged In"), Description("The exact time and date when you last logged in.")]
        public DateTime WhenLastLoggedIn { get; set; }

        // [JsonPropertyName("whenLastLoggedInVisibility")]
        // [DisplayName("When Last Logged In Visibility"), Description("Visibility of when you last logged in to other users.")]
        // public Visibility WhenLastLoggedInVisibility { get; set; }

        [JsonPropertyName("whenLastOnline")]
        [DisplayName("When Last Online"), Description("The exact time and date when you last connected to the site.")]
        public DateTime WhenLastOnline { get; set; }

        // [JsonPropertyName("whenLastOnlineVisibility")]
        // [DisplayName("When Last Online Visibility"), Description("Visibility of when you last connected to other users.")]
        // public Visibility WhenLastOnlineVisibility { get; set; }

        [JsonPropertyName("banExpires")]
        [DisplayName("You Are Allowed Back On"), Description("When your ban expires.")]
        public DateTime? BanExpires { get; set; }

        [JsonPropertyName("banReason")]
        [DisplayName("Ban Reason"), StringLength(1000), Description("The reasoning behind your ban.")]
        public string BanReason { get; set; }

        public string preferredUsername => Handle;


        [JsonIgnore, NotMapped, Nest.Ignore]
        public object PrivateModel { get; set; }
    }
}