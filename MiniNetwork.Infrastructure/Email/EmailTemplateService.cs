using MiniNetwork.Application.Interfaces.Services;

namespace MiniNetwork.Infrastructure.Email;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly string _templatesPath;

    public EmailTemplateService()
    {
        // Thư mục bin của startup (Api)
        var basePath = AppContext.BaseDirectory;

        // Email/Templates nằm dưới bin (đã copy content)
        _templatesPath = Path.Combine(basePath, "Email", "Templates");
    }

    public string Render(string templateName, Dictionary<string, string> values)
    {
        var filePath = Path.Combine(_templatesPath, templateName);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Template not found: {filePath}");

        var html = File.ReadAllText(filePath);

        foreach (var kv in values)
        {
            html = html.Replace("{{" + kv.Key + "}}", kv.Value);
        }

        return html;
    }
}
