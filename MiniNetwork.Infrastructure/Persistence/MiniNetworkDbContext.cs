using Microsoft.EntityFrameworkCore;
using MiniNetwork.Domain.Entities;
using System;

namespace MiniNetwork.Infrastructure.Persistence;

public class MiniNetworkDbContext : DbContext
{
    public MiniNetworkDbContext(DbContextOptions<MiniNetworkDbContext> options)
        : base(options)
    {
    }

    // DbSet cho các entity
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
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);

        // Follow (như cũ)
        modelBuilder.Entity<Follow>()
            .HasOne(f => f.Follower)
            .WithMany(u => u.Following)
            .HasForeignKey(f => f.FollowerId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Follow>()
            .HasOne(f => f.Followee)
            .WithMany(u => u.Followers)
            .HasForeignKey(f => f.FolloweeId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Author)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Post)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique indexes (như cũ)
        modelBuilder.Entity<User>()
            .HasIndex(u => u.NormalizedUserName)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.NormalizedEmail)
            .IsUnique();

        modelBuilder.Entity<Role>()
            .HasIndex(r => r.NormalizedName)
            .IsUnique();

        modelBuilder.Entity<PostLike>()
           .HasOne(pl => pl.User)
           .WithMany(u => u.Likes)
           .HasForeignKey(pl => pl.UserId)
           .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<PostLike>()
           .HasOne(pl => pl.Post)
           .WithMany(p => p.Likes)
           .HasForeignKey(pl => pl.PostId)
           .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PostLike>()
            .HasIndex(pl => new { pl.PostId, pl.UserId })
            .IsUnique();

        modelBuilder.Entity<Follow>()
            .HasIndex(f => new { f.FollowerId, f.FolloweeId })
            .IsUnique();
        modelBuilder.Entity<UserToken>()
            .HasIndex(ut => ut.Token)
            .IsUnique();

        modelBuilder.Entity<UserToken>()
            .HasOne(ut => ut.User)
            .WithMany() 
            .HasForeignKey(ut => ut.UserId)
            .OnDelete(DeleteBehavior.Cascade);


        modelBuilder.Entity<Follow>()
            .HasIndex(f => new { f.FolloweeId, f.CreatedAt });

        modelBuilder.Entity<Follow>()
            .HasIndex(f => new { f.FollowerId, f.CreatedAt });
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MiniNetworkDbContext).Assembly);
        modelBuilder.Entity<Block>()
            .HasOne(b => b.Blocker)
            .WithMany()     
            .HasForeignKey(b => b.BlockerId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Block>()
            .HasOne(b => b.Blocked)
            .WithMany()
            .HasForeignKey(b => b.BlockedId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Block>()
            .HasIndex(b => new { b.BlockerId, b.BlockedId })
            .IsUnique();
    }

}
