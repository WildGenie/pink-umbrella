using System;
using System.Collections.Generic;
using PinkUmbrella.Models.Auth;

namespace PinkUmbrella.ViewModels.Account
{
    public class SetupLoginMethodViewModel: BaseViewModel
    {
        public UserLoginMethod Method { get; set; }
        
        public Func<UserLoginMethod, bool> GetMethodDefault { get; set; }
        
        public bool Allowed { get; set; }
        
        public bool AllowedByDefault { get; set; }

        public string ReturnUrl { get; set; }
        
        public object LoginModel { get; set; }
    }
}