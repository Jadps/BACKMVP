using FluentValidation;
using MVP.Application.DTOs;

namespace MVP.Application.Validators;

public class UserDtoValidator : AbstractValidator<UserDto>
{
    public UserDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

        RuleFor(x => x.SecondLastName)
            .MaximumLength(100).WithMessage("Second last name cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.SecondLastName));

        RuleFor(x => x.Password)
            .MaximumLength(100).WithMessage("Password cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Password));
    }
}
