using System.Net;
using System.Net.Http;
using FluentValidation;
using Likkle.BusinessEntities.Requests;
using System.Text.RegularExpressions;

namespace Likkle.BusinessServices.Validators
{
    public class UpdateSocialLinksRequestDtoValidator : AbstractValidator<UpdateSocialLinksRequestDto>
    {
        private const string FacebookUsernamePrefixForMessenger = "m.me/";
        private const string InstagramUrl = "https://www.instagram.com/{0}/?__a=1";

        public UpdateSocialLinksRequestDtoValidator()
        {
            RuleFor(userInfo => userInfo.InstagramUsername)
                .Must(BeAValidInstagramUsername)
                .WithMessage("Instagram username is not valid");

            RuleFor(userInfo => userInfo.FacebookUsername)
                .Must(BeAValidFacebookUsername)
                .WithMessage("Facebook username must have the prefix: " + FacebookUsernamePrefixForMessenger);

            RuleFor(userInfo => userInfo.TwitterUsername)
                .Must(BeValidTwitterName)
                .WithMessage("Twitter username is not valid");
        }

        private bool BeAValidInstagramUsername(string instagramUsername)
        {
            if (instagramUsername == null || instagramUsername == string.Empty)
                return true;

            return this.ValidateSocialLink(string.Format(InstagramUrl, instagramUsername));
        }

        private bool ValidateSocialLink(string socialLink)
        {
            using (var httpClient = new HttpClient())
            {
                var result = httpClient.GetAsync(socialLink).Result;

                return result.StatusCode == HttpStatusCode.OK ? true : false;
            }
        }

        private bool BeAValidFacebookUsername(string facebookUsername)
        {
            if (facebookUsername == null || facebookUsername == string.Empty)
                return true;

            return facebookUsername.StartsWith(FacebookUsernamePrefixForMessenger);
        }

        private bool BeValidTwitterName(string twitterUsername)
        {
            if (twitterUsername == null || twitterUsername == string.Empty)
                return true;

            var twitterRegex = new Regex(@"^@[a-zA-Z0-9_]{1,15}$");

            return twitterRegex.IsMatch(twitterUsername);
        }
    }
}
