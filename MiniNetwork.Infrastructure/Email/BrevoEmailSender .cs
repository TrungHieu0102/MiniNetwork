using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MiniNetwork.Application.Interfaces.Services;

namespace MiniNetwork.Infrastructure.Email;

public class BrevoEmailSender : IEmailSender
{
    private readonly BrevoOptions _options;
    private readonly HttpClient _client;

    public BrevoEmailSender(IOptions<BrevoOptions> options)
    {
        _options = options.Value;
        _client = new HttpClient();
        _client.BaseAddress = new Uri("https://api.brevo.com/v3/");
        _client.DefaultRequestHeaders.Add("api-key", _options.ApiKey);
        _client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlContent, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            sender = new
            {
                email = _options.FromEmail,
                name = _options.FromName
            },
            to = new[]
            {
                new { email = toEmail }
            },
            subject = subject,
            htmlContent = htmlContent
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("smtp/email", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception($"Brevo send failed: {response.StatusCode} - {body}");
        }
    }
}
