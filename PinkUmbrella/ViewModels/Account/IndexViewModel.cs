using System;
using System.Collections.Generic;
using PinkUmbrella.Models.Auth;

namespace PinkUmbrella.ViewModels.Account
{
    public class IndexViewModel: LocalAccountViewModel
    {
        public Dictionary<UserLoginMethod, UserLoginMethodModel> MethodOverrides { get; set; } = new Dictionary<UserLoginMethod, UserLoginMethodModel>();

        public Func<UserLoginMethod, bool> GetMethodDefault { get; set; }
    }
}