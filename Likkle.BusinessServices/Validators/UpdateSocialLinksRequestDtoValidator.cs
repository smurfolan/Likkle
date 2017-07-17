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
                .Must(CommonValidationRules.BeAValidInstagramUsername)
                .WithMessage("Instagram username is not valid");

            RuleFor(userInfo => userInfo.FacebookUsername)
                .Must(CommonValidationRules.BeAValidFacebookUsername)
                .WithMessage("Facebook username must have the prefix: " + FacebookUsernamePrefixForMessenger);

            RuleFor(userInfo => userInfo.TwitterUsername)
                .Must(CommonValidationRules.BeValidTwitterName)
                .WithMessage("Twitter username is not valid");
        }
    }
}
