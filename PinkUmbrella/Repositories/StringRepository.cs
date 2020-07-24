using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using PinkUmbrella.Models;
using Tides.Models;

namespace PinkUmbrella.Repositories
{
    public class StringRepository
    {
        public class ViewKeyStrings 
        {
            public string StatusBar => "StatusBar";
        }

        public ViewKeyStrings ViewKeys { get; } = new ViewKeyStrings();

        public string GitHubUrl => "https://github.com/pink-umbrella/pink-umbrella";
        public string WikiUrl => "https://github.com/pink-umbrella/pink-umbrella/wiki";
        public string ReportBugUrl => "https://github.com/pink-umbrella/pink-umbrella/issues/new";
        public string ContributorsUrl => "https://github.com/pink-umbrella/pink-umbrella/graphs/contributors";
        public string CommunityDocUrl => "https://docs.google.com/document/d/13RdzrgGWLu21BfCVKf5BBSJgpk2cvhixYOZnRv1bITM/edit#";
        public string SshKeyGenUrl => "https://linux.die.net/man/1/ssh-keygen";
        public string OpenSSLKeyGenUrl => "https://wiki.openssl.org/index.php/Command_Line_Utilities";
        public string MarkdownUrl => "https://guides.github.com/features/mastering-markdown/";

        public string RoleDeveloper => "dev";

        public string RoleAdmin => "admin";


        public string BetaMsg => "This is the beta version.";

        public string InDevelopmentMsg => "This software is still being developed and may change or break.";

        public string PasswordPlaceholder => "***************";

        public string UrlPlaceholder => "https://example.com";

        public string IPPlaceholder => "123.255.123.255";

        public string PortPlaceholder => "80 (Default is 443, https)";

        public static string AllowedUserNameChars => "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";


        public string RecoveryCodeChars => "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        
        public string SoftwareName => "Pink Umbrella";

        private readonly string[] Positives = { "yes", "on", "true", "ok", "accept" };
        private readonly string[] Negatives = { "no", "off", "false", "deny" };

        public Regex ExtractMentionsRegex { get; } = new Regex(@"@([a-zA-Z0-9_]+)");

        public Regex ExtractTagsRegex { get; } = new Regex(@"#([a-zA-Z0-9_]+)");

        public Regex ValidHandleRegex { get; } = new Regex(@"[a-zA-Z0-9_]+");

        public Regex ValidIPRegex { get; } = new Regex(@"((^\s*((([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5]))\s*$)|(^\s*((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:)))(%.+)?\s*$))");

        public bool ValidEmail(string email) => EmailValidator.IsValid(email);

        private readonly EmailAddressAttribute EmailValidator = new EmailAddressAttribute();

        public string StatusCodeMessage(string code)
        {
            switch (code)
            {
                case "400": return "The request was invalid.";
                case "401": return "The request was not authorized.";
                case "403": return "The request was not authorized.";
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
        public string Number(long count) => $"{count:N0}";

        public string CountRound(long count)
        {
            var abs = Math.Abs(count);
            if (abs < 999)
            {
                return $"{count}";
            }
            else if (abs < 9999)
            {
                return $"{count:N0}";
            }
            else if (abs < 999999)
            {
                return $"{count / 1000:N0}K";
            }
            else if (abs < 999999999)
            {
                return $"{count / 1000000:N0}M";
            }
            else if (abs < 999999999999)
            {
                return $"{count / 1000000000:N0}B";
            }
            else if (abs < 999999999999999)
            {
                return $"{count / 1000000000000:N0}T";
            }
            else
            {
                return $"{count / 1000000000000000:N0}Q";
            }
        }

        public string FileSize(long bytes)
        {
            var power = Math.Log10(bytes);
            if (power < 3)
            {
                return $"{bytes} Bytes";
            }
            else if (power >= 3 && power < 6)
            {
                return $"{Math.Round(bytes / 1000.0)} KB";
            }
            else if (power >= 6 && power < 9)
            {
                return $"{Math.Round(bytes / 1000000.0)} MB";
            }
            else if (power >= 9 && power < 12)
            {
                return $"{Math.Round(bytes / 1000000000.0)} GB";
            }
            else if (power >= 12 && power < 15)
            {
                return $"{Math.Round(bytes / 1000000000000.0)} TB";
            }
            else if (bytes == long.MaxValue)
            {
                return $"\u221E";
            }
            else
            {
                return $"File wayyy too big";
            }
        }

        public string format(DateTime timestamp)
        {
            if (timestamp.Ticks == 0)
            {
                return "Never";
            }

            return $"{timestamp.Month}/{timestamp.Day}/{timestamp.Year}, {timestamp.Hour}:{timestamp.Minute}:{timestamp.Second}";
        }

        public string TimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.TotalSeconds < 5)
            {
                return "Less than a second";
            }
            else if (timeSpan.TotalSeconds >= 5 && timeSpan.TotalSeconds < 10)
            {
                return $"A few seconds";
            }
            else if (timeSpan.TotalSeconds >= 10 && timeSpan.TotalSeconds < 60)
            {
                return $"Less than a minute";
            }
            else if (timeSpan.TotalMinutes >= 1 && timeSpan.TotalMinutes < 2)
            {
                return $"A minute";
            }
            else if (timeSpan.TotalMinutes < 5)
            {
                return "A few minutes";
            }
            else if (timeSpan.TotalMinutes >= 5 && timeSpan.TotalMinutes < 60)
            {
                return $"{Math.Round(timeSpan.TotalMinutes)} minutes";
            }
            else if (timeSpan.TotalHours >= 1 && timeSpan.TotalHours < 24)
            {
                return $"{Math.Round(timeSpan.TotalHours)} hours";
            }
            else if (timeSpan.TotalDays > 1 && timeSpan.TotalDays < 32)
            {
                return $"{Math.Round(timeSpan.TotalDays)} days";
            }
            else if (timeSpan.TotalDays < 7 * 30)
            {
                return $"{Math.Round(timeSpan.TotalDays / 7)} weeks";
            }
            else if (timeSpan.TotalDays < 365)
            {
                return $"{Math.Round(timeSpan.TotalDays / 30)} months";
            }
            else
            {
                return $"{Math.Round(timeSpan.TotalDays / 365.25)} years";
            }
        }

        public string Relative(DateTime when)
        {
            if (when.Ticks == 0)
            {
                return "Never";
            }

            var now = DateTime.Now;
            if (when < now)
            {
                var past = now.Subtract(when);
                if (past.TotalSeconds < 5)
                {
                    return "Less than a second ago"; // Is this lying?
                }
                else if (past.TotalSeconds >= 5 && past.TotalSeconds < 10)
                {
                    return $"A few seconds ago";
                }
                else if (past.TotalSeconds >= 10 && past.TotalSeconds < 60)
                {
                    return $"Less than a minute ago";
                }
                else if (past.TotalMinutes >= 1 && past.TotalMinutes < 2)
                {
                    return $"A minute ago";
                }
                else if (past.TotalMinutes < 5)
                {
                    return "A few minutes ago";
                }
                else if (past.TotalMinutes >= 5 && past.TotalMinutes < 60)
                {
                    return $"{Math.Round(past.TotalMinutes)} minutes ago";
                }
                else if (past.TotalHours >= 1 && past.TotalHours < 24)
                {
                    return $"{Math.Round(past.TotalHours)} hours ago";
                }
                else if (past.TotalDays >= 1 && past.TotalDays < 2)
                {
                    return $"Yesterday";
                }
                else if (past.TotalDays > 1 && past.TotalDays < 32)
                {
                    return $"{Math.Round(past.TotalDays)} days ago";
                }
                else if (past.TotalDays < 7 * 30)
                {
                    return $"{Math.Round(past.TotalDays / 7)} weeks ago";
                }
                else if (past.TotalDays < 365)
                {
                    return $"{Math.Round(past.TotalDays / 30)} months ago";
                }
                else
                {
                    return $"{now.Year - when.Year} years ago";
                }
            }
            else
            {
                var future = when.Subtract(now);
                if (future.TotalSeconds < 5)
                {
                    return "In less than a second"; // Is this lying?
                }
                else if (future.TotalSeconds >= 5 && future.TotalSeconds < 10)
                {
                    return $"In a few seconds";
                }
                else if (future.TotalSeconds >= 10 && future.TotalSeconds < 60)
                {
                    return $"In {Math.Round(future.TotalSeconds)} seconds";
                }
                else if (future.TotalMinutes < 5)
                {
                    return "In a few minutes";
                }
                else if (future.TotalMinutes >= 5 && future.TotalMinutes < 60)
                {
                    return $"In {Math.Round(future.TotalMinutes)} minutes";
                }
                else if (future.TotalHours >= 1 && future.TotalHours < 24)
                {
                    return $"In {Math.Round(future.TotalHours)} hours";
                }
                else if (future.TotalDays >= 1 && future.TotalDays < 2)
                {
                    return $"Tomorrow";
                }
                else if (future.TotalDays > 1 && future.TotalDays < 32)
                {
                    return $"In {Math.Round(future.TotalDays)} days";
                }
                else
                {
                    return $"On {when.Year}/{when.Month}/{when.Day}";
                }
            }
        }

        public string RelativeShort(DateTime when)
        {
            if (when.Ticks == 0)
            {
                return "Never";
            }

            var now = DateTime.Now;
            var past = now.Subtract(when);
            if (when > now)
            {
                past = when.Subtract(now);
            }

            var roundedTime = RoundTime(past);
            if (roundedTime != null)
            {
                if (when > now)
                {
                    return $"In {roundedTime}";
                }
                else
                {
                    return $"{roundedTime} ago";
                }
            }
            else
            {
                return $"{when.Year}/{when.Month}/{when.Day}";
            }
        }

        public string RoundTime(TimeSpan past)
        {
            if (past.TotalSeconds < 5)
            {
                return "< 1 sec";
            }
            else if (past.TotalSeconds >= 5 && past.TotalSeconds < 10)
            {
                return $"< 10 sec";
            }
            else if (past.TotalSeconds >= 10 && past.TotalSeconds < 60)
            {
                return $"< 1 min";
            }
            else if (past.TotalMinutes < 5)
            {
                return "< 5 min";
            }
            else if (past.TotalMinutes >= 5 && past.TotalMinutes < 60)
            {
                return $"{Math.Round(past.TotalMinutes)} min";
            }
            else if (past.TotalHours > 1 && past.TotalHours < 2)
            {
                return $"1 hr";
            }
            else if (past.TotalHours >= 2 && past.TotalHours < 24)
            {
                return $"{Math.Round(past.TotalHours)} hrs";
            }
            else if (past.TotalDays >= 1 && past.TotalDays < 2)
            {
                return $"1 day";
            }
            else if (past.TotalDays > 2 && past.TotalDays < 48)
            {
                return $"{Math.Round(past.TotalDays)} days";
            }
            else
            {
                return null;
            }
        }

        public string GetMessage(ReactionType t, ReactionSubject subject)
        {
            switch (subject)
            {
                case ReactionSubject.Post:
                switch (t)
                {
                    case ReactionType.Like:
                    return "Liked post";
                    case ReactionType.Dislike:
                    return "Disliked post";
                    case ReactionType.Report:
                    return "Reported post";
                    case ReactionType.Block:
                    return "Blocked post";
                }
                break;
                case ReactionSubject.Profile:
                switch (t)
                {
                    case ReactionType.Like:
                    return "Liked profile";
                    case ReactionType.Dislike:
                    return "Disliked profile";
                    case ReactionType.Report:
                    return "Reported profile";
                    case ReactionType.Block:
                    return "Blocked profile";
                    case ReactionType.Follow:
                    return "Followed profile";
                }
                break;
                case ReactionSubject.Shop:
                switch (t)
                {
                    case ReactionType.Like:
                    return "Liked shop";
                    case ReactionType.Dislike:
                    return "Disliked shop";
                    case ReactionType.Report:
                    return "Reported shop";
                    case ReactionType.Block:
                    return "Blocked shop";
                    case ReactionType.Follow:
                    return "Followed shop";
                }
                break;
                case ReactionSubject.ArchivedMedia:
                switch (t)
                {
                    case ReactionType.Like:
                    return "Liked media";
                    case ReactionType.Dislike:
                    return "Disliked media";
                    case ReactionType.Report:
                    return "Reported media";
                    case ReactionType.Block:
                    return "Blocked media";
                }
                break;
            }
            return "no message";
        }

        public string GetUndoMessage(ReactionType t, ReactionSubject subject)
        {
            switch (subject)
            {
                case ReactionSubject.Post:
                switch (t)
                {
                    case ReactionType.Like:
                    return "Unliked post";
                    case ReactionType.Dislike:
                    return "Un-disliked post";
                    case ReactionType.Block:
                    return "Unblocked post";
                }
                break;
                case ReactionSubject.Profile:
                switch (t)
                {
                    case ReactionType.Like:
                    return "Unliked profile";
                    case ReactionType.Dislike:
                    return "Un-disliked profile";
                    case ReactionType.Block:
                    return "Unblocked profile";
                    case ReactionType.Follow:
                    return "Unfollowed profile";
                }
                break;
                case ReactionSubject.Shop:
                switch (t)
                {
                    case ReactionType.Like:
                    return "Unliked shop";
                    case ReactionType.Dislike:
                    return "Un-disliked shop";
                    case ReactionType.Block:
                    return "Unblocked shop";
                    case ReactionType.Follow:
                    return "Unfollowed shop";
                }
                break;
                case ReactionSubject.ArchivedMedia:
                switch (t)
                {
                    case ReactionType.Like:
                    return "Unliked media";
                    case ReactionType.Dislike:
                    return "Un-disliked media";
                    case ReactionType.Block:
                    return "Unblocked media";
                }
                break;
            }
            return "no undo message";
        }

        public string GetCTA(ReactionType t, ReactionSubject subject)
        {
            switch (subject)
            {
                case ReactionSubject.Post:
                switch (t)
                {
                    case ReactionType.Like:
                    return "Like post";
                    case ReactionType.Dislike:
                    return "Dislike post";
                    case ReactionType.Report:
                    return "Report post";
                    case ReactionType.Block:
                    return "Block post";
                }
                break;
                case ReactionSubject.Profile:
                switch (t)
                {
                    case ReactionType.Like:
                    return "Like profile";
                    case ReactionType.Dislike:
                    return "Dislike profile";
                    case ReactionType.Report:
                    return "Report profile";
                    case ReactionType.Block:
                    return "Block profile";
                    case ReactionType.Follow:
                    return "Follow profile";
                }
                break;
                case ReactionSubject.Shop:
                switch (t)
                {
                    case ReactionType.Like:
                    return "Like shop";
                    case ReactionType.Dislike:
                    return "Dislike shop";
                    case ReactionType.Report:
                    return "Report shop";
                    case ReactionType.Block:
                    return "Block shop";
                    case ReactionType.Follow:
                    return "Follow shop";
                }
                break;
                case ReactionSubject.ArchivedMedia:
                switch (t)
                {
                    case ReactionType.Like:
                    return "Like media";
                    case ReactionType.Dislike:
                    return "Dislike media";
                    case ReactionType.Report:
                    return "Report media";
                    case ReactionType.Block:
                    return "Block media";
                }
                break;
            }
            return "no message";
        }

        public string GetUndoCTA(ReactionType t, ReactionSubject subject)
        {
            switch (subject)
            {
                case ReactionSubject.Post:
                switch (t)
                {
                    case ReactionType.Like:
                    return "Unlike post";
                    case ReactionType.Dislike:
                    return "Un-dislike post";
                    case ReactionType.Block:
                    return "Unblock post";
                }
                break;
                case ReactionSubject.Profile:
                switch (t)
                {
                    case ReactionType.Like:
                    return "Unlike profile";
                    case ReactionType.Dislike:
                    return "Un-dislike profile";
                    case ReactionType.Block:
                    return "Unblock profile";
                    case ReactionType.Follow:
                    return "Unfollow profile";
                }
                break;
                case ReactionSubject.Shop:
                switch (t)
                {
                    case ReactionType.Like:
                    return "Unlike shop";
                    case ReactionType.Dislike:
                    return "Un-dislike shop";
                    case ReactionType.Block:
                    return "Unblock shop";
                    case ReactionType.Follow:
                    return "Unfollow shop";
                }
                break;
                case ReactionSubject.ArchivedMedia:
                switch (t)
                {
                    case ReactionType.Like:
                    return "Unlike media";
                    case ReactionType.Dislike:
                    return "Un-dislike media";
                    case ReactionType.Block:
                    return "Unblock media";
                }
                break;
            }
            return "no undo message";
        }

        public string GetContentType(string extension)
        {
            switch (extension)
            {
                case "png": return "image/png";
            }
            return "application/octet-stream";
        }

        public bool IsPositive(string value)
        {
            if (Array.IndexOf(Positives, value) >= 0)
            {
                return true;
            }
            else if (Array.IndexOf(Negatives, value) >= 0)
            {
                return false;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        public string GetInvitationMessagePlaceholder(InvitationType type)
        {
            switch (type)
            {
                case InvitationType.AddMeToGroup: return "For example, \"Here's an invitation to the group I talked to you about\"";
                case InvitationType.Register: return "For example, \"Here's an invitation to sign up on the site I talked to you about\"";
                case InvitationType.FollowMe: return "For example, \"Here's an invitation to follow my profile for posts and notifications\"";
                default: return "";
            }
        }
    }
}