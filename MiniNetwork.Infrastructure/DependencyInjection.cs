using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniNetwork.Application.Auth.Options;
using MiniNetwork.Application.Interfaces.Repositories;
using MiniNetwork.Application.Interfaces.Services;
using MiniNetwork.Infrastructure.Auth;
using MiniNetwork.Infrastructure.Email;
using MiniNetwork.Infrastructure.Persistence;
using MiniNetwork.Infrastructure.Repositories;
using MiniNetwork.Infrastructure.Storage;
using MiniNetwork.Infrastructure.Uow;
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
        services.AddScoped<IFollowRepository, FollowRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
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
        // AwsS3 options
        services.Configure<AwsS3Options>(configuration.GetSection("AwsS3"));

        // File storage
        services.AddScoped<IFileStorageService, S3FileStorageService>();
        //Google Login
        services.Configure<GoogleAuthOptions>(configuration.GetSection("GoogleAuth"));
        services.AddScoped<IBlockRepository, BlockRepository>();
        //Notification Repository   
        services.AddScoped<INotificationRepository, NotificationRepository>();

        return services;
        

    }
}
