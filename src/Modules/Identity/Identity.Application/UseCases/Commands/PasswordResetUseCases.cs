using Identity.Application.DTOs;
using Identity.Application.Ports;
using Identity.Domain.Users;
using SharedKernel;
using MediatR;

namespace Identity.Application.UseCases.Commands;

public sealed record RequestPasswordResetCommand(RequestPasswordResetDto Data) : Command;

public sealed class RequestPasswordResetHandler : IRequestHandler<RequestPasswordResetCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RequestPasswordResetHandler(IUserRepository userRepository, IIdentityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(request.Data.Email);
        if (emailResult.IsFailure)
            return Result.Success(); // Don't reveal if email exists

        var user = await _userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (user == null)
            return Result.Success(); // Don't reveal if email exists

        if (!user.IsActive)
            return Result.Success(); // Don't reveal if user is inactive

        var result = user.InitiatePasswordReset();
        if (result.IsFailure)
            return result;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed record ResetPasswordCommand(ResetPasswordDto Data) : Command;

public sealed class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public ResetPasswordHandler(IUserRepository userRepository, IIdentityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(request.Data.Email);
        if (emailResult.IsFailure)
            return Result.Failure(UserErrors.UserNotFound);

        var user = await _userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (user == null)
            return Result.Failure(UserErrors.UserNotFound);

        var result = user.ResetPassword(request.Data.Token, request.Data.NewPassword);
        if (result.IsFailure)
            return result;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed record ChangePasswordCommand(UserId UserId, ChangePasswordDto Data) : Command;

public sealed class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public ChangePasswordHandler(IUserRepository userRepository, IIdentityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId.Value, cancellationToken);
        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        var result = user.ChangePassword(request.Data.CurrentPassword, request.Data.NewPassword);
        if (result.IsFailure)
            return result;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
