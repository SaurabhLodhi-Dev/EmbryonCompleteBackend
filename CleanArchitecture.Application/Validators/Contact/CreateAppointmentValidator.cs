using CleanArchitecture.Application.DTOs.Appointments;
using FluentValidation;

namespace CleanArchitecture.Application.Validators.Appointments
{
    /// <summary>
    /// Validates appointment booking requests.
    /// </summary>
    public class CreateAppointmentValidator : AbstractValidator<CreateAppointmentDto>
    {
        public CreateAppointmentValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(150);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(150);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress();

            RuleFor(x => x.AppointmentTypeId)
                .NotEmpty().WithMessage("Appointment type is required.");

            RuleFor(x => x.SlotId)
                .NotEmpty().WithMessage("Appointment slot is required.");

            RuleFor(x => x.Description)
                .MaximumLength(2000);

            RuleFor(x => x.IpAddress)
                .NotEmpty().WithMessage("IP Address is required.");
        }
    }
}
