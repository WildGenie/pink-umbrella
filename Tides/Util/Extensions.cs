using System;

namespace Tides.Util
{
    public static class Extensions
    {
        public static string EscapeUrl(this string s) => Uri.EscapeUriString(s);
    }
}