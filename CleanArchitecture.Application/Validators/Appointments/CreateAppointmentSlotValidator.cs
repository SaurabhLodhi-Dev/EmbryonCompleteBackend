using CleanArchitecture.Application.DTOs.Appointments;
using FluentValidation;

namespace CleanArchitecture.Application.Validators.Appointments
{
    /// <summary>
    /// Validates appointment slot creation (admin side).
    /// </summary>
    public class CreateAppointmentSlotValidator : AbstractValidator<CreateAppointmentSlotDto>
    {
        public CreateAppointmentSlotValidator()
        {
            RuleFor(x => x.SlotDate)
                .GreaterThanOrEqualTo(DateTime.Today)
                .WithMessage("Slot date must be today or later.");

            RuleFor(x => x.StartTime)
                .NotEmpty();

            RuleFor(x => x.EndTime)
                .NotEmpty()
                .GreaterThan(x => x.StartTime)
                .WithMessage("End time must be later than start time.");
        }
    }
}
