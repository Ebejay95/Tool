using Identity.Application.DTOs;
using Identity.Application.Ports;
using Identity.Domain.Users;
using SharedKernel;
using MediatR;

namespace Identity.Application.UseCases.Commands;

// ── Verifizierungs-E-Mail erneut anfordern ────────────────────────────────────

public sealed record ResendVerificationEmailCommand(ResendVerificationEmailDto Data) : Command;

public sealed class ResendVerificationEmailHandler : IRequestHandler<ResendVerificationEmailCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public ResendVerificationEmailHandler(IUserRepository userRepository, IIdentityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ResendVerificationEmailCommand request, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(request.Data.Email);
        if (emailResult.IsFailure)
            return Result.Success(); // Keine Existenz offenbaren

        var user = await _userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (user == null || !user.IsActive)
            return Result.Success(); // Keine Existenz offenbaren

        if (user.IsEmailVerified)
            return Result.Success(); // Leise ignorieren

        var result = user.RequestEmailVerification();
        if (result.IsFailure)
            return result; // Gibt UserErrors.EmailVerificationTooSoon zurück

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

// ── E-Mail-Adresse bestätigen ─────────────────────────────────────────────────

public sealed record VerifyEmailCommand(VerifyEmailDto Data) : Command;

public sealed class VerifyEmailHandler : IRequestHandler<VerifyEmailCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public VerifyEmailHandler(IUserRepository userRepository, IIdentityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(request.Data.Email);
        if (emailResult.IsFailure)
            return Result.Failure(UserErrors.InvalidEmailVerificationToken);

        var user = await _userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (user == null)
            return Result.Failure(UserErrors.InvalidEmailVerificationToken);

        var result = user.VerifyEmail(request.Data.Token);
        if (result.IsFailure)
            return result;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
