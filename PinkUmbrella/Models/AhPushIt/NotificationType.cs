using PinkUmbrella.Util;
using Tides.Util;

namespace PinkUmbrella.Models.AhPushIt
{
    public enum NotificationType
    {
        [Name("Direct notifications from other users")]
        DIRECT_NOTIFICATION = 0,

        [Name("When you are mentioned in a text post")]
        TEXT_POST_MENTION = 1,

        [Name("When a post is tagged with a tag you follow")]
        TEXT_POST_FOLLOWED_TAG = 2,

        [Name("When a post is made from someone you follow")]
        TEXT_POST_FOLLOWED_USER = 3,


        [Name("When a post you've made is liked")]
        YOUR_POST_WAS_LIKED =  4,

        [Name("When a post you've made is disliked")]
        YOUR_POST_WAS_DISLIKED = 5,

        [Name("When a post you've made is replied to")]
        YOUR_POST_WAS_REPLIED_TO = 6,

        [Name("When a post you've made is shared")]
        YOUR_POST_WAS_SHARED = 7,

        [Name("When media you've uploaded is liked")]
        YOUR_MEDIA_WAS_LIKED =  8,

        [Name("When media you've uploaded is disliked")]
        YOUR_MEDIA_WAS_DISLIKED = 9,

        [Name("When a shop you own is liked")]
        YOUR_SHOP_WAS_LIKED =  10,

        [Name("When a shop you own is disliked")]
        YOUR_SHOP_WAS_DISLIKED = 11,

        [Name("When someone follows you")]
        SOMONE_FOLLOWED_YOU = 12,


        [Name("When someone likes you")]
        SOMEONE_LIKED_YOU = 13,

        [Name("When someone dislikes you")]
        SOMEONE_DISLIKED_YOU = 14,
        

        [Name("When you password changes")]
        ACCOUNT_PASSWORD_CHANGED = 15,

        [Name("When you email changes")]
        ACCOUNT_EMAIL_CHANGED = 16,

        [Name("When your personal data is downloaded")]
        ACCOUNT_PERSONAL_DATA_DOWNLOADED = 17,

        [Name("When your account is deleted")]
        ACCOUNT_DELETED = 18,

        [Name("When your account is banned")]
        ACCOUNT_BANNED = 19,

        [Name("When your account is logged into frrom a new device")]
        ACCOUNT_LOGGED_IN_NEW_DEVICE = 20,
    }
}