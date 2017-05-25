using System.Linq;
using FluentValidation;
using Likkle.BusinessEntities.Requests;
using Likkle.BusinessServices.Utils;

namespace Likkle.BusinessServices.Validators
{
    public class UpdatedUserInfoRequestDtoValidator : AbstractValidator<UpdateUserInfoRequestDto>
    {
        private readonly IPhoneValidationManager _phoneValidationManager;
        private readonly IUserService _userService;

        public UpdatedUserInfoRequestDtoValidator(
            IPhoneValidationManager phoneValidationManager, 
            IUserService userService)
        {
            _phoneValidationManager = phoneValidationManager;
            _userService = userService;

            RuleFor(r => r.FirstName).NotNull();
            RuleFor(r => r.LastName).NotNull();
            RuleFor(r => r.Email).NotNull();

            RuleFor(r => r.PhoneNumber).Must(BeAValidPhoneNumber).WithMessage("Phone number provided is invalid");
            RuleFor(r => r.Email).Must(BeUniqueEmail).WithMessage("User with the same email has been already added.");
        }

        private bool BeUniqueEmail(string email)
        {
            return this._userService.GetAllUsers().All(x => x.Email != email);
        }

        private bool BeAValidPhoneNumber(string phone)
        {
            return this._phoneValidationManager.PhoneNumberIsValid(phone);
        }
    }
}
