//using CleanArchitecture.Application.DTOs.Contact;
//using FluentValidation;

//namespace CleanArchitecture.Application.Validators.Contact
//{
//    /// <summary>
//    /// Validates the Contact Form submission request.
//    /// </summary>
//    public class CreateContactSubmissionValidator : AbstractValidator<CreateContactSubmissionDto>
//    {
//        public CreateContactSubmissionValidator()
//        {
//            RuleFor(x => x.FirstName)
//                .NotEmpty().WithMessage("First name is required.")
//                .MaximumLength(150);

//            RuleFor(x => x.LastName)
//                .NotEmpty().WithMessage("Last name is required.")
//                .MaximumLength(150);

//            RuleFor(x => x.Email)
//                .NotEmpty().WithMessage("Email is required.")
//                .EmailAddress().WithMessage("Please enter a valid email address.");

//            RuleFor(x => x.Phone)
//                .NotEmpty().WithMessage("Phone number is required.")
//                .MaximumLength(50);

//            RuleFor(x => x.PhoneCountryCode)
//                .NotEmpty().WithMessage("Country calling code is required.")
//                .MaximumLength(10);

//            RuleFor(x => x.Subject)
//                .NotEmpty().WithMessage("Subject is required.")
//                .MaximumLength(200);

//            RuleFor(x => x.Message)
//                .NotEmpty().WithMessage("Message is required.")
//                .MaximumLength(2000);
//            RuleFor(x => x.CaptchaToken)

//    .NotNull().WithMessage("Captcha token is required.")
//    .NotEmpty().WithMessage("Captcha token cannot be empty.")
//    .MinimumLength(10).WithMessage("Invalid captcha token.");


//            //RuleFor(x => x.CaptchaPassed)
//            //    .Equal(true).WithMessage("Captcha verification failed.");

//            //RuleFor(x => x.IpAddress)
//            //    .NotEmpty().WithMessage("IP Address is required.");
//        }
//    }
//}


using FluentValidation;
using CleanArchitecture.Application.DTOs.Contact;

namespace CleanArchitecture.Application.Validators
{
    /// <summary>
    /// Validates contact form submission data.
    /// Enforces business rules for required fields, formats, and lengths.
    /// </summary>
    public class CreateContactSubmissionValidator : AbstractValidator<CreateContactSubmissionDto>
    {
        public CreateContactSubmissionValidator()
        {
            // ==========================================
            // REQUIRED FIELDS
            // ==========================================

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("First name is required.")
                .MaximumLength(100)
                .WithMessage("First name cannot exceed 100 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Last name is required.")
                .MaximumLength(100)
                .WithMessage("Last name cannot exceed 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email address is required.")
                .EmailAddress()
                .WithMessage("Please provide a valid email address.")
                .MaximumLength(255)
                .WithMessage("Email cannot exceed 255 characters.");

            RuleFor(x => x.Message)
                .NotEmpty()
                .WithMessage("Message is required.")
                .MinimumLength(10)
                .WithMessage("Message must be at least 10 characters.")
                .MaximumLength(5000)
                .WithMessage("Message cannot exceed 5000 characters.");

            RuleFor(x => x.CaptchaToken)
                .NotEmpty()
                .WithMessage("CAPTCHA token is required for security verification.");

            // ==========================================
            // OPTIONAL FIELDS WITH VALIDATION
            // ==========================================

            RuleFor(x => x.Phone)
                .MaximumLength(20)
                .WithMessage("Phone number cannot exceed 20 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Phone));

            RuleFor(x => x.PhoneCountryCode)
                .MaximumLength(5)
                .WithMessage("Phone country code cannot exceed 5 characters.")
                .Matches(@"^\+?\d{1,4}$")
                .WithMessage("Phone country code must be numeric and may start with '+'.")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneCountryCode));

            RuleFor(x => x.Subject)
                .MaximumLength(200)
                .WithMessage("Subject cannot exceed 200 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Subject));

            RuleFor(x => x.State)
                .MaximumLength(100)
                .WithMessage("State cannot exceed 100 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.State));

            RuleFor(x => x.City)
                .MaximumLength(100)
                .WithMessage("City cannot exceed 100 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.City));

            // ==========================================
            // BUSINESS RULES
            // ==========================================

            // If phone is provided, country code should be provided too
            RuleFor(x => x.PhoneCountryCode)
                .NotEmpty()
                .WithMessage("Phone country code is required when phone number is provided.")
                .When(x => !string.IsNullOrWhiteSpace(x.Phone));
        }
    }
}