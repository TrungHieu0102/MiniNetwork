using Microsoft.Extensions.DependencyInjection;
using MiniNetwork.Application.Auth;

namespace MiniNetwork.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Đăng ký các service ở Application
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
