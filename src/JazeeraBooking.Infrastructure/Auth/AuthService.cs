using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JazeeraBooking.Application.Common.Exceptions;
using JazeeraBooking.Application.Common.Interfaces;
using JazeeraBooking.Application.Common.Models;
using JazeeraBooking.Application.Features.Auth;
using JazeeraBooking.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JazeeraBooking.Infrastructure.Auth;

/// <summary>
/// Implements Application's IAuthService here (not in Application itself) because it needs
/// two genuinely infrastructure-flavored things: a cryptographic password hasher and a JWT
/// library. Password hashing uses ASP.NET Core Identity's PasswordHasher&lt;T&gt; - PBKDF2
/// with a random salt and per-hash iteration count embedded in the output, so hasher upgrades
/// don't invalidate existing hashes. We deliberately did NOT pull in the rest of Identity
/// (UserManager, SignInManager, IdentityDbContext, external login providers) - this API has
/// exactly one login flow for internal staff, and the full Identity stack would add
/// significant DI/schema surface for features we don't use.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IStaffUserRepository _staffUserRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;
    private readonly PasswordHasher<StaffUser> _passwordHasher = new();

    public AuthService(IStaffUserRepository staffUserRepository, IUnitOfWork unitOfWork, IOptions<JwtSettings> jwtSettings)
    {
        _staffUserRepository = staffUserRepository;
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _staffUserRepository.GetByUsernameAsync(request.Username, cancellationToken);

        // Same generic message whether the username doesn't exist or the password is wrong -
        // distinguishing them lets an attacker enumerate valid usernames.
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        var (token, expiresAtUtc) = GenerateAccessToken(user);
        return new LoginResponse(token, expiresAtUtc, user.Username, user.Role);
    }

    public async Task<StaffUserDto> RegisterStaffAsync(RegisterStaffRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _staffUserRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (existing is not null)
        {
            throw new ConflictException($"Username '{request.Username}' is already taken.");
        }

        var user = new StaffUser
        {
            Username = request.Username,
            Email = request.Email,
            Role = request.Role,
            IsActive = true
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        await _staffUserRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new StaffUserDto(user.Id, user.Username, user.Email, user.Role, user.IsActive);
    }

    private (string Token, DateTime ExpiresAtUtc) GenerateAccessToken(StaffUser user)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}
