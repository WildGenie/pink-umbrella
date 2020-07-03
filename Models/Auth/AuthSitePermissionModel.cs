using PinkUmbrella.Models.Auth.Permissions;

namespace PinkUmbrella.Models.Auth
{
    public class AuthSitePermissionModel
    {
        public long Id { get; set; }
        public long AuthId { get; set; }
        public SitePermission Permission { get; set; }
        public bool OverrideValue { get; set; }
    }
}