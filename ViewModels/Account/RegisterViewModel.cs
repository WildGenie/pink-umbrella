using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace seattle.ViewModels.Account
{
    public class RegisterViewModel
    {
        public string ReturnUrl { get; set; }

        [BindProperty]
        public RegisterInputModel Input { get; set; }
    }
}