using Tides.Models.Auth.Types;
using Tides.Models.Crypto;

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