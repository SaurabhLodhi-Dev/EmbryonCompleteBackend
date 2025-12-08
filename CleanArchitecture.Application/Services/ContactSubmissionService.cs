//using CleanArchitecture.Application.DTOs.Contact;
//using CleanArchitecture.Application.Interfaces;
//using CleanArchitecture.Domain.Entities;
//using CleanArchitecture.Domain.Interfaces;

//namespace CleanArchitecture.Application.Services
//{
//    public class ContactSubmissionService : IContactSubmissionService
//    {
//        private readonly IContactSubmissionRepository _repository;
//        private readonly ICountryRepository _countryRepository;

//        public ContactSubmissionService(
//            IContactSubmissionRepository repository,
//            ICountryRepository countryRepository)
//        {
//            _repository = repository;
//            _countryRepository = countryRepository;
//        }

//        public async Task<ContactSubmissionDto> GetByIdAsync(Guid id)
//        {
//            var entity = await _repository.GetByIdAsync(id);

//            var countryName = entity.CountryId.HasValue
//                ? (await _countryRepository.GetByIdAsync(entity.CountryId.Value))?.Name
//                : null;

//            return new ContactSubmissionDto(
//                entity.Id,
//                entity.FirstName,
//                entity.LastName,
//                entity.Email,
//                entity.Phone,
//                entity.PhoneCountryCode,
//                countryName,
//                entity.State,
//                entity.City,
//                entity.Subject,
//                entity.Message,
//                entity.IpAddress,
//                entity.CreatedAt
//            );
//        }

//        public async Task<IEnumerable<ContactSubmissionDto>> GetAllAsync()
//        {
//            var submissions = await _repository.GetAllAsync();

//            var dtos = new List<ContactSubmissionDto>();

//            foreach (var s in submissions)
//            {
//                var country = s.CountryId.HasValue
//                    ? (await _countryRepository.GetByIdAsync(s.CountryId.Value))?.Name
//                    : null;

//                dtos.Add(new ContactSubmissionDto(
//                    s.Id,
//                    s.FirstName,
//                    s.LastName,
//                    s.Email,
//                    s.Phone,
//                    s.PhoneCountryCode,
//                    country,
//                    s.State,
//                    s.City,
//                    s.Subject,
//                    s.Message,
//                    s.IpAddress,
//                    s.CreatedAt
//                ));
//            }

//            return dtos;
//        }

//        public async Task<ContactSubmissionDto> CreateAsync(CreateContactSubmissionDto dto)
//        {
//            var entity = new ContactSubmission
//            {
//                FirstName = dto.FirstName,
//                LastName = dto.LastName,
//                Email = dto.Email,
//                Phone = dto.Phone,
//                PhoneCountryCode = dto.PhoneCountryCode,
//                CountryId = dto.CountryId,
//                State = dto.State,
//                City = dto.City,
//                Subject = dto.Subject,
//                Message = dto.Message,

//                IpAddress = dto.IpAddress
//            };

//            var created = await _repository.AddAsync(entity);

//            string? countryName = created.CountryId.HasValue
//                ? (await _countryRepository.GetByIdAsync(created.CountryId.Value))?.Name
//                : null;

//            return new ContactSubmissionDto(
//                created.Id,
//                created.FirstName,
//                created.LastName,
//                created.Email,
//                created.Phone,
//                created.PhoneCountryCode,
//                countryName,
//                created.State,
//                created.City,
//                created.Subject,
//                created.Message,
//                created.IpAddress,
//                created.CreatedAt
//            );
//        }
//    }
//}


using CleanArchitecture.Application.DTOs.Contact;
using CleanArchitecture.Application.Email;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Interfaces;
using Microsoft.Extensions.Options;
using CleanArchitecture.Application.Options;
using Microsoft.Extensions.Logging;


namespace CleanArchitecture.Application.Services
{
    /// <summary>
    /// Handles all business logic related to Contact Form submissions.
    /// 
    /// Responsibilities:
    /// ------------------
    /// • Validate + Save contact submission data
    /// • Auto-resolve and map country name
    /// • Queue notification emails (user + admin)
    /// • Return DTO responses to API layer
    /// 
    /// This service does NOT:
    /// -----------------------
    /// • Perform model validation (handled by FluentValidation)
    /// • Deal with HTTP concerns (controllers do that)
    /// • Send emails directly (background worker handles it)
    /// </summary>
    public class ContactSubmissionService : IContactSubmissionService
    {
        private readonly IContactSubmissionRepository _repository;
        private readonly ICountryRepository _countryRepository;
        private readonly IEmailQueue _emailQueue;
        private readonly ILogger<ContactSubmissionService> _logger;
        private readonly SmtpFromOptions _smtpFrom;
        private readonly ICaptchaValidator _captchaValidator;


        /// <summary>
        /// Constructor with injected dependencies.
        /// </summary>
        public ContactSubmissionService(
      IContactSubmissionRepository repository,
      ICountryRepository countryRepository,
      IEmailQueue emailQueue,
      ILogger<ContactSubmissionService> logger,
      IOptions<SmtpFromOptions> smtpFromOptions, ICaptchaValidator captchaValidator)
        {
            _repository = repository;
            _countryRepository = countryRepository;
            _emailQueue = emailQueue;
            _logger = logger;
            _smtpFrom = smtpFromOptions.Value;
            _captchaValidator = captchaValidator;
        }


        /// <summary>
        /// Returns a single contact submission by ID.
        /// Throws <see cref="KeyNotFoundException"/> if not found.
        /// </summary>
        public async Task<ContactSubmissionDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);

            var countryName = entity.CountryId.HasValue
                ? (await _countryRepository.GetByIdAsync(entity.CountryId.Value))?.Name
                : null;

            return new ContactSubmissionDto(
                entity.Id,
                entity.FirstName,
                entity.LastName,
                entity.Email,
                entity.Phone,
                entity.PhoneCountryCode,
                countryName,
                entity.State,
                entity.City,
                entity.Subject,
                entity.Message,
                entity.IpAddress,
                entity.CreatedAt
            );
        }

        /// <summary>
        /// Returns all contact submissions sorted by creation date (descending).
        /// </summary>
        public async Task<IEnumerable<ContactSubmissionDto>> GetAllAsync()
        {
            var list = await _repository.GetAllAsync();
            var result = new List<ContactSubmissionDto>();

            foreach (var s in list)
            {
                var country = s.CountryId.HasValue
                    ? (await _countryRepository.GetByIdAsync(s.CountryId.Value))?.Name
                    : null;

                result.Add(new ContactSubmissionDto(
                    s.Id,
                    s.FirstName,
                    s.LastName,
                    s.Email,
                    s.Phone,
                    s.PhoneCountryCode,
                    country,
                    s.State,
                    s.City,
                    s.Subject,
                    s.Message,
                    s.IpAddress,
                    s.CreatedAt
                ));
            }

            return result;
        }

        /// <summary>
        /// Creates a new contact form submission and queues notification emails.
        /// 
        /// Steps:
        /// -------
        /// 1) Map DTO → Entity
        /// 2) Save to database (repository)
        /// 3) Resolve country name
        /// 4) Enqueue:
        ///     • Confirmation email to user
        ///     • Alert email to admin
        /// 5) Return formatted DTO
        /// </summary>
        public async Task<ContactSubmissionDto> CreateAsync(CreateContactSubmissionDto dto)
        {

            // Check for null or empty values and handle them
            if (string.IsNullOrEmpty(dto.CaptchaToken))
            {
                throw new ArgumentNullException(nameof(dto.CaptchaToken), "Captcha token cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(dto.IpAddress))
            {
                throw new ArgumentNullException(nameof(dto.IpAddress), "IP address cannot be null or empty.");
            }

            // Validate CAPTCHA
            if (!await _captchaValidator.ValidateAsync(dto.CaptchaToken, dto.IpAddress))
            {
                throw new Exception("CAPTCHA validation failed.");
            }

            // --------------- 1. Map DTO → Entity ---------------
            var entity = new ContactSubmission
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                PhoneCountryCode = dto.PhoneCountryCode,
                CountryId = dto.CountryId,
                State = dto.State,
                City = dto.City,
                Subject = dto.Subject,
                Message = dto.Message,
                IpAddress = dto.IpAddress
            };

            // --------------- 2. Save to Database ---------------
            var created = await _repository.AddAsync(entity);

            // --------------- 3. Resolve Country Name ---------------
            string? countryName = created.CountryId.HasValue
                ? (await _countryRepository.GetByIdAsync(created.CountryId.Value))?.Name
                : null;

            var responseDto = new ContactSubmissionDto(
                created.Id,
                created.FirstName,
                created.LastName,
                created.Email,
                created.Phone,
                created.PhoneCountryCode,
                countryName,
                created.State,
                created.City,
                created.Subject,
                created.Message,
                created.IpAddress,
                created.CreatedAt
            );

            // --------------- 4. Queue Notification Emails ---------------
            try
            {
                // User confirmation email
                if (!string.IsNullOrWhiteSpace(created.Email))
                {
                    var html = EmailTemplates.ContactUserConfirmation(created);
                    var plain = EmailTemplates.ContactUserConfirmationPlain(created);

                    await _emailQueue.EnqueueAsync(new QueuedEmail
                    {
                        ToEmail = created.Email!,
                        FromEmail = _smtpFrom.FromEmail,
                        FromName = _smtpFrom.FromName,
                        Subject = "Thank you for contacting us",
                        HtmlBody = html,
                        PlainBody = plain,
                        Type = "contact_user",
                        RelatedEntityId = created.Id
                    });
                }

                // Admin notification email
                if (!string.IsNullOrWhiteSpace(_smtpFrom.AdminEmail))
                {
                    var html = EmailTemplates.ContactAdminNotification(created);
                    var plain = EmailTemplates.ContactAdminNotificationPlain(created);

                    await _emailQueue.EnqueueAsync(new QueuedEmail
                    {
                        ToEmail = _smtpFrom.AdminEmail!,
                        FromEmail = _smtpFrom.FromEmail,
                        FromName = _smtpFrom.FromName,
                        Subject = $"New Contact Submission — {created.FirstName} {created.LastName}",
                        HtmlBody = html,
                        PlainBody = plain,
                        Type = "contact_admin",
                        RelatedEntityId = created.Id
                    });
                }
            }
            catch (Exception ex)
            {
                // Do NOT throw — creation must succeed even if email fails
                _logger.LogError(ex,
                    "Failed to enqueue contact notification emails for ContactSubmission {Id}",
                    created.Id);
            }

            // --------------- 5. Return DTO ---------------
            return responseDto;
        }
    }
}

