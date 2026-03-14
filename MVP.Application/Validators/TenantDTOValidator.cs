using FluentValidation;
using MVP.Application.DTOs;

namespace MVP.Application.Validators;

public class TenantDtoValidator : AbstractValidator<TenantDto>
{
    public TenantDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tenant name is required.")
            .MaximumLength(200).WithMessage("Tenant name cannot exceed 200 characters.");

        RuleFor(x => x.Domain)
            .NotEmpty().WithMessage("Tenant domain is required.")
            .MaximumLength(100).WithMessage("Domain cannot exceed 100 characters.");
    }
}
