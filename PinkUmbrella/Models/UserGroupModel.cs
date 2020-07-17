using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using PinkUmbrella.Util;
using Poncho.Models;
using Poncho.Util;

namespace PinkUmbrella.Models
{
    [IsDocumented]
    public class UserGroupModel : IdentityRole<int>
    {
        public string Description { get; set; }

        public int OwnerId { get; set; }

        public Visibility Visibility { get; set; }

        public GroupType GroupType { get; set; }

        public UserGroupModel() { }

        public UserGroupModel(string name)
        {
            Name = name;
        }
    }
}