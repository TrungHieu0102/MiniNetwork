namespace MiniNetwork.Infrastructure.Email;

public class BrevoOptions
{
    public string ApiKey { get; set; } = null!;
    public string FromEmail { get; set; } = null!;
    public string FromName { get; set; } = "MiniNetwork";
}
