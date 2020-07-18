using Poncho.Models.Auth.Types;
using Poncho.Models.Crypto;

namespace PinkUmbrella.Models.Auth
{
    public class SiteKeyManager: BaseKeyManager
    {
        public async void Generate()
        {
            await Generate(new OpenPGPAuthHandler());
        }
    }
}