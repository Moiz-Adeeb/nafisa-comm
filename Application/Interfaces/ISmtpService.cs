namespace Application.Interfaces;

public interface ISmtpService
{
    Task<bool> SendEmailAsync(string email, string subject, string body);
}
