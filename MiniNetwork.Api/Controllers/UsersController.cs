using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniNetwork.Application.Users;
using MiniNetwork.Application.Users.DTOs;

namespace MiniNetwork.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
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
    public async Task<IActionResult> UpdateMe(
        [FromBody] UpdateProfileRequest request,
        CancellationToken ct)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty) return Unauthorized();

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var result = await _userService.UpdateProfileAsync(userId, request, ct);
        if (!result.Succeeded)
            return BadRequest(new { error = result.Error });

        return NoContent();
    }

    // GET api/users/{id}
    [HttpGet("{id:guid}")]
    [Authorize] 
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _userService.GetUserProfileAsync(id, ct);
        if (!result.Succeeded || result.Data is null)
            return NotFound(new { error = result.Error });

        return Ok(result.Data);
    }

    // GET api/users?query=abc
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Search([FromQuery] string? query, CancellationToken ct)
    {
        var result = await _userService.SearchUsersAsync(query, ct);
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
