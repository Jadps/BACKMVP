using FluentValidation;
using MVP.Application.DTOs;

namespace MVP.Application.Validators;

public class TenantDTOValidator : AbstractValidator<TenantDTO>
{
    public TenantDTOValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del tenant es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre del tenant no puede exceder los 200 caracteres.");

        RuleFor(x => x.Dominio)
            .NotEmpty().WithMessage("El dominio del tenant es obligatorio.")
            .MaximumLength(100).WithMessage("El dominio no puede exceder los 100 caracteres.");
    }
}
