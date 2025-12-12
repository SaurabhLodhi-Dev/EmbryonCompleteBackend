//using AutoMapper;
//using CleanArchitecture.Application.DTOs.Contact;
//using CleanArchitecture.Application.Email;
//using CleanArchitecture.Application.Interfaces;
//using CleanArchitecture.Application.Options;
//using CleanArchitecture.Domain.Entities;
//using CleanArchitecture.Domain.Interfaces;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;

//namespace CleanArchitecture.Application.Services
//{
//    public class ContactSubmissionService : IContactSubmissionService
//    {
//        private readonly IContactSubmissionRepository _repository;
//        private readonly IEmailQueue _emailQueue;
//        private readonly ILogger<ContactSubmissionService> _logger;
//        private readonly SmtpFromOptions _smtpFrom;
//        private readonly IMapper _mapper;
//        private readonly ICaptchaValidator _captchaValidator;

//        public ContactSubmissionService(
//            IContactSubmissionRepository repository,
//            IEmailQueue emailQueue,
//            ILogger<ContactSubmissionService> logger,
//            IOptions<SmtpFromOptions> smtpFromOptions,
//            ICaptchaValidator captchaValidator, IMapper mapper)
//        {
//            _repository = repository;
//            _emailQueue = emailQueue;
//            _logger = logger;
//            _smtpFrom = smtpFromOptions.Value;
//            _mapper = mapper;
//            _captchaValidator = captchaValidator;
//        }

//        // ============================================================
//        // GET BY ID
//        // ============================================================
//        public async Task<ContactSubmissionDto?> GetByIdAsync(Guid id)
//        {
//            var entity = await _repository.GetByIdAsync(id);
//            if (entity == null)
//                return null;

//            return new ContactSubmissionDto(
//                entity.Id,
//                entity.FirstName,
//                entity.LastName,
//                entity.Email,
//                entity.Phone,
//                entity.PhoneCountryCode,
//                entity.CountryName, // ← UPDATED
//                entity.State,
//                entity.City,
//                entity.Subject,
//                entity.Message,
//                entity.IpAddress,
//                entity.UserAgent,
//                entity.CreatedAt,
//                entity.Latitude,
//                entity.Longitude
//            );
//        }

//        // ============================================================
//        // GET ALL
//        // ============================================================
//        public async Task<IEnumerable<ContactSubmissionDto>> GetAllAsync()
//        {
//            var list = await _repository.GetAllAsync();
//            var result = new List<ContactSubmissionDto>();

//            foreach (var s in list)
//            {
//                result.Add(new ContactSubmissionDto(
//                    s.Id,
//                    s.FirstName,
//                    s.LastName,
//                    s.Email,
//                    s.Phone,
//                    s.PhoneCountryCode,
//                    s.CountryName, // ← UPDATED
//                    s.State,
//                    s.City,
//                    s.Subject,
//                    s.Message,
//                    s.IpAddress,
//                    s.UserAgent,
//                    s.CreatedAt,
//                    s.Latitude,
//                    s.Longitude
//                ));
//            }

//            return result;
//        }

//        /// <summary>
//        /// Creates a new contact form submission.
//        /// Performs:
//        /// 1. CAPTCHA validation
//        /// 2. DTO → Entity mapping
//        /// 3. Saves to database
//        /// 4. Returns formatted DTO
//        /// 5. Queues user + admin emails
//        ///
//        /// Now supports:
//        /// - CountryName instead of CountryId
//        /// - Latitude & Longitude (Geo-location)
//        /// </summary>
//        /// <param name="dto">Incoming sanitized contact submission DTO</param>
//        /// <returns>ContactSubmissionDto representing the created record</returns>
//        /// <exception cref="ArgumentException">Invalid captcha or IP</exception>
//        /// <exception cref="InvalidOperationException">Captcha validation failure</exception>
//        public async Task<ContactSubmissionDto> CreateAsync(CreateContactSubmissionDto dto)
//        {
//            // ======================================================================================
//            // 1. CAPTCHA VALIDATION
//            // ======================================================================================

//            if (string.IsNullOrWhiteSpace(dto.CaptchaToken))
//            {
//                _logger.LogWarning("Contact submission blocked: Missing CAPTCHA token");
//                throw new ArgumentException("CAPTCHA token is required.", nameof(dto.CaptchaToken));
//            }

//            if (string.IsNullOrWhiteSpace(dto.IpAddress))
//            {
//                _logger.LogWarning("Contact submission blocked: Missing IP address");
//                throw new ArgumentException("IP address is required.", nameof(dto.IpAddress));
//            }

//            var isValid = await _captchaValidator.ValidateAsync(dto.CaptchaToken, dto.IpAddress);
//            if (!isValid)
//            {
//                _logger.LogWarning("CAPTCHA validation FAILED for IP {IP}", dto.IpAddress);
//                throw new InvalidOperationException("CAPTCHA validation failed.");
//            }

//            _logger.LogInformation("CAPTCHA validated successfully for IP {IP}", dto.IpAddress);


//            // ======================================================================================
//            // 2. MAP DTO → ENTITY  (NOW INCLUDES LAT & LON)
//            // ======================================================================================
//            //var entity = _mapper.Map<ContactSubmission>(dto);
//            var entity = new ContactSubmission
//            {
//                FirstName = dto.FirstName,
//                LastName = dto.LastName,
//                Email = dto.Email,
//                Phone = dto.Phone,
//                PhoneCountryCode = dto.PhoneCountryCode,

//                // Updated field
//                CountryName = dto.CountryName,

//                State = dto.State,
//                City = dto.City,
//                Subject = dto.Subject,
//                Message = dto.Message,
//                IpAddress = dto.IpAddress,
//                UserAgent = dto.UserAgent,

//                // ⭐ NEW: Geo-Coordinates
//                Latitude = dto.Latitude,
//                Longitude = dto.Longitude
//            };


//            // ======================================================================================
//            // 3. SAVE TO DATABASE
//            // ======================================================================================

//            var created = await _repository.AddAsync(entity);

//            _logger.LogInformation(
//                "Contact submission stored: ID={Id}, Email={Email}, IP={IP}, Lat={Lat}, Lon={Lon}",
//                created.Id, created.Email, created.IpAddress, created.Latitude, created.Longitude
//            );


//            // ======================================================================================
//            // 4. PREPARE RESPONSE DTO (INCLUDES LAT & LON)
//            // ======================================================================================

//            var responseDto = new ContactSubmissionDto(
//                created.Id,
//                created.FirstName,
//                created.LastName,
//                created.Email,
//                created.Phone,
//                created.PhoneCountryCode,
//                created.CountryName,
//                created.State,
//                created.City,
//                created.Subject,
//                created.Message,
//                created.IpAddress,
//                created.UserAgent,
//                created.CreatedAt,
//                created.Latitude,
//                created.Longitude
//            );


//            // ======================================================================================
//            // 5. QUEUE EMAILS (User + Admin)
//            // ======================================================================================

//            try
//            {
//                // --------------------------------------------------
//                // 5A: User Confirmation Email
//                // --------------------------------------------------
//                if (!string.IsNullOrWhiteSpace(created.Email))
//                {
//                    await _emailQueue.EnqueueAsync(new QueuedEmail
//                    {
//                        ToEmail = created.Email!,
//                        FromEmail = _smtpFrom.FromEmail,
//                        FromName = _smtpFrom.FromName,
//                        Subject = "Thank you for contacting us",
//                        HtmlBody = EmailTemplates.ContactUserConfirmation(created),
//                        PlainBody = EmailTemplates.ContactUserConfirmationPlain(created),
//                        Type = "contact_user",
//                        RelatedEntityId = created.Id
//                    });

//                    _logger.LogInformation("User confirmation email queued → {Email}", created.Email);
//                }

//                // --------------------------------------------------
//                // 5B: Admin Notification
//                // --------------------------------------------------
//                if (!string.IsNullOrWhiteSpace(_smtpFrom.AdminEmail))
//                {
//                    await _emailQueue.EnqueueAsync(new QueuedEmail
//                    {
//                        ToEmail = _smtpFrom.AdminEmail!,
//                        FromEmail = _smtpFrom.FromEmail,
//                        FromName = _smtpFrom.FromName,
//                        Subject = $"New Contact Submission — {created.FirstName} {created.LastName}",
//                        HtmlBody = EmailTemplates.ContactAdminNotification(created),
//                        PlainBody = EmailTemplates.ContactAdminNotificationPlain(created),
//                        Type = "contact_admin",
//                        RelatedEntityId = created.Id
//                    });

//                    _logger.LogInformation("Admin notification email queued → {AdminEmail}", _smtpFrom.AdminEmail);
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex,
//                    "Email queue failure for ContactSubmission ID={Id}", created.Id);
//                // DO NOT throw; submission must remain successful even if emails fail
//            }


//            // ======================================================================================
//            // 6. RETURN RESPONSE DTO
//            // ======================================================================================
//            return responseDto;
//        }

//    }
//}


using AutoMapper;
using CleanArchitecture.Application.DTOs.Contact;
using CleanArchitecture.Application.Email;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Options;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class ContactSubmissionService : IContactSubmissionService
{
    private readonly IContactSubmissionRepository _repository;
    private readonly IEmailQueue _emailQueue;
    private readonly ILogger<ContactSubmissionService> _logger;
    private readonly SmtpFromOptions _smtpFrom;
    private readonly ICaptchaValidator _captchaValidator;
    private readonly IMapper _mapper;

    public ContactSubmissionService(
        IContactSubmissionRepository repository,
        IEmailQueue emailQueue,
        ILogger<ContactSubmissionService> logger,
        IOptions<SmtpFromOptions> smtpFromOptions,
        ICaptchaValidator captchaValidator,
        IMapper mapper)
    {
        _repository = repository;
        _emailQueue = emailQueue;
        _logger = logger;
        _smtpFrom = smtpFromOptions.Value;
        _captchaValidator = captchaValidator;
        _mapper = mapper;
    }

    // ============================================================
    // GET BY ID
    // ============================================================
    public async Task<ContactSubmissionDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : _mapper.Map<ContactSubmissionDto>(entity);
    }

    // ============================================================
    // GET ALL
    // ============================================================
    public async Task<IEnumerable<ContactSubmissionDto>> GetAllAsync()
    {
        var list = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<ContactSubmissionDto>>(list);
    }

    // ============================================================
    // CREATE
    // ============================================================
    public async Task<ContactSubmissionDto> CreateAsync(CreateContactSubmissionDto dto)
    {
        // --------------------------
        // 1. CAPTCHA validation
        // --------------------------
        if (string.IsNullOrWhiteSpace(dto.CaptchaToken))
            throw new ArgumentException("CAPTCHA token is required.");

        if (string.IsNullOrWhiteSpace(dto.IpAddress))
            throw new ArgumentException("IP address is required.");

        var isValid = await _captchaValidator.ValidateAsync(dto.CaptchaToken, dto.IpAddress);
        if (!isValid)
            throw new InvalidOperationException("CAPTCHA validation failed.");


        // --------------------------
        // 2. AUTOMAPPER mapping
        // --------------------------
        var entity = _mapper.Map<ContactSubmission>(dto);

        // Server-assigned fields
        entity.CreatedAt = DateTime.UtcNow;


        // --------------------------
        // 3. Save to DB
        // --------------------------
        var created = await _repository.AddAsync(entity);


        // --------------------------
        // 4. AUTO-MAP response DTO
        // --------------------------
        var responseDto = _mapper.Map<ContactSubmissionDto>(created);


        // --------------------------
        // 5. Queue Emails
        // --------------------------
        QueueEmails(created);


        // --------------------------
        // 6. Return DTO
        // --------------------------
        return responseDto;
    }


    // ============================================================
    // Email queue logic (cleanly extracted)
    // ============================================================
    private async void QueueEmails(ContactSubmission created)
    {
        try
        {
            // USER CONFIRMATION
            if (!string.IsNullOrWhiteSpace(created.Email))
            {
                await _emailQueue.EnqueueAsync(new QueuedEmail
                {
                    ToEmail = created.Email,
                    FromEmail = _smtpFrom.FromEmail,
                    FromName = _smtpFrom.FromName,
                    Subject = "Thank you for contacting us",
                    HtmlBody = EmailTemplates.ContactUserConfirmation(created),
                    PlainBody = EmailTemplates.ContactUserConfirmationPlain(created),
                    Type = "contact_user",
                    RelatedEntityId = created.Id
                });
            }

            // ADMIN NOTIFICATION
            if (!string.IsNullOrWhiteSpace(_smtpFrom.AdminEmail))
            {
                await _emailQueue.EnqueueAsync(new QueuedEmail
                {
                    ToEmail = _smtpFrom.AdminEmail,
                    FromEmail = _smtpFrom.FromEmail,
                    FromName = _smtpFrom.FromName,
                    Subject = $"New Contact Submission — {created.FirstName} {created.LastName}",
                    HtmlBody = EmailTemplates.ContactAdminNotification(created),
                    PlainBody = EmailTemplates.ContactAdminNotificationPlain(created),
                    Type = "contact_admin",
                    RelatedEntityId = created.Id
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email queue failure for ContactSubmission ID={Id}", created.Id);
        }
    }
}
