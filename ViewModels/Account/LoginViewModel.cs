using System.ComponentModel.DataAnnotations;
using PinkUmbrella.Models;

namespace PinkUmbrella.ViewModels.Account
{
    public class LoginViewModel
    {
        public UserProfileModel User { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string EmailAddress { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}