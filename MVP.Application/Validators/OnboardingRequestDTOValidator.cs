using FluentValidation;
using MVP.Application.DTOs;

namespace MVP.Application.Validators;

public class OnboardingRequestDTOValidator : AbstractValidator<OnboardingRequestDTO>
{
    public OnboardingRequestDTOValidator()
    {
        RuleFor(x => x.EmpresaNombre)
            .NotEmpty().WithMessage("El nombre de la empresa es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre de la empresa no puede exceder los 200 caracteres.");

        RuleFor(x => x.Dominio)
            .NotEmpty().WithMessage("El dominio es obligatorio.")
            .MaximumLength(100).WithMessage("El dominio no puede exceder los 100 caracteres.");

        RuleFor(x => x.AdminEmail)
            .NotEmpty().WithMessage("El correo electrónico del administrador es obligatorio.")
            .EmailAddress().WithMessage("El formato del correo electrónico del administrador no es válido.")
            .MaximumLength(256).WithMessage("El correo electrónico no puede exceder los 256 caracteres.");

        RuleFor(x => x.AdminPassword)
            .NotEmpty().WithMessage("La contraseña del administrador es obligatoria.")
            .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.")
            .MaximumLength(100).WithMessage("La contraseña no puede exceder los 100 caracteres.");

        RuleFor(x => x.AdminNombre)
            .NotEmpty().WithMessage("El nombre del administrador es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre del administrador no puede exceder los 100 caracteres.");

        RuleFor(x => x.AdminPrimerApellido)
            .NotEmpty().WithMessage("El primer apellido del administrador es obligatorio.")
            .MaximumLength(100).WithMessage("El primer apellido no puede exceder los 100 caracteres.");

        RuleFor(x => x.AdminSegundoApellido)
            .MaximumLength(100).WithMessage("El segundo apellido no puede exceder los 100 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.AdminSegundoApellido));
    }
}
