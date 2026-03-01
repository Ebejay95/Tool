using SharedKernel;

namespace Identity.Domain.Users;

public sealed record UserRegisteredEvent(UserId UserId, Email Email) : DomainEvent;

public sealed record UserLoggedInEvent(UserId UserId, Email Email) : DomainEvent;

public sealed record UserPasswordChangedEvent(UserId UserId, Email Email) : DomainEvent;

public sealed record UserPasswordResetRequestedEvent(UserId UserId, Email Email, string ResetToken) : DomainEvent;

public sealed record UserPasswordResetCompletedEvent(UserId UserId, Email Email) : DomainEvent;

public sealed record UserDeactivatedEvent(UserId UserId, Email Email) : DomainEvent;
