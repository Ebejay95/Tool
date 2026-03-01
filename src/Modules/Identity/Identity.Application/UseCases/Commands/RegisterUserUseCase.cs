using Identity.Application.DTOs;
using Identity.Application.Ports;
using Identity.Domain.Users;
using SharedKernel;
using MediatR;
using static SharedKernel.Debug;

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
        Debug.Log("=== RegisterUserHandler Debug ===");
        Debug.Log($"Registering user: {request.Data.Email}");
        Debug.Log($"Raw password length: {request.Data.Password?.Length ?? 0}");

        var emailResult = Email.Create(request.Data.Email);
        if (emailResult.IsFailure)
            return Result.Failure<UserDto>(emailResult.Error);

        var email = emailResult.Value;

        // Check if user already exists
        if (await _userRepository.ExistsWithEmailAsync(email, cancellationToken))
        {
            Debug.Log("ERROR: User already exists!");
            return Result.Failure<UserDto>(UserErrors.EmailAlreadyExists);
        }

        // Create user
        Debug.Log("Creating user...");
        var userResult = User.Create(
            email,
            request.Data.Password,
            request.Data.FirstName,
            request.Data.LastName);

        if (userResult.IsFailure)
        {
            Debug.Log($"User creation failed: {userResult.Error}");
            return Result.Failure<UserDto>(userResult.Error);
        }

        var user = userResult.Value;
        Debug.Log($"User created with ID: {user.Id}");
        Debug.Log($"Password hash length after creation: {user.PasswordHash.Value?.Length ?? 0}");

        Debug.Log("Adding user to repository...");
        _userRepository.Add(user);
        Log($"User added to repository. Repository type: {_userRepository.GetType().Name}");
        
        Log("Saving changes to database...");
        Log($"UnitOfWork type: {_unitOfWork.GetType().Name}");
        
        try 
        {
            var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);
            Log($"SUCCESS: SaveChangesAsync completed! Changes saved: {saveResult}");
            
            // Verify user was actually saved by trying to retrieve it
            Log("Verifying user was saved by querying database...");
            var emailObj = Email.Create(user.Email);
            if (emailObj.IsSuccess)
            {
                var savedUser = await _userRepository.GetByEmailAsync(emailObj.Value, cancellationToken);
                Log($"Verification query result: User found = {savedUser != null}");
                if (savedUser != null)
                {
                    Log($"Saved user ID: {savedUser.Id}, Email: {savedUser.Email}");
                }
            }
        }
        catch (Exception ex)
        {
            Log($"ERROR: Failed to save user to database: {ex.Message}");
            Log($"Exception type: {ex.GetType().Name}");
            Log($"Stack trace: {ex.StackTrace}");
            throw;
        }

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
