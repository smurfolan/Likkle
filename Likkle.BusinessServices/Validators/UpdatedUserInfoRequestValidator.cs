using System.Net;
using System.Net.Http;
using FluentValidation;
using Likkle.BusinessEntities.Requests;

namespace Likkle.BusinessServices.Validators
{
    public class UpdatedUserInfoRequestValidator : AbstractValidator<UpdateUserInfoRequestDto>
    {
        private const string FacebookUrl = "https://www.facebook.com/{0}";
        private const string FacebookUsernamePrefixForMessenger = "m.me/";
        private const string InstagramUrl = "https://www.instagram.com/{0}/?__a=1";

        public UpdatedUserInfoRequestValidator()
        {
            RuleFor(userInfo => userInfo.Email)
                .EmailAddress()
                .WithMessage("Invalid email address");

            RuleFor(userInfo => userInfo.InstagramUsername)
                .Must(BeAValidInstagramUsername)
                .WithMessage("Instagram username is not valid");

            RuleFor(userInfo => userInfo.FacebookUsername)
                .Must(BeAValidFacebookUsername)
                .WithMessage("Facebook username is not valid");

            RuleFor(userInfo => userInfo.FacebookUsername)
                .Must(s => s.StartsWith(FacebookUsernamePrefixForMessenger))
                .WithMessage("Facebook username must have the prefix: " + FacebookUsernamePrefixForMessenger);
        }

        private bool BeAValidInstagramUsername(string instagramUsername)
        {
            if (instagramUsername == null)
                return true;

            return this.ValidateSocialLink(string.Format(InstagramUrl, instagramUsername));
        }

        private bool BeAValidFacebookUsername(string facebookUsername)
        {
            // TODO: Think of a way for validating facebook somehow
            //if (facebookUsername == null)
            //    return true;

            //return this.ValidateSocialLink(string.Format(FacebookUrl, facebookUsername));

            return true;
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
