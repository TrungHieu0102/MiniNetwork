using Microsoft.AspNetCore.Identity;
using MiniNetwork.Application.Interfaces.Services;
using MiniNetwork.Domain.Entities;

namespace MiniNetwork.Infrastructure.Auth;

public class PasswordHasherService : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    public string HashPassword(string password)
    {
        return _hasher.HashPassword(user: null!, password);
    }

    public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
    {
        var result = _hasher.VerifyHashedPassword(user: null!, hashedPassword, providedPassword);
        return result != PasswordVerificationResult.Failed;
    }
}
