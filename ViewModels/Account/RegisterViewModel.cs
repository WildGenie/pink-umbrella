using Microsoft.AspNetCore.Mvc;

namespace PinkUmbrella.ViewModels.Account
{
    public class RegisterViewModel: BaseViewModel
    {
        public string ReturnUrl { get; set; }

        [BindProperty]
        public RegisterInputModel Input { get; set; }
        
        public string Code { get; set; }
    }
}