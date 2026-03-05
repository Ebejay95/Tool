using Identity.Application.DTOs;
using Identity.Application.Ports;
using Identity.Domain.Users;
using SharedKernel;
using MediatR;

namespace Identity.Application.UseCases.Commands;

public sealed record LoginCommand(LoginDto Data) : Command<LoginResponseDto>;

public sealed class LoginHandler : IRequestHandler<LoginCommand, Result<LoginResponseDto>>
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

    public async Task<Result<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(request.Data.Email);
        if (emailResult.IsFailure)
            return Result.Failure<LoginResponseDto>(UserErrors.InvalidCredentials);

        var user = await _userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (user == null)
            return Result.Failure<LoginResponseDto>(UserErrors.InvalidCredentials);

        if (!user.IsActive)
            return Result.Failure<LoginResponseDto>(UserErrors.UserInactive);

        var passwordValid = user.VerifyPassword(request.Data.Password);
        if (!passwordValid)
            return Result.Failure<LoginResponseDto>(UserErrors.InvalidCredentials);

        // ── Master-Rolle: Direkter Login ohne E-Mail-Verifizierung und 2FA ───
        if (user.Role == UserRoles.Master)
        {
            user.RecordLogin();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            var masterToken = _tokenService.GenerateToken(user);
            return Result.Success(LoginHandler.BuildCompleteResult(user, masterToken));
        }

        // ── Schritt 1: E-Mail-Verifizierung prüfen ───────────────────────────
        if (!user.IsEmailVerified)
        {
            return Result.Success(new LoginResponseDto
            {
                Stage = LoginStage.RequiresEmailVerification,
                Email = user.Email
            });
        }

        // ── Schritt 2: 2FA prüfen ────────────────────────────────────────────
        if (!user.IsTwoFactorEnabled)
        {
            // 2FA noch nicht eingerichtet → Pre-Auth-Token für Setup ausstellen
            var preAuth = _tokenService.GeneratePreAuthToken(user.Id, PreAuthStages.TwoFactorSetup);
            return Result.Success(new LoginResponseDto
            {
                Stage        = LoginStage.RequiresTwoFactorSetup,
                Email        = user.Email,
                PreAuthToken = preAuth.Token
            });
        }

        // 2FA eingerichtet → Pre-Auth-Token für Validation ausstellen
        var preAuthValidation = _tokenService.GeneratePreAuthToken(user.Id, PreAuthStages.TwoFactorValidation);
        return Result.Success(new LoginResponseDto
        {
            Stage        = LoginStage.RequiresTwoFactorValidation,
            Email        = user.Email,
            PreAuthToken = preAuthValidation.Token
        });
    }

    /// <summary>Stellt nach erfolgreicher 2FA das vollständige JWT aus. Intern von 2FA-Handlern genutzt.</summary>
    public static LoginResponseDto BuildCompleteResult(User user, GeneratedToken generated) =>
        new()
        {
            Stage     = LoginStage.Complete,
            UserId    = user.Id.Value.ToString(),
            Email     = user.Email,
            FirstName = user.FirstName,
            LastName  = user.LastName,
            Token     = generated.Token,
            ExpiresAt = generated.ExpiresAt
        };
}
