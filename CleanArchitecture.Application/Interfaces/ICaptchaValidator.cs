namespace CleanArchitecture.Application.Interfaces
{
    public interface ICaptchaValidator
    {
        Task<bool> ValidateAsync(string token, string ip);
    }
}
