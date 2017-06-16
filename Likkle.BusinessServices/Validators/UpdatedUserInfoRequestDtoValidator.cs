using System;
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
            RuleFor(r => r.LastName).NotNull();
            RuleFor(r => r.Email).NotNull();

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
    }
}
