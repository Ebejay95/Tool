using Identity.Application.DTOs;
using Identity.Application.Ports;
using Identity.Domain.Users;
using SharedKernel;
using MediatR;

namespace Identity.Application.UseCases.Commands;

// ── Get all users (master only) ──────────────────────────────────────────────

public sealed record GetUsersQuery : IRequest<Result<IReadOnlyList<UserManagementItemDto>>>;

public sealed class GetUsersHandler
    : IRequestHandler<GetUsersQuery, Result<IReadOnlyList<UserManagementItemDto>>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<IReadOnlyList<UserManagementItemDto>>> Handle(
        GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllForManagementAsync(cancellationToken);

        var dtos = users
            .Select(u => new UserManagementItemDto(
                u.Id.Value.ToString(),
                u.Email,
                u.FirstName,
                u.LastName,
                u.FullName,
                u.Role,
                u.IsActive,
                u.CreatedAt))
            .ToList();

        return Result.Success<IReadOnlyList<UserManagementItemDto>>(dtos);
    }
}

// ── Update user role (master only) ──────────────────────────────────────────

public sealed record UpdateUserRoleCommand(Guid UserId, string Role)
    : IRequest<Result>;

public sealed class UpdateUserRoleHandler : IRequestHandler<UpdateUserRoleCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public UpdateUserRoleHandler(IUserRepository userRepository, IIdentityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        var result = user.SetRole(request.Role);
        if (result.IsFailure)
            return result;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
