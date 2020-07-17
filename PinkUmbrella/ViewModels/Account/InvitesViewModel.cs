using System;
using System.Collections.Generic;
using PinkUmbrella.Models;

namespace PinkUmbrella.ViewModels.Account
{
    public class InvitesViewModel: BaseViewModel
    {
        public List<GroupAccessCodeModel> InvitesToMe { get; set; }
        
        public List<GroupAccessCodeModel> InvitesFromMe { get; set; }
        
        public string Selected { get; set; }
    }
}