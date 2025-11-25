using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniNetwork.Application.Interfaces.Repositories;
using MiniNetwork.Application.Interfaces.Services;
using MiniNetwork.Infrastructure.Auth;
using MiniNetwork.Infrastructure.Persistence;
using MiniNetwork.Infrastructure.Repositories;
using MiniNetwork.Infrastructure.Uow;
using MiniNetwork.Infrastructure.Email;
namespace MiniNetwork.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<MiniNetworkDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        // JwtOptions (bind từ appsettings.json section "Jwt")
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddScoped<IPasswordHasher, PasswordHasherService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IUserTokenRepository, UserTokenRepository>();
        // Bind Brevo options
        services.Configure<BrevoOptions>(configuration.GetSection("Brevo"));
        services.AddSingleton<IEmailTemplateService, EmailTemplateService>();

        // Email sender
        services.AddScoped<IEmailSender, BrevoEmailSender>();
        return services;
    }
}
