using System.Collections.Generic;
using PinkUmbrella.Models;

namespace PinkUmbrella.ViewModels.Admin
{
    public class IndexViewModel : BaseViewModel
    {
        public List<GroupAccessCodeModel> UnusedUnexpiredAccessCodes { get; internal set; }
    }
}