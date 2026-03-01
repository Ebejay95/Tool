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

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserDto dto, CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(dto);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });
        }

        return Ok(result.Value);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto, CancellationToken cancellationToken)
    {
    Debug.Log("=== AuthController.Login Debug ===");
    Debug.Log($"Received Email: {dto.Email}");
    Debug.Log($"Password provided: {!string.IsNullOrEmpty(dto.Password)}");

        var command = new LoginCommand(dto);
        var result = await _mediator.Send(command, cancellationToken);
        Debug.Log($"Login result success: {result.IsSuccess}");

        if (result.IsFailure)
        {
            Debug.Log($"Login error: {result.Error}");

            return BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });
        }

        return Ok(result.Value);
    }

    [HttpPost("request-password-reset")]
    public async Task<IActionResult> RequestPasswordReset(RequestPasswordResetDto dto, CancellationToken cancellationToken)
    {
        var command = new RequestPasswordResetCommand(dto);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });
        }

        return Ok(new { Message = "If the email exists, a reset code has been sent." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto, CancellationToken cancellationToken)
    {
        var command = new ResetPasswordCommand(dto);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { Error = result.Error.Code, Message = result.Error.Description });
        }

        return Ok(new { Message = "Password reset successfully." });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        // For JWT tokens, logout is handled client-side by removing the token
        // Here we could implement token blacklisting if needed
        return Ok(new { Message = "Logged out successfully." });
    }
}
