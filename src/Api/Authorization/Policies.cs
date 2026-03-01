using SharedKernel;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Api.Authorization;

public static class Policies
{
    public const string RequireAuthenticatedUser = "RequireAuthenticatedUser";
    public const string RequireOwnership = "RequireOwnership";
}

public sealed class OwnershipRequirement : IAuthorizationRequirement { }

/// <summary>
/// Prüft ob der eingeloggte User der Owner der Ressource ist.
/// Verwendung im Controller: _authorizationService.AuthorizeAsync(User, resource, Policies.RequireOwnership)
/// Ressource muss IResourceOwner implementieren.
/// </summary>
public sealed class OwnershipHandler : AuthorizationHandler<OwnershipRequirement, IResourceOwner>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OwnershipRequirement requirement,
        IResourceOwner resource)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId != null && userId == resource.OwnerId)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
