using Identity.Application.DTOs;
using Identity.Application.Ports;
using Identity.Domain.Users;
using SharedKernel;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Identity.Application.UseCases.Commands;

// ── 2FA-Setup initiieren ──────────────────────────────────────────────────────

/// <summary>
/// Initiiert den 2FA-Setup-Prozess. Erfordert einen PreAuth-Token mit Stage "2fa_setup".
/// Gibt Secret-Key und QR-Code (Base64-PNG) zurück.
/// </summary>
public sealed record InitiateTwoFactorSetupCommand(string PreAuthToken) : Command<TwoFactorSetupDto>;

public sealed class InitiateTwoFactorSetupHandler
    : IRequestHandler<InitiateTwoFactorSetupCommand, Result<TwoFactorSetupDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly ITotpService _totpService;
    private readonly IIdentityUnitOfWork _unitOfWork;
    private readonly TotpOptions _totpOptions;
    private readonly ILogger<InitiateTwoFactorSetupHandler> _logger;

    public InitiateTwoFactorSetupHandler(
        IUserRepository userRepository,
        ITokenService tokenService,
        ITotpService totpService,
        IIdentityUnitOfWork unitOfWork,
        IOptions<TotpOptions> totpOptions,
        ILogger<InitiateTwoFactorSetupHandler> logger)
    {
        _userRepository = userRepository;
        _tokenService   = tokenService;
        _totpService    = totpService;
        _unitOfWork     = unitOfWork;
        _totpOptions    = totpOptions.Value;
        _logger         = logger;
    }

    public async Task<Result<TwoFactorSetupDto>> Handle(
        InitiateTwoFactorSetupCommand request, CancellationToken cancellationToken)
    {
        var tokenPreview = request.PreAuthToken.Length > 20
            ? request.PreAuthToken[..20] + "…"
            : request.PreAuthToken;

        _logger.LogDebug("setup-2fa/init: Validiere PreAuthToken ({Preview}) – ServerUtcNow={Now}",
            tokenPreview, DateTime.UtcNow);

        var preAuthResult = _tokenService.ValidatePreAuthToken(request.PreAuthToken);
        if (preAuthResult.IsFailure)
        {
            _logger.LogWarning("setup-2fa/init: Token-Validierung fehlgeschlagen – {Code}: {Message}",
                preAuthResult.Error.Code, preAuthResult.Error.Description);
            return Result.Failure<TwoFactorSetupDto>(preAuthResult.Error);
        }

        _logger.LogDebug("setup-2fa/init: Token gültig, Stage={Stage}, UserId={UserId}",
            preAuthResult.Value.Stage, preAuthResult.Value.UserId.Value);

        if (preAuthResult.Value.Stage != PreAuthStages.TwoFactorSetup)
        {
            _logger.LogWarning("setup-2fa/init: Ungültige Stage – erwartet '{Expected}', erhalten '{Actual}'",
                PreAuthStages.TwoFactorSetup, preAuthResult.Value.Stage);
            return Result.Failure<TwoFactorSetupDto>(new Error("Auth.InvalidStage", "Invalid pre-auth token stage"));
        }

        var user = await _userRepository.GetByIdAsync(preAuthResult.Value.UserId.Value, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("setup-2fa/init: User {UserId} nicht gefunden", preAuthResult.Value.UserId.Value);
            return Result.Failure<TwoFactorSetupDto>(UserErrors.UserNotFound);
        }

        _logger.LogDebug("setup-2fa/init: User gefunden – IsTwoFactorEnabled={Enabled}", user.IsTwoFactorEnabled);

        var secret       = _totpService.GenerateSecret();
        var otpAuthUri   = _totpService.GetOtpAuthUri(secret, user.Email, _totpOptions.Issuer);
        var qrCodeBase64 = _totpService.GenerateQrCodeBase64(otpAuthUri);

        var domainResult = user.InitiateTwoFactorSetup(secret);
        if (domainResult.IsFailure)
        {
            _logger.LogWarning("setup-2fa/init: Domain-Fehler – {Code}: {Message}",
                domainResult.Error.Code, domainResult.Error.Description);
            return Result.Failure<TwoFactorSetupDto>(domainResult.Error);
        }

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("setup-2fa/init: Setup erfolgreich initiiert für User {UserId}", user.Id.Value);

        return Result.Success(new TwoFactorSetupDto
        {
            SecretKey    = secret,
            QrCodeBase64 = qrCodeBase64,
            OtpAuthUri   = otpAuthUri
        });
    }
}

// ── 2FA-Setup bestätigen ──────────────────────────────────────────────────────

/// <summary>
/// Bestätigt die 2FA-Einrichtung mit dem ersten Code aus der Authenticator-App.
/// Bei Erfolg wird ein vollständiges JWT ausgestellt.
/// </summary>
public sealed record ConfirmTwoFactorSetupCommand(ConfirmTwoFactorDto Data) : Command<LoginResponseDto>;

public sealed class ConfirmTwoFactorSetupHandler
    : IRequestHandler<ConfirmTwoFactorSetupCommand, Result<LoginResponseDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly ITotpService _totpService;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public ConfirmTwoFactorSetupHandler(
        IUserRepository userRepository,
        ITokenService tokenService,
        ITotpService totpService,
        IIdentityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _tokenService   = tokenService;
        _totpService    = totpService;
        _unitOfWork     = unitOfWork;
    }

    public async Task<Result<LoginResponseDto>> Handle(
        ConfirmTwoFactorSetupCommand request, CancellationToken cancellationToken)
    {
        var preAuthResult = _tokenService.ValidatePreAuthToken(request.Data.PreAuthToken);
        if (preAuthResult.IsFailure)
            return Result.Failure<LoginResponseDto>(preAuthResult.Error);

        if (preAuthResult.Value.Stage != PreAuthStages.TwoFactorSetup)
            return Result.Failure<LoginResponseDto>(new Error("Auth.InvalidStage", "Invalid pre-auth token stage"));

        var user = await _userRepository.GetByIdAsync(preAuthResult.Value.UserId.Value, cancellationToken);
        if (user == null)
            return Result.Failure<LoginResponseDto>(UserErrors.UserNotFound);

        if (string.IsNullOrEmpty(user.TwoFactorPendingSecret))
            return Result.Failure<LoginResponseDto>(UserErrors.TwoFactorNotConfigured);

        var codeValid = _totpService.ValidateCode(user.TwoFactorPendingSecret, request.Data.Code);

        var domainResult = user.ConfirmTwoFactorSetup(codeValid);
        if (domainResult.IsFailure)
            return Result.Failure<LoginResponseDto>(domainResult.Error);

        // Login abschließen
        user.RecordLogin();
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var generated = _tokenService.GenerateToken(user);
        return Result.Success(LoginHandler.BuildCompleteResult(user, generated));
    }
}

// ── 2FA validieren (Login-Schritt) ────────────────────────────────────────────

/// <summary>
/// Validiert den TOTP-Code während des Login-Vorgangs.
/// Bei Erfolg wird ein vollständiges JWT ausgestellt.
/// </summary>
public sealed record ValidateTwoFactorCommand(ValidateTwoFactorDto Data) : Command<LoginResponseDto>;

public sealed class ValidateTwoFactorHandler
    : IRequestHandler<ValidateTwoFactorCommand, Result<LoginResponseDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly ITotpService _totpService;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public ValidateTwoFactorHandler(
        IUserRepository userRepository,
        ITokenService tokenService,
        ITotpService totpService,
        IIdentityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _tokenService   = tokenService;
        _totpService    = totpService;
        _unitOfWork     = unitOfWork;
    }

    public async Task<Result<LoginResponseDto>> Handle(
        ValidateTwoFactorCommand request, CancellationToken cancellationToken)
    {
        var preAuthResult = _tokenService.ValidatePreAuthToken(request.Data.PreAuthToken);
        if (preAuthResult.IsFailure)
            return Result.Failure<LoginResponseDto>(preAuthResult.Error);

        if (preAuthResult.Value.Stage != PreAuthStages.TwoFactorValidation)
            return Result.Failure<LoginResponseDto>(new Error("Auth.InvalidStage", "Invalid pre-auth token stage"));

        var user = await _userRepository.GetByIdAsync(preAuthResult.Value.UserId.Value, cancellationToken);
        if (user == null)
            return Result.Failure<LoginResponseDto>(UserErrors.UserNotFound);

        if (!user.IsTwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
            return Result.Failure<LoginResponseDto>(UserErrors.TwoFactorNotConfigured);

        if (!_totpService.ValidateCode(user.TwoFactorSecret, request.Data.Code))
            return Result.Failure<LoginResponseDto>(UserErrors.InvalidTwoFactorCode);

        // Login abschließen
        user.RecordLogin();
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var generated = _tokenService.GenerateToken(user);
        return Result.Success(LoginHandler.BuildCompleteResult(user, generated));
    }
}

// ── Konfiguration ─────────────────────────────────────────────────────────────

/// <summary>Konfigurationsoptionen für den TOTP-Dienst.</summary>
public sealed class TotpOptions
{
    /// <summary>Name der App wie er in der Authenticator-App angezeigt wird.</summary>
    public string Issuer { get; set; } = "CMC";
}
