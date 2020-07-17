using System;
using Poncho.Util;

namespace Poncho.Models.Auth
{
    public class AuthKeyFormatResolver
    {
        public static AuthKeyFormat? Resolve(string key)
        {
            if (RegexHelper.RSAKeyRegex.IsMatch(key))
            {
                return AuthKeyFormat.Raw;
            }
            else if (RegexHelper.OpenPGPKeyRegex.IsMatch(key))
            {
                return AuthKeyFormat.PEM;
            }
            else if (RegexHelper.Base64Regex.IsMatch(key))
            {
                return AuthKeyFormat.Msft;
            }

            return null;
        }
    }
}