using System.Text.RegularExpressions;

namespace PinkUmbrella.Repositories
{
    public class StringRepository
    {
        public static string AllowedUserNameChars => "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        public string SiteName => "Pink Umbrella";
        public string GitHubUrl => "https://github.com/viveret/pink-umbrella";
        public string WikiUrl => "https://github.com/viveret/pink-umbrella/wiki";
        public string ContributorsUrl => "https://github.com/viveret/pink-umbrella/graphs/contributors";
        public string CommunityDocUrl => "https://docs.google.com/document/d/13RdzrgGWLu21BfCVKf5BBSJgpk2cvhixYOZnRv1bITM/edit#";



        public Regex ExtractMentionsRegex { get; } = new Regex(@"@([a-zA-Z0-9_]+)");

        public string StatusCodeMessage(string code)
        {
            switch (code)
            {
                case "400": return "The request was invalid.";
                case "401": return "The request was not authorized.";
                case "403": return "That request is forbidden.";
                case "404": return "The requested page does not exist.";
                case "405": return "The method of the request is not allowed.";
                case "408": return "The requested page timed out.";
                case "418": return "The requested page is a tea pot.";

                case "500": return "The requested page caused an internal error.";
                case "501": return "The requested page is not finished.";
                case "502": return "Bad gateway.";

                default: return "No message for " + code + ".";
            }
        }
    }
}