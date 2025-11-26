using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniNetwork.Api.Contracts.Users;
using MiniNetwork.Application.Follows;
using MiniNetwork.Application.Interfaces.Services;
using MiniNetwork.Application.Users;
using MiniNetwork.Application.Users.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MiniNetwork.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IFileStorageService _fileStorage;
    private readonly IFollowService _followService;

    public UsersController(IUserService userService, IFileStorageService fileStorage, IFollowService followService)
    {
        _userService = userService;
        _fileStorage = fileStorage;
        _followService = followService;
    }

    // GET api/users/me
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _userService.GetCurrentUserProfileAsync(userId, ct);
        if (!result.Succeeded || result.Data is null)
            return NotFound(new { error = result.Error });

        return Ok(result.Data);
    }

    // PUT api/users/me
    [HttpPut("me")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateMe(
        [FromForm] UpdateProfileWithAvatarRequest request,
        CancellationToken ct)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty) return Unauthorized();

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        string? avatarUrl = null;

        if (request.Avatar is not null && request.Avatar.Length > 0)
        {
            var key = BuildAvatarKey(userId, request.Avatar.FileName);
            using var stream = request.Avatar.OpenReadStream();

            avatarUrl = await _fileStorage.UploadAsync(
                stream,
                key,
                request.Avatar.ContentType ?? "image/png",
                ct);
        }

        var appRequest = new UpdateProfileRequest
        {
            DisplayName = request.DisplayName,
            Bio = request.Bio,
            AvatarUrl = avatarUrl // nếu null, trong UserService có thể giữ avatar cũ tuỳ bạn
        };

        var result = await _userService.UpdateProfileAsync(userId, appRequest, ct);
        if (!result.Succeeded)
            return BadRequest(new { error = result.Error });

        return NoContent();
    }

    // GET api/users/{id}
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid profileUserId, CancellationToken ct)
    {
        var user = GetUserIdFromClaims();
        if(user == Guid.Empty) return Unauthorized();
        var result = await _userService.GetUserProfileAsync(profileUserId,user, ct);
        if (!result.Succeeded || result.Data is null)
            return NotFound(new { error = result.Error });

        return Ok(result.Data);
    }
    [HttpPost("me/avatar")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAvatar(
        [FromForm] IFormFile avatar,
        CancellationToken ct)
    {
        if (avatar is null || avatar.Length == 0)
            return BadRequest(new { error = "File avatar không hợp lệ." });

        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty) return Unauthorized();

        var key = BuildAvatarKey(userId, avatar.FileName);

        using var stream = avatar.OpenReadStream();

        var avatarUrl = await _fileStorage.UploadAsync(
            stream,
            key,
            avatar.ContentType ?? "image/png",
            ct);

        var result = await _userService.UpdateAvatarAsync(userId, avatarUrl, ct);
        if (!result.Succeeded)
            return BadRequest(new { error = result.Error });

        return Ok(new { avatarUrl });
    }

    // GET api/users?query=abc
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Search([FromQuery] string? query, CancellationToken ct)
    {
        var result = await _userService.SearchUsersAsync(query, ct);
        return Ok(result.Data);
    }
    [HttpDelete("me/avatar")]
    [Authorize]
    public async Task<IActionResult> DeleteAvatar(
    [FromQuery] string url,
    CancellationToken ct)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty) return Unauthorized();

        if (string.IsNullOrWhiteSpace(url))
            return BadRequest(new { error = "Thiếu url ảnh cần xoá." });

        // Optional: kiểm tra url có thuộc user này không
        if (!UrlBelongsToUserAvatar(url, userId))
            return Forbid(); // tránh user này xoá ảnh của user khác

        await _fileStorage.DeleteAsync(url, ct);



        return NoContent();
    }
    [HttpPost("{targetId:guid}/follow")]
    [Authorize]
    public async Task<IActionResult> Follow(Guid targetId, CancellationToken ct)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty) return Unauthorized();
        var result = await _followService.FollowAsync(userId, targetId, ct);
        if (!result.Succeeded)
            return BadRequest(new { error = result.Error });
        return NoContent();
    }
    [HttpDelete("{targetId:guid}/follow")]
    [Authorize]
    public async Task<IActionResult> Unfollow(Guid targetId, CancellationToken ct)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty) return Unauthorized();
        var result = await _followService.UnfollowAsync(userId, targetId, ct);
        if (!result.Succeeded)
            return BadRequest(new { error = result.Error });
        return NoContent();
    }
    private static bool UrlBelongsToUserAvatar(string url, Guid userId)
    {
        // Vì key upload đang dạng avatars/{userId}/xxx.ext
        // nên URL sẽ chứa đoạn này
        return url.Contains($"/avatars/{userId}/", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildAvatarKey(Guid userId, string originalFileName)
    {
        var ext = Path.GetExtension(originalFileName);
        if (string.IsNullOrWhiteSpace(ext)) ext = ".png";

        return $"avatars/{userId}/{Guid.NewGuid()}{ext}";
    }

    private Guid GetUserIdFromClaims()
    {
        var userIdStr =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
            User.FindFirstValue("sub");

        return Guid.TryParse(userIdStr, out var id) ? id : Guid.Empty;
    }
}
