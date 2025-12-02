using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniNetwork.Api.RequestModels;
using MiniNetwork.Application.Posts;
using MiniNetwork.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MiniNetwork.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postServices;

        public PostsController(IPostService postServices)
        {
            _postServices = postServices;
        }
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreatePost([FromForm] string content, [FromForm] List<IFormFile>? images, CancellationToken ct)
        {
            var user = GetUserIdFromClaims();
            if (user == Guid.Empty) return Unauthorized();
            IEnumerable<Stream>? imageStreams = images?.Select(f => f.OpenReadStream());
            var result = await _postServices.CreateAsync(user, content, imageStreams, ct);
            if (!result.Succeeded)
            {
                return BadRequest(result.Error);
            }
            return Ok(result.Data);
        }
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPostDetail(Guid id, CancellationToken ct)
        {
            var result = await _postServices.GetByIdAsync(id, ct);
            if (!result.Succeeded)
            {
                return NotFound(result.Error);
            }
            return Ok(result.Data);
        }
        [HttpPut("{id:guid}/content")]
        public async Task<IActionResult> UpdateContent(Guid id, [FromBody] UpdatePostContentRequest request, CancellationToken ct)
        {
            var user = GetUserIdFromClaims();
            if (user == Guid.Empty) return Unauthorized();
            var result = await _postServices.UpdateContentAsync(id, user, request.Content, ct);
            if (!result.Succeeded) return BadRequest(new { error = result.Error });
            return Ok(result);
        }
        [HttpPost("{id:guid}/images")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddImages(Guid id, [FromForm] List<IFormFile> images, CancellationToken ct)
        {
            var user = GetUserIdFromClaims();
            if (user == Guid.Empty) return Unauthorized();
            var streams = images.Select(i => i.OpenReadStream());
            var result = await _postServices.AddImagesAsync(id, user, streams, ct);
            if (!result.Succeeded) return BadRequest(new { error = result.Error });
            return Ok(result);
        }
        // ------------------------------
        // DELETE ONE IMAGE
        // ------------------------------
        [HttpDelete("{postId:guid}/images/{imageId:guid}")]
        public async Task<IActionResult> DeleteImage(Guid postId, Guid imageId, CancellationToken ct)
        {
            var user = GetUserIdFromClaims();
            if (user == Guid.Empty) return Unauthorized();
            var result = await _postServices.DeleteImageAsync(postId, user, imageId, ct);
            if (!result.Succeeded) return BadRequest(new { error = result.Error });
            return NoContent();
        }
        [HttpDelete("{postId:guid}")]
        public async Task<IActionResult> Delete(Guid postId, CancellationToken ct)
        {
            var user = GetUserIdFromClaims();
            if (user == Guid.Empty) return Unauthorized();
            var result = await _postServices.DeleteAsync(postId, user, ct);
            if (!result.Succeeded) return BadRequest(new { error = result.Error });
            return NoContent();
        }
        // GET api/posts/me?page=1&pageSize=10
        [HttpGet("me")]
        public async Task<IActionResult> GetMyPosts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            var user = GetUserIdFromClaims();
            if (user == Guid.Empty) return Unauthorized();

            var result = await _postServices.GetUserPostsAsync(user, page, pageSize, ct);
            if (!result.Succeeded)
                return BadRequest(new { error = result.Error });

            return Ok(result.Data);
        }

        // GET api/posts/user/{userId}?page=1&pageSize=10
        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetUserPosts(
            Guid userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            var result = await _postServices.GetUserPostsAsync(userId, page, pageSize, ct);
            if (!result.Succeeded)
                return BadRequest(new { error = result.Error });

            return Ok(result.Data);
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
}
