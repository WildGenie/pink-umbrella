using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PinkUmbrella.ViewModels.Account
{
    public class RegisterViewModel: BaseViewModel
    {
        public string ReturnUrl { get; set; }

        [BindProperty]
        public RegisterInputModel Input { get; set; }
    }
}