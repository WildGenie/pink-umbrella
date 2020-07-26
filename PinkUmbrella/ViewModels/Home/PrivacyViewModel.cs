using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using PinkUmbrella.Models;

namespace PinkUmbrella.ViewModels.Home
{
    public class PrivacyViewModel: BaseViewModel
    {
        public class PrivacyField
        {
            public string DisplayName { get; set; }
            public string Description { get; set; }
        }

        public class PrivacyModel
        {
            public PrivacyModel(Type t)
            {
                Fields = new List<PrivacyField>();
                DisplayName = ((DisplayNameAttribute) Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute))).DisplayName;
                Description = ((DescriptionAttribute) Attribute.GetCustomAttribute(t, typeof(DescriptionAttribute))).Description;

                var personalDataProps = t.GetProperties().Where(
                                prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
                foreach (var p in personalDataProps)
                {
                    var displayNameAttr = Attribute.IsDefined(p, typeof(DisplayNameAttribute)) ? Attribute.GetCustomAttribute(p, typeof(DisplayNameAttribute)) : null;
                    var descriptionAttr = Attribute.IsDefined(p, typeof(DescriptionAttribute)) ? Attribute.GetCustomAttribute(p, typeof(DescriptionAttribute)) as DescriptionAttribute : null;

                    if (descriptionAttr == null) {
                        continue;
                    }

                    Fields.Add(new PrivacyField() {
                        DisplayName = (displayNameAttr as DisplayNameAttribute)?.DisplayName ?? p.Name,
                        Description = descriptionAttr.Description,
                    });
                }
            }

            public string DisplayName { get; set; }
            public string Description { get; set; }
            public List<PrivacyField> Fields { get; set; }
        }

        public PrivacyViewModel() {
            Models = new List<PrivacyModel>() {
                new PrivacyModel(typeof(UserProfileModel)),
            };
        }

        public List<PrivacyModel> Models { get; set; }
    }
}