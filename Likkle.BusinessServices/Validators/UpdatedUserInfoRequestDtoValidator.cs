using System;
using System.Linq;
using FluentValidation;
using Likkle.BusinessEntities.Requests;
using Likkle.BusinessServices.Utils;
using Likkle.BusinessEntities;

namespace Likkle.BusinessServices.Validators
{
    public class UpdatedUserInfoRequestDtoValidator : AbstractValidator<UpdateUserInfoRequestDto>
    {
        private readonly IPhoneValidationManager _phoneValidationManager;
        private readonly IUserService _userService;
        private readonly Guid _userId;

        public UpdatedUserInfoRequestDtoValidator(
            Guid id,
            IPhoneValidationManager phoneValidationManager, 
            IUserService userService)
        {
            _phoneValidationManager = phoneValidationManager;
            _userService = userService;
            _userId = id;

            RuleFor(r => r.FirstName).NotNull();
            RuleFor(r => r.Email).NotNull();

            When(x => x.AutomaticSubscriptionSettings != null, () => {
                RuleFor(r => r.AutomaticSubscriptionSettings)
                    .Must(BeValidNotificationSettingsCombination)
                    .WithMessage("The options for AutomaticallySubscribeToAllGroups and AutomaticallySubscribeToAllGroupsWithTag can not be both set to 'true'.");
            });

            When(x => x.SocialLinks != null, () => {
                RuleFor(r => r.SocialLinks.FacebookUsername)
                    .Must(CommonValidationRules.BeAValidFacebookUsername)
                    .WithMessage("Facebook username must have the prefix: m.me/");

                RuleFor(r => r.SocialLinks.TwitterUsername)
                    .Must(CommonValidationRules.BeValidTwitterName)
                    .WithMessage("Twitter username is not valid");

                RuleFor(r => r.SocialLinks.InstagramUsername)
                    .Must(CommonValidationRules.BeAValidInstagramUsername)
                    .WithMessage("Instagram username is not valid");
            });     

            RuleFor(r => r.PhoneNumber).Must(BeAValidPhoneNumber).WithMessage("Phone number provided is invalid");
            RuleFor(r => r.Email).EmailAddress().Must(BeUniqueEmail).WithMessage("User with the same email has been already added.");
        }

        private bool BeUniqueEmail(string email)
        {
            return this._userService.GetAllUsers().Where(u => u.Id != this._userId).All(x => x.Email != email);
        }

        private bool BeAValidPhoneNumber(string phone)
        {
            return string.IsNullOrEmpty(phone) || this._phoneValidationManager.PhoneNumberIsValid(phone);
        }

        private bool BeValidNotificationSettingsCombination(AutomaticSubscriptionSettingsDto notificationSettings)
        {
            return !(notificationSettings.AutomaticallySubscribeToAllGroups && notificationSettings.AutomaticallySubscribeToAllGroupsWithTag);
        }
    }
}
