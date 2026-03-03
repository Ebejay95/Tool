using Identity.Application.Ports;
using Identity.Domain.Users;
using SharedKernel;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity.Infrastructure.Services;

public sealed class JwtTokenOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationDays { get; set; } = 7;
}

public sealed class JwtTokenService : ITokenService
{
    private const string PreAuthClaim   = "auth_stage";
    private const string PreAuthAudience = "pre_auth";
    private const int    PreAuthMinutes  = 10;

    private readonly JwtTokenOptions _options;
    private readonly SymmetricSecurityKey _key;

    public JwtTokenService(IOptions<JwtTokenOptions> options)
    {
        _options = options.Value;
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
    }

    // ── Vollständiges JWT ─────────────────────────────────────────────────────

    public GeneratedToken GenerateToken(User user)
    {
        var expiresAt = DateTime.UtcNow.AddDays(_options.ExpirationDays);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.Value.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new GeneratedToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public Result<UserId> ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.MapInboundClaims = false;
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _key,
                ValidateIssuer = true,
                ValidIssuer = _options.Issuer,
                ValidateAudience = true,
                ValidAudience = _options.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

            var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Result.Failure<UserId>(new Error("Token.InvalidUserId", "Invalid user ID in token"));

            return Result.Success(UserId.From(userId));
        }
        catch (SecurityTokenExpiredException)
        {
            return Result.Failure<UserId>(new Error("Token.Expired", "Token has expired"));
        }
        catch (SecurityTokenException)
        {
            return Result.Failure<UserId>(new Error("Token.Invalid", "Invalid token"));
        }
        catch (Exception)
        {
            return Result.Failure<UserId>(new Error("Token.ValidationFailed", "Token validation failed"));
        }
    }

    // ── Pre-Auth-JWT (kurzlebig, nur für 2FA-Schritte) ───────────────────────

    public GeneratedToken GeneratePreAuthToken(UserId userId, string stage)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(PreAuthMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.Value.ToString()),
            new Claim(PreAuthClaim, stage),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: PreAuthAudience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new GeneratedToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public Result<PreAuthClaims> ValidatePreAuthToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.MapInboundClaims = false;
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _key,
                ValidateIssuer = true,
                ValidIssuer = _options.Issuer,
                ValidateAudience = true,
                ValidAudience = PreAuthAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

            var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Result.Failure<PreAuthClaims>(new Error("Token.InvalidUserId", "Invalid user ID in token"));

            var stageClaim = principal.FindFirst(PreAuthClaim)?.Value;
            if (string.IsNullOrEmpty(stageClaim))
                return Result.Failure<PreAuthClaims>(new Error("Token.NotPreAuth", "Token is not a pre-auth token"));

            return Result.Success(new PreAuthClaims(UserId.From(userId), stageClaim));
        }
        catch (SecurityTokenExpiredException)
        {
            return Result.Failure<PreAuthClaims>(new Error("Token.Expired", "Pre-auth token has expired"));
        }
        catch (SecurityTokenException)
        {
            return Result.Failure<PreAuthClaims>(new Error("Token.Invalid", "Invalid pre-auth token"));
        }
        catch (Exception)
        {
            return Result.Failure<PreAuthClaims>(new Error("Token.ValidationFailed", "Token validation failed"));
        }
    }
}
