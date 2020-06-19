using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PinkUmbrella.Models;

namespace PinkUmbrella.ViewModels.Shop
{
    public class NewShopViewModel : BaseViewModel
    {
        public ShopModel Shop { get; set; } = new ShopModel();
        public ShopModel Validate(ModelStateDictionary modelState)
        {
            return Shop;
        }
    }
}