using System;
using System.Collections.Generic;
using System.Text;

namespace MiniNetwork.Application.Interfaces.Services
{
    public interface IEmailTemplateService
    {
        string Render(string templateName, Dictionary<string, string> values);
    }
}
