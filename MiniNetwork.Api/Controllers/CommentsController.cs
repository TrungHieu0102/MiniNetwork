using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniNetwork.Api.RequestModels;
using MiniNetwork.Application.Comments;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MiniNetwork.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;
        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }
        [HttpPost("post/{postId:guid}")]
        public async Task<IActionResult> AddCommentAsync([FromRoute] Guid postId, [FromBody] string content, CancellationToken ct)
        {
            var userId = GetUserIdFromClaims();
            if (userId == Guid.Empty) return Unauthorized();
            var result = await _commentService.AddCommentAsync(postId, userId, content, ct);
            if (!result.Succeeded || result.Data is null)
                return BadRequest(new { error = result.Error });
            return Ok(result.Data);
        }
        [HttpPost("reply/{commentId:guid}")]
        public async Task<IActionResult> ReplyToCommentAsync([FromRoute] Guid commentId, [FromBody] CreateCommentRequest request, CancellationToken ct)
        {
            var userId = GetUserIdFromClaims();
            if (userId == Guid.Empty) return Unauthorized();
            var result = await _commentService.ReplyToCommentAsync(commentId, userId, request.Content, ct);
            if (!result.Succeeded || result.Data is null)
                return BadRequest(new { error = result.Error });
            return Ok(result.Data);
        }
        [AllowAnonymous]
        [HttpGet("post/{postId:guid}")]
        public async Task<IActionResult> GetPostComments(Guid postId, int page = 1, int pageSize = 20, CancellationToken ct = default)
        {
            var result = await _commentService.GetPostCommentsAsync(postId, page, pageSize, ct);
            if (!result.Succeeded) return BadRequest(new { error = result.Error });

            return Ok(result.Data);
        }
        // DELETE /api/comments/{commentId}
        [HttpDelete("{commentId:guid}")]
        public async Task<IActionResult> DeleteComment(Guid commentId, CancellationToken ct)
        {
            var userId = GetUserIdFromClaims();
            if (userId == Guid.Empty) return Unauthorized();

            var result = await _commentService.DeleteCommentAsync(commentId, userId, ct);
            if (!result.Succeeded) return BadRequest(new { error = result.Error });

            return NoContent();
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
