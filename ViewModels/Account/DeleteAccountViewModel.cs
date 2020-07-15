using System.ComponentModel.DataAnnotations;
using PinkUmbrella.Models.Public;

namespace PinkUmbrella.ViewModels.Account
{
    public class DeleteAccountViewModel: BaseViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
}