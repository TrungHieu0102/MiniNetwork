using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using MiniNetwork.Application.Auth;
using MiniNetwork.Application.Blocks;
using MiniNetwork.Application.Common.Mapping;
using MiniNetwork.Application.Follows;
using MiniNetwork.Application.Notifications;
using MiniNetwork.Application.Posts;
using MiniNetwork.Application.Users;
namespace MiniNetwork.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {

        services.AddScoped<IAuthService, AuthService>();
        // User
        services.AddScoped<IUserService, UserService>();
        //Follow
        services.AddScoped<IFollowService, FollowService>();
        //Notification
        services.AddScoped<INotificationService, NotificationService>();
        //Block
        services.AddScoped<IBlockService, BlockService>();
        //Post
        services.AddScoped<IPostService, PostService>();
        // AutoMapper
        var mapperConfig = new MapperConfiguration(cfg =>   
        {
            cfg.AddProfile<MappingProfile>();
        }, NullLoggerFactory.Instance);

        IMapper mapper = mapperConfig.CreateMapper();
        services.AddSingleton(mapper);

        return services;
    }
}
