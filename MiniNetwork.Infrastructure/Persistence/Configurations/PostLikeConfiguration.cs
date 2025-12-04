using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniNetwork.Domain.Entities;
namespace MiniNetwork.Infrastructure.Persistence.Configurations;

public class PostLikeConfiguration : IEntityTypeConfiguration<PostLike>
{
    public void Configure(EntityTypeBuilder<PostLike> builder)
    {
        builder.HasIndex(pl => new { pl.PostId, pl.UserId }).IsUnique();

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        builder.HasOne(pl => pl.User)
            .WithMany(u => u.Likes)
            .HasForeignKey(pl => pl.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(pl => pl.Post)
            .WithMany(p => p.Likes)
            .HasForeignKey(pl => pl.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
