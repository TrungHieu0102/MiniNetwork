using Microsoft.EntityFrameworkCore;
using MiniNetwork.Domain.Entities;
namespace MiniNetwork.Infrastructure.Persistence;

public class MiniNetworkDbContext : DbContext
{
    public MiniNetworkDbContext(DbContextOptions<MiniNetworkDbContext> options)
        : base(options)
    {

    }
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserToken> UserTokens => Set<UserToken>();
    public DbSet<Block> Blocks => Set<Block>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<PostLike> PostLikes => Set<PostLike>();
    public DbSet<Follow> Follows => Set<Follow>();
    public DbSet<PostImage> PostImages => Set<PostImage>();

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MiniNetworkDbContext).Assembly);
    }

}
