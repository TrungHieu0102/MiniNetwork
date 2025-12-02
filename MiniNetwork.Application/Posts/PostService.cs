using MiniNetwork.Application.Common;
using MiniNetwork.Application.Interfaces.Repositories;
using MiniNetwork.Application.Interfaces.Services;
using MiniNetwork.Application.Posts.DTOs;
using MiniNetwork.Domain.Entities;

namespace MiniNetwork.Application.Posts
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IUnitOfWork _unitOfWork;
        public PostService(IPostRepository postRepository, IFileStorageService fileStorageService, IUnitOfWork unitOfWork)
        {
            _postRepository = postRepository;
            _fileStorageService = fileStorageService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> AddImagesAsync(Guid postId, Guid userId, IEnumerable<Stream> images, CancellationToken ct = default)
        {
            var post = await _postRepository.GetByIdAsync(postId, ct);
            if (post == null)
                return Result.Failure("Post not found.");
            if (post.AuthorId != userId)
                return Result.Failure("Unauthorized to add images to this post.");
            if (images == null || !images.Any())
                return Result.Failure("No images provided.");
            foreach (var image in images)
            {
                var fileName = image is FileStream fs ? Path.GetFileName(fs.Name) : "image.png";
                var key = BuildPostKey(post.Id, fileName);
                var url = await _fileStorageService.UploadAsync(image, key, "image/jpeg", ct);
                post.AddImage(url);
            }
            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }

        public async Task<Result<PostDto>> CreateAsync(Guid userId, string content, IEnumerable<Stream>? images, CancellationToken ct = default)
        {
            var post = new Post(userId, content);
            await _postRepository.AddAsync(post, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            if (images != null && images.Any())
            {
                foreach (var image in images)
                {
                    var fileName = image is FileStream fs ? Path.GetFileName(fs.Name) : "image.png";
                    var key = BuildPostKey(post.Id, fileName);
                    var url = await _fileStorageService.UploadAsync(image, key, "image/jpeg", ct);
                    post.AddImage(url);
                }
                await _unitOfWork.SaveChangesAsync(ct);
            }

            var dto = MapToDto(post);

            return Result<PostDto>.Success(dto);
        }

        public async Task<Result> DeleteAsync(Guid postId, Guid userId, CancellationToken ct = default)
        {
            var post = await _postRepository.GetDetailAsync(postId, ct);
            if (post == null)
                return Result.Failure("Post not found.");
            if (post.AuthorId != userId)
                return Result.Failure("Unauthorized to delete this post.");
            foreach (var image in post.Images)
            {
                await _fileStorageService.DeleteAsync(image.Url, ct);
            }
            _postRepository.Remove(post);
            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }

        public async Task<Result> DeleteImageAsync(Guid postId, Guid userId, Guid imageId, CancellationToken ct = default)
        {
            var post = await _postRepository.GetDetailAsync(postId, ct);
            if (post == null)
                return Result.Failure("Post not found.");
            if (post.AuthorId != userId)
                return Result.Failure("Unauthorized to delete images from this post.");
            var image = post.Images.FirstOrDefault(i => i.Id == imageId);
            if (image == null)
                return Result.Failure("Image not found in this post.");
            await _fileStorageService.DeleteAsync(image.Url, ct);
            post.RemoveImage(imageId);
            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success();

        }

        public async Task<Result<PostDto>> GetByIdAsync(Guid postId, CancellationToken ct = default)
        {
            if (postId == Guid.Empty)
                return Result<PostDto>.Failure("Invalid post ID.");
            var post = await _postRepository.GetDetailAsync(postId, ct);
            if (post == null)
                return Result<PostDto>.Failure("Post not found.");
            var dto = MapToDto(post);

            return Result<PostDto>.Success(dto);
        }

        public async Task<Result> UpdateContentAsync(Guid postId, Guid userId, string content, CancellationToken ct = default)
        {
            var post = await _postRepository.GetByIdAsync(postId, ct);
            if (post == null)
                return Result.Failure("Post not found.");
            if (post.AuthorId != userId)
                return Result.Failure("Unauthorized to update this post.");
            post.SetContent(content);
            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }
        private static string BuildPostKey(Guid postId, string originalFileName)
        {
            var ext = Path.GetExtension(originalFileName);
            if (string.IsNullOrWhiteSpace(ext)) ext = ".png";
            return $"posts/{postId}/{Guid.NewGuid()}{ext}";
        }
        private static PostDto MapToDto(Post post)
        {
            return new PostDto
            {
                Id = post.Id,
                AuthorId = post.AuthorId,
                Content = post.Content,
                AuthorName = post.Author?.UserName ?? "",
                Images = post.Images.Select(i => i.Url).ToList(),
                CommentsCount = post.Comments.Count,
                LikesCount = post.Likes.Count,
                CreatedAt = post.CreatedAt
            };
        }

        public async Task<Result<IReadOnlyList<PostDto>>> GetUserPostsAsync(Guid authorId, int page, int pageSize, CancellationToken ct = default)
        {
            if (authorId == Guid.Empty)
                return Result<IReadOnlyList<PostDto>>.Failure("Invalid author id.");

            if (page <= 0 || pageSize <= 0)
                return Result<IReadOnlyList<PostDto>>.Failure("Invalid paging parameters.");

            var posts = await _postRepository.GetPostsByUserAsync(authorId, page, pageSize, ct);

            var dtos = posts
                .Select(MapToDto)
                .ToList()
                .AsReadOnly();

            return Result<IReadOnlyList<PostDto>>.Success(dtos);
        }
    }
}
