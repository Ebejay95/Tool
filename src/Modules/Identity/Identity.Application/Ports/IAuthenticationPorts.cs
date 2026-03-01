using Identity.Domain.Users;
using SharedKernel;

namespace Identity.Application.Ports;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithEmailAsync(Email email, CancellationToken cancellationToken = default);
}

public interface IAuthenticationService
{
    Task<Result<AuthenticationResult>> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<Result> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    Task SignOutAsync(string userId, CancellationToken cancellationToken = default);
}

public sealed record AuthenticationResult(
    UserId UserId,
    Email Email,
    string FirstName,
    string LastName,
    string Token,
    DateTime ExpiresAt);

public interface ITokenService
{
    string GenerateToken(User user);
    Result<UserId> ValidateToken(string token);
}
