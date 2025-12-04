using Bogus;
using Microsoft.EntityFrameworkCore;
using MiniNetwork.Domain.Entities;
using MiniNetwork.Domain.Enums;
using MiniNetwork.Infrastructure.Persistence;

namespace MiniNetwork.Infrastructure.Seeding;

public class DataSeeder
{
    private readonly MiniNetworkDbContext _dbContext;

    public DataSeeder(MiniNetworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        // Nếu DB đã có user thì bỏ qua
        //if (await _dbContext.Users.AnyAsync(ct))
        //    return;

        const int USER_COUNT = 300;
        const int MAX_FOLLOWS_PER_USER = 30;
        const int MIN_POSTS_PER_USER = 3;
        const int MAX_POSTS_PER_USER = 10;
        const int MAX_LIKES_PER_POST = 80;

        // Speed up seeding
        _dbContext.ChangeTracker.AutoDetectChangesEnabled = false;

        // === Seed Steps ===

        var users = await SeedUsersAsync(USER_COUNT, ct);
        await SeedFollowsAsync(users, MAX_FOLLOWS_PER_USER, ct);
        var posts = await SeedPostsAsync(users, MIN_POSTS_PER_USER, MAX_POSTS_PER_USER, ct);
        await SeedLikesAsync(users, posts, MAX_LIKES_PER_POST, ct);

        // enable lại tracker
        _dbContext.ChangeTracker.AutoDetectChangesEnabled = true;
    }

    private async Task<List<User>> SeedUsersAsync(int count, CancellationToken ct)
    {
        var faker = new Faker("en");
        var users = new List<User>();

        for (int i = 0; i < count; i++)
        {
            var userName = faker.Internet.UserName();
            var email = faker.Internet.Email();
            var displayName = faker.Name.FullName();

            // Fake password hash (bạn có thể thay bằng hashing thật)
            var passwordHash = Guid.NewGuid().ToString("N");

            var user = new User(
                userName,
                email,
                passwordHash,
                displayName);

            // Optional profile fields
            user.UpdateProfile(
                displayName,
                faker.Lorem.Sentence(),
                faker.Internet.Avatar());

            users.Add(user);
        }

        await _dbContext.Users.AddRangeAsync(users, ct);
        await _dbContext.SaveChangesAsync(ct);

        return users;
    }

    private async Task SeedFollowsAsync(List<User> users, int maxFollowsPerUser, CancellationToken ct)
    {
        var random = new Random();
        var follows = new List<Follow>();

        foreach (var user in users)
        {
            int followCount = random.Next(0, maxFollowsPerUser + 1);

            var targets = users
                .Where(u => u.Id != user.Id)
                .OrderBy(_ => random.Next())
                .Take(followCount)
                .ToList();

            foreach (var target in targets)
            {
                // tránh duplicate
                if (follows.Any(f => f.FollowerId == user.Id && f.FolloweeId == target.Id))
                    continue;

                follows.Add(new Follow(user.Id, target.Id));
            }
        }

        await _dbContext.Follows.AddRangeAsync(follows, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    private async Task<List<Post>> SeedPostsAsync(
        List<User> users,
        int minPostsPerUser,
        int maxPostsPerUser,
        CancellationToken ct)
    {
        var faker = new Faker("en");
        var random = new Random();
        var posts = new List<Post>();

        foreach (var user in users)
        {
            int postCount = random.Next(minPostsPerUser, maxPostsPerUser + 1);

            for (int i = 0; i < postCount; i++)
            {
                var content = faker.Lorem.Sentences(random.Next(1, 5));

                var post = new Post(user.Id, content, PostVisibility.Public);

                posts.Add(post);
            }
        }

        await _dbContext.Posts.AddRangeAsync(posts, ct);
        await _dbContext.SaveChangesAsync(ct);

        return posts;
    }

    private async Task SeedLikesAsync(
        List<User> users,
        List<Post> posts,
        int maxLikesPerPost,
        CancellationToken ct)
    {
        var random = new Random();
        var likes = new List<PostLike>();

        foreach (var post in posts)
        {
            int likeCount = random.Next(0, maxLikesPerPost + 1);

            var likers = users
                .Where(u => u.Id != post.AuthorId)
                .OrderBy(_ => random.Next())
                .Take(likeCount)
                .ToList();

            foreach (var user in likers)
            {
                if (likes.Any(l => l.PostId == post.Id && l.UserId == user.Id))
                    continue;

                likes.Add(new PostLike(post.Id, user.Id));
            }
        }

        await _dbContext.PostLikes.AddRangeAsync(likes, ct);
        await _dbContext.SaveChangesAsync(ct);
    }
}
