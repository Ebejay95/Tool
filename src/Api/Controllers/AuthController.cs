using Identity.Application.DTOs;
using Identity.Application.UseCases.Commands;
using SharedKernel;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;

    public AuthController(IMediator mediator, ICurrentUser currentUser)
    {
        _mediator    = mediator;
        _currentUser = currentUser;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserDto dto, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RegisterUserCommand(dto), cancellationToken);
        if (result.IsFailure)
            return BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });
        return Ok(result.Value);
    }

    /// <summary>
    /// Multi-Stage-Login. Stage-Werte im Response:
    /// Complete / RequiresEmailVerification / RequiresTwoFactorSetup / RequiresTwoFactorValidation
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new LoginCommand(dto), cancellationToken);
        if (result.IsFailure)
            return BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });
        return Ok(result.Value);
    }

    // ── E-Mail-Verifizierung ──────────────────────────────────────────────────

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailDto dto, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new VerifyEmailCommand(dto), cancellationToken);
        if (result.IsFailure)
            return BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });
        return Ok(new { Message = "E-Mail-Adresse erfolgreich bestätigt." });
    }

    [HttpPost("resend-verification-email")]
    public async Task<IActionResult> ResendVerificationEmail(
        ResendVerificationEmailDto dto, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ResendVerificationEmailCommand(dto), cancellationToken);

        if (result.IsFailure && result.Error.Code == "User.EmailVerificationTooSoon")
            return BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });

        return Ok(new { Message = "Falls die Adresse existiert und unverifiziert ist, wurde eine neue E-Mail versandt." });
    }

    // ── Zwei-Faktor-Authentifizierung ─────────────────────────────────────────

    [HttpPost("setup-2fa/init")]
    public async Task<IActionResult> InitiateTwoFactorSetup(
        [FromBody] InitTwoFactorSetupRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new InitiateTwoFactorSetupCommand(request.PreAuthToken), cancellationToken);
        if (result.IsFailure)
            return BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });
        return Ok(result.Value);
    }

    [HttpPost("setup-2fa/confirm")]
    public async Task<IActionResult> ConfirmTwoFactorSetup(
        ConfirmTwoFactorDto dto, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ConfirmTwoFactorSetupCommand(dto), cancellationToken);
        if (result.IsFailure)
            return BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });
        return Ok(result.Value);
    }

    [HttpPost("validate-2fa")]
    public async Task<IActionResult> ValidateTwoFactor(
        ValidateTwoFactorDto dto, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ValidateTwoFactorCommand(dto), cancellationToken);
        if (result.IsFailure)
            return BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });
        return Ok(result.Value);
    }

    // ── Passwort ──────────────────────────────────────────────────────────────

    [HttpPost("request-password-reset")]
    public async Task<IActionResult> RequestPasswordReset(
        RequestPasswordResetDto dto, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RequestPasswordResetCommand(dto), cancellationToken);
        if (result.IsFailure)
            return BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });
        return Ok(new { Message = "If the email exists, a reset code has been sent." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ResetPasswordCommand(dto), cancellationToken);
        if (result.IsFailure)
            return BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });
        return Ok(new { Message = "Password reset successfully." });
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout() => Ok(new { Message = "Logged out successfully." });

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Unauthorized();

        var result = await _mediator.Send(new ChangePasswordCommand(_currentUser.UserId, dto), cancellationToken);
        if (result.IsFailure)
            return BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });
        return Ok(new { Message = "Password changed successfully." });
    }
}

public sealed class InitTwoFactorSetupRequest
{
    public string PreAuthToken { get; set; } = string.Empty;
}
