namespace PinkUmbrella.Models.Settings
{
    public enum FeatureFlags
    {
        [FeatureFlagInfo("System API Controller", "API for site system information", FeatureFlagType.Controller)]
        ApiControllerSystem,

        [FeatureFlagInfo("Profile API Controller", "API for user profile information", FeatureFlagType.Controller)]
        ApiControllerProfile,




        [FeatureFlagInfo("Account Controller", "View and manage account", FeatureFlagType.Controller)]
        ControllerAccount,

        [FeatureFlagInfo("Admin Controller", "Enable administrative functions", FeatureFlagType.Controller)]
        ControllerAdmin,

        [FeatureFlagInfo("Archive Controller", "View and upload archive media", FeatureFlagType.Controller)]
        ControllerArchive,

        [FeatureFlagInfo("Developer Controller", "Enable developer tools", FeatureFlagType.Controller)]
        ControllerDeveloper,

        [FeatureFlagInfo("Graph QL Controller", "Enable GraphQL API for client users", FeatureFlagType.Controller)]
        ControllerGraphQl,

        [FeatureFlagInfo("Home Controller", "View most relevant info for logged in users. Anonymous users get homepage.", FeatureFlagType.Controller)]
        ControllerHome,

        [FeatureFlagInfo("Civic Duty Controller", "View and participate in civic duties", FeatureFlagType.Controller)]
        ControllerCivicDuty,

        [FeatureFlagInfo("Inventory Controller", "View and manage inventories", FeatureFlagType.Controller)]
        ControllerInventory,

        [FeatureFlagInfo("Notification Controller", "View and manage notifications", FeatureFlagType.Controller)]
        ControllerNotification,

        [FeatureFlagInfo("Profile Controller", "View and manage user profiles", FeatureFlagType.Controller)]
        ControllerProfile,

        [FeatureFlagInfo("Post Controller", "View and manage posts", FeatureFlagType.Controller)]
        ControllerPost,

        [FeatureFlagInfo("Reaction Controller", "React to things", FeatureFlagType.Controller)]
        ControllerReaction,

        [FeatureFlagInfo("Shop Controller", "View and manage shops", FeatureFlagType.Controller)]
        ControllerShop,

        [FeatureFlagInfo("Tag Controller", "Create, read, update, delete tags", FeatureFlagType.Controller)]
        ControllerTag,

        
        
        
        
        // [FeatureFlagInfo("User Service", "Create, read, update, delete users", FeatureFlagType.Service)]
        // ServiceUser,


        
        
        [FeatureFlagInfo("Mutations", "Change data in any way (read only mode if disabled)", FeatureFlagType.Global)]
        MutationsAllowed,

        // [FeatureFlagInfo("Registered Users Allowed (Mod Only)", "Disabling will cause mod only mode for site", FeatureFlagType.Global)]
        // RegisteredAllowed,

        // [FeatureFlagInfo("Admins Allowed (Dev Only)", "Disabling will cause dev only mode for site", FeatureFlagType.Global)]
        // AdminsAllowed,



        [FeatureFlagInfo("Log Exception", "Logs exceptions", FeatureFlagType.Function)]
        FunctionExceptionLog,


        
        
        [FeatureFlagInfo("User Register", "Allow users to register", FeatureFlagType.Function)]
        FunctionUserRegister,
        
        [FeatureFlagInfo("User Register Invitation Only", "Allow users to register only if they have a valid invitation", FeatureFlagType.Function)]
        FunctionUserRegisterInvitationOnly,

        [FeatureFlagInfo("User Log In", "Allow users to log in", FeatureFlagType.Function)]
        FunctionUserLogin,

        [FeatureFlagInfo("User Log In via FIDO U2F", "Allow users to log in via FIDO, a security key exchange system", FeatureFlagType.Function)]
        FunctionUserLoginFIDO,

        [FeatureFlagInfo("User Log In via Public / Private Key", "Allow users to log in via public and private keys using RSA or PGP", FeatureFlagType.Function)]
        FunctionUserLoginPublicKey,

        [FeatureFlagInfo("User Log In via Open Auth", "Allow users to log in via external login with an open authenticator", FeatureFlagType.Function)]
        FunctionUserLoginOAuth,

        [FeatureFlagInfo("User Update Account", "Allow users to update their account", FeatureFlagType.Function)]
        FunctionUserUpdateAccount,

        [FeatureFlagInfo("User Update Profile", "Allow users to update their profile", FeatureFlagType.Function)]
        FunctionUserUpdateProfile,

        [FeatureFlagInfo("User Download Personal Data", "Allow users to download their personal data", FeatureFlagType.Function)]
        FunctionUserDownloadPersonalData,

        [FeatureFlagInfo("User Delete", "Allow users to delete their accounts and related data", FeatureFlagType.Function)]
        FunctionUserDelete,

        [FeatureFlagInfo("User Change Password", "Allow users to change their password", FeatureFlagType.Function)]
        FunctionUserChangePassword,

        [FeatureFlagInfo("Function User Manage Auth Key", "Allow users to manage authentication keys", FeatureFlagType.Function)]
        FunctionUserManageKeys,

        [FeatureFlagInfo("Function User Add Auth Key", "Allow users to add authentication keys", FeatureFlagType.Function)]
        FunctionUserAddAuthKey,

        [FeatureFlagInfo("Function User Gen Auth Key", "Allow users to generate authentication keys", FeatureFlagType.Function)]
        FunctionUserGenAuthKey,

        [FeatureFlagInfo("Function User Invite", "Allow users to generate invitations (like registration or following)", FeatureFlagType.Function)]
        FunctionUserInvite,



        [FeatureFlagInfo("Post Message", "Allow users to post content", FeatureFlagType.Function)]
        FunctionPostMessage,



        [FeatureFlagInfo("Archive Upload", "Allow users to upload archive media", FeatureFlagType.Function)]
        FunctionArchiveUpload,



        [FeatureFlagInfo("New Inventory", "Allow users to create inventories", FeatureFlagType.Function)]
        FunctionInventoryNew,

        [FeatureFlagInfo("New Inventory Resource", "Allow users to create resources", FeatureFlagType.Function)]
        FunctionInventoryNewResource,



        [FeatureFlagInfo("List Your Business", "Allow users to create new shops", FeatureFlagType.Function)]
        FunctionShopNew,

        
        [FeatureFlagInfo("View COVID-19 Banner Message", "Links to CDC", FeatureFlagType.View)]
        ViewCovid19BannerMessage,


        
        [FeatureFlagInfo("View Search Pages", "Disabling will hide search pages and actions", FeatureFlagType.View)]
        ViewSearch,
    }
}