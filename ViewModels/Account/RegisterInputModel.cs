using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PinkUmbrella.Util;

namespace PinkUmbrella.ViewModels.Account
{
    public class RegisterInputModel
    {
        [Required]
        [DataType(DataType.Text), Display(Name = "Display Name"), InputPlaceHolder("Your Name"), DebugValue("Test Account")]
        [Description("The name displayed to other users.")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        public string DisplayName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        [Description("We will send you an email to confirm your account and for account updates.")]
        [InputPlaceHolder("yourname@email.com"), DebugValue("test_acct@nowhere.com")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [Description("A password manager generated code is recommended.")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [Description("Re-enter your password to ensure you have entered it correctly")]
        public string ConfirmPassword { get; set; }

        [Required]
        [StringLength(30, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        [DataType(DataType.Text), Display(Name = "Handle"), InputPlaceHolder("your_name"), DebugValue("test_acct")]
        [Description("An identifiable handle to easily reference your profile")]
        [Remote("IsHandleUnique", "Account",  HttpMethod = "GET", ErrorMessage = "Handle already in use.")]
        [RegularExpression(@"[a-zA-Z0-9_]+")]
        public string Handle { get; set; }
    }
}