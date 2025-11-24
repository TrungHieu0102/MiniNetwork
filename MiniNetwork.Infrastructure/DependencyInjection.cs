using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniNetwork.Infrastructure.Persistence;

namespace MiniNetwork.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Connection string trong appsettings.json
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<MiniNetworkDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        // Sau này: đăng ký repository, cache, v.v. ở đây

        return services;
    }
}
