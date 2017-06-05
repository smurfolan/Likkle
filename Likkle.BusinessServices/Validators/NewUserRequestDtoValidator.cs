using System.Linq;
using FluentValidation;
using Likkle.BusinessEntities.Requests;
using Likkle.BusinessServices.Utils;

namespace Likkle.BusinessServices.Validators
{
    public class NewUserRequestDtoValidator : AbstractValidator<NewUserRequestDto>
    {
        private readonly IPhoneValidationManager _phoneValidationManager;
        private readonly IUserService _userService;

        public NewUserRequestDtoValidator(
            IPhoneValidationManager phoneValidationManager,
            IUserService userService)
        {
            this._phoneValidationManager = phoneValidationManager;
            this._userService = userService;

            RuleFor(r => r.IdsrvUniqueId).NotNull();
            RuleFor(r => r.FirstName).NotNull();
            RuleFor(r => r.LastName).NotNull();
            RuleFor(r => r.Email).NotNull();

            RuleFor(r => r.PhoneNumber).Must(BeAValidPhoneNumber).WithMessage("Phone number provided is invalid");
            RuleFor(r => r.IdsrvUniqueId).Must(BeUniqueIdServerId).WithMessage("User with the same STS id has been already added.");
            RuleFor(r => r.Email).Must(BeUniqueEmail).WithMessage("User with the same email has been already added.");
        }

        private bool BeUniqueEmail(string email)
        {
            return this._userService.GetAllUsers().All(x => x.Email != email);
        }

        private bool BeUniqueIdServerId(string stsId)
        {
            return this._userService.GetAllUsers().All(x => x.IdsrvUniqueId != stsId);
        }

        private bool BeAValidPhoneNumber(string phone)
        {
            return string.IsNullOrEmpty(phone) || this._phoneValidationManager.PhoneNumberIsValid(phone);
        }
    }
}
