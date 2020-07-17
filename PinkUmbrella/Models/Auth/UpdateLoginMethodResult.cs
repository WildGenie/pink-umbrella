namespace PinkUmbrella.Models.Auth
{
    public class UpdateLoginMethodResult
    {
        public enum ResultType
        {
            NoError,
            NotAllowed,
            MinimumOneAllowedLoginMethod,
        }

        public ResultType Result { get; set; }
    }
}