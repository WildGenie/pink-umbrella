using System.Text.RegularExpressions;

namespace seattle.Repositories
{
    public class StringRepository
    {
        public static string AllowedUserNameChars => "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        public string SiteName => "seattle.git";
        public string GitHubUrl => "https://github.com/viveret/seattle";
        public string WikiUrl => "https://github.com/viveret/seattle/wiki";
        public string ContributorsUrl => "https://github.com/viveret/seattle/graphs/contributors";
        public string CommunityDocUrl => "https://docs.google.com/document/d/13RdzrgGWLu21BfCVKf5BBSJgpk2cvhixYOZnRv1bITM/edit#";



        public Regex ExtractMentionsRegex { get; } = new Regex(@"@([a-zA-Z0-9_]+)");
    }
}