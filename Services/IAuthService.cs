using System.Net;
using System.Threading.Tasks;
using PinkUmbrella.Models.Auth;

namespace PinkUmbrella.Services
{
    public interface IAuthService
    {
        Task<IPAddressModel> GetOrRememberIP(IPAddress address);

        Task ForgetIPs();

        Task<AuthKey> GetOrAdd(AuthKey key);

        Task Trust(IPAddress address, AuthKey key);

        Task Untrust(AuthKey key);

        Task<bool> IsTrusted(IPAddress address, AuthKey key);
        
        Task<AuthKey> GetCurrent();
    }
}