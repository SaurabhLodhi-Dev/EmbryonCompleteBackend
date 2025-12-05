using CleanArchitecture.Application.DTOs.Appointments;
using FluentValidation;

namespace CleanArchitecture.Application.Validators.Appointments
{
    /// <summary>
    /// Validates new appointment types (zoom/webrtc/gmeet).
    /// </summary>
    public class CreateAppointmentTypeValidator : AbstractValidator<CreateAppointmentTypeDto>
    {
        public CreateAppointmentTypeValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Type name is required.")
                .MaximumLength(100);
        }
    }
}
