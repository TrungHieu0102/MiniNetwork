namespace MiniNetwork.Application.Interfaces.Services;

public interface IEmailSender
{
    Task SendEmailAsync(
        string toEmail,
        string subject,
        string htmlContent,
        CancellationToken cancellationToken = default);
}
