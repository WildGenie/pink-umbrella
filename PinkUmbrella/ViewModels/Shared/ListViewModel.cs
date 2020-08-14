using System;
using System.Linq;
using Estuary.Core;

namespace PinkUmbrella.ViewModels.Shared
{
    public class ListViewModel
    {
        public CollectionObject Items { get; set; }

        public BaseObject Selected { get; set; }

        public BaseObject Highlighted { get; set; }

        public string ItemViewName { get; set; }

        public string ItemTag { get; set; } = "div";

        public string EmptyViewName { get; set; }

        public object EmptyViewModel { get; set; }

        public string ContainerClasses => (Selected ?? Highlighted) != null ? "highlight-selected" : null;

        public string ContainerTag { get; set; } = "div";

        public string Title { get; set; }

        public string TitleClass { get; set; }

        public string TitleTag { get; set; } = "h1";
        public string SelectedId
        {
            get
            {
                return Selected?.id;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    Selected = null;
                }
                else
                {
                    Selected = Items?.items?.FirstOrDefault(i => i.id == value);
                }
            }
        }

        public static ListViewModel Regular(BaseObject baseObject)
        {
            return new ListViewModel
            {
                EmptyViewModel = "This list is empty",
                EmptyViewName = "Activity/ListEmpty",
                ItemViewName = "Activity/ListItemRegular",
                Items = baseObject as CollectionObject
            };
        }

        public static ListViewModel Links(BaseObject baseObject)
        {
            return new ListViewModel
            {
                ContainerTag = "ul",
                EmptyViewModel = "This list is empty",
                EmptyViewName = "Activity/ListEmpty",
                ItemViewName = "Activity/ListItemLink",
                ItemTag = "li",
                Items = baseObject as CollectionObject
            };
        }
    }
}