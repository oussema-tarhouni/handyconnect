using HandyConnect.Api.Common;
using HandyConnect.Api.Data;
using HandyConnect.Api.Models;
using HandyConnect.Api.Options;
using HandyConnect.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace HandyConnect.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    UserRepository users,
    ContentRepository content,
    TokenService tokens,
    IOptions<SubscriptionOptions> subscription) : ControllerBase
{
    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        var phone = PhoneNumber.Normalize(request.Phone);
        if (phone is null) return BadRequest(ApiError.InvalidPhone);

        var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLowerInvariant();

        ArtisanProfileData? artisan = null;
        if (request.Role == "artisan")
        {
            var trades = await content.GetTradesAsync(ct);
            if (request.TradeId is null || trades.All(t => t.Id != request.TradeId))
                return BadRequest(ApiError.InvalidInput);

            artisan = new ArtisanProfileData
            {
                TradeId = request.TradeId.Value,
                Bio = request.Bio?.Trim() ?? "",
                Zone = request.Zone?.Trim() ?? "",
                ExperienceYears = request.ExperienceYears ?? 0,
                Specialties = ParseSpecialties(request.Specialties),
                // Payment is out of scope: the profile is visible for a trial period.
                SubscriptionEndsAt = DateTime.UtcNow.AddDays(subscription.Value.TrialDays)
            };
        }

        if (await users.ExistsAsync(email, phone, ct)) return Conflict(ApiError.DuplicateAccount);

        var user = new UserRow
        {
            FullName = request.FullName.Trim(),
            Email = email,
            Phone = phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            GovernorateId = request.GovernorateId
        };

        try
        {
            user.Id = await users.CreateAsync(user, artisan, ct);
        }
        catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
        {
            return Conflict(ApiError.DuplicateAccount);
        }
        catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.NoReferencedRow2)
        {
            return BadRequest(ApiError.InvalidInput);
        }

        return StatusCode(StatusCodes.Status201Created, ToResponse(user));
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        var identifier = request.Identifier.Trim();
        var isEmail = identifier.Contains('@');
        var user = await users.FindByIdentifierAsync(
            isEmail ? identifier.ToLowerInvariant() : null,
            isEmail ? null : PhoneNumber.Normalize(identifier), ct);

        // Same answer for "unknown user" and "wrong password" (no account enumeration).
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(ApiError.InvalidCredentials);

        return Ok(ToResponse(user));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var user = await users.FindByIdAsync(User.UserId(), ct);
        return user is null ? Unauthorized() : Ok(new AuthUserDto(user.Id, user.FullName, user.Role));
    }

    private AuthResponse ToResponse(UserRow user) =>
        new(tokens.Create(user), new AuthUserDto(user.Id, user.FullName, user.Role));

    private static List<string> ParseSpecialties(string? raw) =>
        (raw ?? "")
            .Split(new[] { ',', '،', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => s.Length > 60 ? s[..60] : s)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .ToList();
}
