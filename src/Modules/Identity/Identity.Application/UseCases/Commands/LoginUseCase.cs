using Identity.Application.DTOs;
using Identity.Application.Ports;
using Identity.Domain.Users;
using SharedKernel;
using MediatR;
using static SharedKernel.Debug;

namespace Identity.Application.UseCases.Commands;

public sealed record LoginCommand(LoginDto Data) : Command<AuthenticationResult>;

public sealed class LoginHandler : IRequestHandler<LoginCommand, Result<AuthenticationResult>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public LoginHandler(
        IUserRepository userRepository,
        ITokenService tokenService,
        IIdentityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthenticationResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        Debug.Log("=== LoginHandler Debug ===");
        Debug.Log($"Looking for user with email: {request.Data.Email}");
        Debug.Log($"Password length: {request.Data.Password?.Length ?? 0}");

        var emailResult = Email.Create(request.Data.Email);
        if (emailResult.IsFailure)
        {
            Debug.Log("Invalid email format!");
            return Result.Failure<AuthenticationResult>(UserErrors.InvalidCredentials);
        }
        
        Debug.Log($"Created Email object: {emailResult.Value}");
        Debug.Log("Searching user in database...");
        
        var user = await _userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        Debug.Log($"Database query completed. User found: {user != null}");
        
        if (user == null)
        {
            Debug.Log("ERROR: User not found in database!");
            return Result.Failure<AuthenticationResult>(UserErrors.InvalidCredentials);
        }

        Debug.Log($"User found: {user.Email}");
        Debug.Log($"User ID: {user.Id}");
        Debug.Log($"User is active: {user.IsActive}");

        if (!user.IsActive)
        {
            Debug.Log("ERROR: User is inactive!");
            return Result.Failure<AuthenticationResult>(UserErrors.UserInactive);
        }

        Debug.Log($"Stored password hash length: {user.PasswordHash.Value?.Length ?? 0}");
        Debug.Log($"Provided password: '{request.Data.Password}'");

        var passwordValid = user.VerifyPassword(request.Data.Password);
        Debug.Log($"Password verification result: {passwordValid}");

        if (!passwordValid)
        {
            Debug.Log("ERROR: Password verification failed!");
            return Result.Failure<AuthenticationResult>(UserErrors.InvalidCredentials);
        }

        // Record login
        user.RecordLogin();
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate token
        var token = _tokenService.GenerateToken(user);

        var result = new AuthenticationResult(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            token,
            DateTime.UtcNow.AddDays(7)); // Token expires in 7 days

        return Result.Success(result);
    }
}
