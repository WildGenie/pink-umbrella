using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PinkUmbrella.Models;
using PinkUmbrella.Services;
using PinkUmbrella.Util;
using Tides.Actors;
using Tides.Core;
using Tides.Models;
using static PinkUmbrella.Controllers.ShopController;
using static Tides.Objects.Common;

namespace PinkUmbrella.ViewModels.Shop
{
    public class NewShopViewModel : BaseViewModel
    {
        [DefaultValue(Visibility.VISIBLE_TO_WORLD), PersonalData, Description("Visibility of your shop to other users.")]
        public Visibility Visibility { get; set; } = Visibility.VISIBLE_TO_WORLD;
        
        [Required, StringLength(100), PersonalData, Description("An identifiable handle to easily reference your shop."), InputPlaceHolder("my_shop"), DebugValue("planet_express")]
        [Remote("IsHandleUnique", "Shop",  HttpMethod = "GET", ErrorMessage = "Handle already in use.")]
        public string Handle { get; set; }

        [Required, StringLength(200), PersonalData, DisplayName("Display Name"), Description("The name of your shop displayed to other users."), InputPlaceHolder("My Shop"), DebugValue("Planet Express")]
        public string DisplayName { get; set; }

        [Required, StringLength(1000), PersonalData, Description("A description of your shop - what makes it special or unique."), InputPlaceHolder("We offer indoor and outdoor goods, sundries, gifts, and really anything you need."), DebugValue("We ship to anywhere in the multiverse")]
        public string Description { get; set; }

        [Required, StringLength(300), PersonalData, DisplayName("Street Address"), Description("The street address of your shop within the city."), InputPlaceHolder("305 Harrison St"), DebugValue("305 Harrison St")]
        public string StreetAddress { get; set; }

        [Required, StringLength(20), PersonalData, DisplayName("Zip Code"), Description("The zip code of your shop within the city."), InputPlaceHolder("98109"), DebugValue("98109")]
        public string ZipCode { get; set; }

        [Required, StringLength(300), PersonalData, DisplayName("Website Link"), Description("Link to the website for your shop, if you have one."), InputPlaceHolder("https://yourwebsite.com"), DebugValue("https://www.google.com")]
        public string WebsiteLink { get; set; }

        [Required, StringLength(300), PersonalData, DisplayName("Menu Link"), Description("Link to a menu for your shop, if applicable."), InputPlaceHolder("https://yourwebsite.com/menu"), DebugValue("https://www.youtube.com")]
        public string MenuLink { get; set; }

        [NotMapped, Description("Make your business easier for other users to find."), Nest.Ignore]
        public CollectionObject Tags { get; set; } = new CollectionObject();
        
        // [NotMapped, JsonPropertyName("tags"), Nest.PropertyName("tags")]
        // public string[] TagStrings
        // {
        //     get
        //     {
        //         return Tags.Select(t => t.Tag).ToArray();
        //     }
        //     set
        //     {
        //         Tags = value.Select(t => new TagModel() { Tag = t }).ToList();
        //     }
        // }
        public string tagsJson { get; set; }

        public async Task<ActorObject> Validate(ModelStateDictionary modelState, int userId, ITagService _tags)
        {
            var tags = new List<BaseObject>();
            using (var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(tagsJson)))
            {
                var tagsIn = await System.Text.Json.JsonSerializer.DeserializeAsync<List<SimpleTag>>(ms);
                foreach (var tag in tagsIn)
                {
                    var tm = new Note()
                    {
                        content = tag.label,
                        objectId = tag.value,
                    };
                    Tags.Add(tm);
                    var newTag = await _tags.TryGetOrCreateTag(tm, userId);
                    if (newTag != null)
                    {
                        tags.Add(newTag);
                    }
                    else
                    {
                        modelState.AddModelError(nameof(Tags), $"Tag invalid: {tm.content}");
                    }
                }
            }
            
            if (modelState.ErrorCount == 0)
            {
                return new Common.Organization
                {
                    // tag = Tags.Select(t => new BaseObject
                    // {
                    //     content = t.Tag
                    // }).ToList()
                };
            }
            else
            {
                return null;
            }
        }
    }
}