using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Likkle.BusinessServices.Validators
{
    public static class CommonValidationRules
    {
        private const string FacebookUsernamePrefixForMessenger = "m.me/";
        private const string InstagramUrl = "https://www.instagram.com/{0}/?__a=1";

        public static bool BeAValidInstagramUsername(string instagramUsername)
        {
            if (instagramUsername == null || instagramUsername == string.Empty)
                return true;

            return ValidateSocialLink(string.Format(InstagramUrl, instagramUsername));
        }

        private static bool ValidateSocialLink(string socialLink)
        {
            using (var httpClient = new HttpClient())
            {
                var result = httpClient.GetAsync(socialLink).Result;

                return result.StatusCode == HttpStatusCode.OK ? true : false;
            }
        }

        public static bool BeAValidFacebookUsername(string facebookUsername)
        {
            if (facebookUsername == null || facebookUsername == string.Empty)
                return true;

            return facebookUsername.StartsWith(FacebookUsernamePrefixForMessenger);
        }

        public static bool BeValidTwitterName(string twitterUsername)
        {
            if (twitterUsername == null || twitterUsername == string.Empty)
                return true;

            var twitterRegex = new Regex(@"^@[a-zA-Z0-9_]{1,15}$");

            return twitterRegex.IsMatch(twitterUsername);
        }
    }
}
