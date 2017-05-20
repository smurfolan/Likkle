using System.Net;
using System.Net.Http;
using FluentValidation;
using Likkle.BusinessEntities.Requests;

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
                .Must(s => s.StartsWith(FacebookUsernamePrefixForMessenger))
                .WithMessage("Facebook username must have the prefix: " + FacebookUsernamePrefixForMessenger);

            RuleFor(userInfo => userInfo.TwitterUsername)
                .Matches("^@[a-zA-Z0-9_]{1,15}$")
                .WithMessage("Twitter username is not valid");
        }

        private bool BeAValidInstagramUsername(string instagramUsername)
        {
            if (instagramUsername == null)
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
    }
}
