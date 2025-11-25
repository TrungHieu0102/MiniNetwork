using Microsoft.Extensions.DependencyInjection;
using MiniNetwork.Application.Auth;

namespace MiniNetwork.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {

        services.AddScoped<IAuthService, AuthService>();
        // AutoMapper
        return services;
    }
}
