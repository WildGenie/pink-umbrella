using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using PinkUmbrella.Util;
using Poncho.Util;

namespace PinkUmbrella.ViewModels.Doc
{
    public class DataViewModel: BaseViewModel
    {
        public class FieldModel
        {
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public string ReturnType { get; set; }
            public string ReturnTypeDisplayName { get; set; }
        }

        public class ClassModel
        {
            public ClassModel(Type t)
            {
                TypeName = t.AssemblyQualifiedName;
                Fields = new List<FieldModel>();
                DisplayName = ((DisplayNameAttribute) Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute)))?.DisplayName ?? t.Name;
                Description = ((DescriptionAttribute) Attribute.GetCustomAttribute(t, typeof(DescriptionAttribute)))?.Description ?? "no description";

                foreach (var p in t.GetProperties())
                {
                    var displayNameAttr = Attribute.IsDefined(p, typeof(DisplayNameAttribute)) ? Attribute.GetCustomAttribute(p, typeof(DisplayNameAttribute)) : null;
                    var descriptionAttr = Attribute.IsDefined(p, typeof(DescriptionAttribute)) ? Attribute.GetCustomAttribute(p, typeof(DescriptionAttribute)) as DescriptionAttribute : null;

                    if (descriptionAttr == null)
                    {
                        continue;
                    }

                    Fields.Add(new FieldModel()
                    {
                        DisplayName = (displayNameAttr as DisplayNameAttribute)?.DisplayName ?? p.Name,
                        Description = descriptionAttr.Description,
                        ReturnType = Attribute.IsDefined(p.PropertyType, typeof(IsDocumentedAttribute)) ? p.PropertyType.AssemblyQualifiedName : null,
                        ReturnTypeDisplayName = (Attribute.IsDefined(p.PropertyType, typeof(DisplayNameAttribute)) ? (Attribute.GetCustomAttribute(p.PropertyType, typeof(DisplayNameAttribute)) as DisplayNameAttribute) : null)?.DisplayName ?? p.PropertyType.Name,
                    });
                }
            }

            public string TypeName { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public List<FieldModel> Fields { get; set; }
        }

        public DataViewModel()
        {
        }

        public static readonly Dictionary<string, ClassModel> Models = AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(a => a.GetTypes())
                            .Where(t => Attribute.IsDefined(t, typeof(IsDocumentedAttribute)))
                            .Select(t => new ClassModel(t))
                            .ToDictionary(k => k.TypeName);
        public string Selected { get; set; }
    }
}