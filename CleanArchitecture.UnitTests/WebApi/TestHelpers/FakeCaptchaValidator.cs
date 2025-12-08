using CleanArchitecture.Application.Interfaces;

public class FakeCaptchaValidator : ICaptchaValidator
{
    public Task<bool> ValidateAsync(string token, string ip)
    {
        return Task.FromResult(true);
    }
}
