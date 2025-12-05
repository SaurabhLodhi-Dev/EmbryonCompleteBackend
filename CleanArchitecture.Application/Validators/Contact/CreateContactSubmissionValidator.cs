using CleanArchitecture.Application.DTOs.Contact;
using FluentValidation;

namespace CleanArchitecture.Application.Validators.Contact
{
    /// <summary>
    /// Validates the Contact Form submission request.
    /// </summary>
    public class CreateContactSubmissionValidator : AbstractValidator<CreateContactSubmissionDto>
    {
        public CreateContactSubmissionValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(150);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(150);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Please enter a valid email address.");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone number is required.")
                .MaximumLength(50);

            RuleFor(x => x.PhoneCountryCode)
                .NotEmpty().WithMessage("Country calling code is required.")
                .MaximumLength(10);

            RuleFor(x => x.Subject)
                .NotEmpty().WithMessage("Subject is required.")
                .MaximumLength(200);

            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("Message is required.")
                .MaximumLength(2000);
            RuleFor(x => x.CaptchaToken)

    .NotNull().WithMessage("Captcha token is required.")
    .NotEmpty().WithMessage("Captcha token cannot be empty.")
    .MinimumLength(10).WithMessage("Invalid captcha token.");


            //RuleFor(x => x.CaptchaPassed)
            //    .Equal(true).WithMessage("Captcha verification failed.");

            //RuleFor(x => x.IpAddress)
            //    .NotEmpty().WithMessage("IP Address is required.");
        }
    }
}
