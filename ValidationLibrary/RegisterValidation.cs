using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedModels;

namespace ValidationLibrary;

public class RegisterValidation : AbstractValidator<RegistrationViewModel>
{
    public RegisterValidation()
    {
        RuleFor(p => p.Username).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(30)
            .Must(BeValidUsername).WithMessage("Username can be only Letter, Digit or _")
            .Must(UsernameFirstChar).WithMessage("First Char must be Letter");

        RuleFor(p => p.FirstName).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MinimumLength(1)
            .MaximumLength(50)
            .Must(firstname => firstname.All(Char.IsLetter)).WithMessage("FirstName must be all Letter");

        RuleFor(p => p.LastName).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(50)
            .Must(lastname => lastname.All(Char.IsLetter)).WithMessage("FirstName must be all Letter");

        RuleFor(p => p.EmailAddress).EmailAddress();
    }

    protected bool BeValidUsername(string username)
    {
        // Check if username is not empty, has a valid length, and meets the alphanumeric + underscore pattern
        return !string.IsNullOrEmpty(username) &&
               username.Length >= 6 && username.Length <= 30 &&
               username.All(c => char.IsLetterOrDigit(c) || c == '_');
    }
    protected bool UsernameFirstChar(string username)
    {
        return !string.IsNullOrEmpty(username) &&
            char.IsLetter(username.First());
    }
}
