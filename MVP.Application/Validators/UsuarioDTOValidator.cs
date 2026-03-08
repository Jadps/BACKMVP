using FluentValidation;
using MVP.Application.DTOs;

namespace MVP.Application.Validators;

public class UsuarioDTOValidator : AbstractValidator<UsuarioDTO>
{
    public UsuarioDTOValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress().WithMessage("El formato del correo electrónico no es válido.")
            .MaximumLength(256).WithMessage("El correo electrónico no puede exceder los 256 caracteres.");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres.");

        RuleFor(x => x.PrimerApellido)
            .NotEmpty().WithMessage("El primer apellido es obligatorio.")
            .MaximumLength(100).WithMessage("El primer apellido no puede exceder los 100 caracteres.");

        RuleFor(x => x.SegundoApellido)
            .MaximumLength(100).WithMessage("El segundo apellido no puede exceder los 100 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.SegundoApellido));

        RuleFor(x => x.Password)
            .MaximumLength(100).WithMessage("La contraseña no puede exceder los 100 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Password));
    }
}
