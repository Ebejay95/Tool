using Identity.Application.DTOs;
using Identity.Application.Ports;
using Identity.Domain.Users;
using SharedKernel;
using MediatR;

namespace Identity.Application.UseCases.Commands;

public sealed record RegisterUserCommand(RegisterUserDto Data) : Command<UserDto>;

public sealed class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public RegisterUserHandler(IUserRepository userRepository, IIdentityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(request.Data.Email);
        if (emailResult.IsFailure)
            return Result.Failure<UserDto>(emailResult.Error);

        var email = emailResult.Value;

        if (await _userRepository.ExistsWithEmailAsync(email, cancellationToken))
            return Result.Failure<UserDto>(UserErrors.EmailAlreadyExists);

        var userResult = User.Create(
            email,
            request.Data.Password,
            request.Data.FirstName,
            request.Data.LastName);

        if (userResult.IsFailure)
            return Result.Failure<UserDto>(userResult.Error);

        var user = userResult.Value;
        _userRepository.Add(user);

        // Verifizierungs-E-Mail sofort beim Registrieren anfordern
        user.RequestEmailVerification();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userDto = new UserDto(
            user.Id.Value.ToString(),
            user.Email,
            user.FirstName,
            user.LastName,
            user.FullName,
            user.CreatedAt,
            user.LastLoginAt,
            user.IsActive);

        return Result.Success(userDto);
    }
}
