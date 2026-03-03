using SharedKernel;

namespace Identity.Domain.Users;

public sealed record UserRegisteredEvent(UserId UserId, Email Email) : DomainEvent;

public sealed record UserLoggedInEvent(UserId UserId, Email Email) : DomainEvent;

public sealed record UserPasswordChangedEvent(UserId UserId, Email Email) : DomainEvent;

public sealed record UserPasswordResetRequestedEvent(UserId UserId, Email Email, string ResetToken) : DomainEvent;

public sealed record UserPasswordResetCompletedEvent(UserId UserId, Email Email) : DomainEvent;

public sealed record UserDeactivatedEvent(UserId UserId, Email Email) : DomainEvent;

// ── E-Mail-Verifizierung ──────────────────────────────────────────────────────

public sealed record UserEmailVerificationRequestedEvent(UserId UserId, Email Email, string VerificationToken) : DomainEvent;

public sealed record UserEmailVerifiedEvent(UserId UserId, Email Email) : DomainEvent;

// ── Zwei-Faktor-Authentifizierung ─────────────────────────────────────────────

public sealed record UserTwoFactorEnabledEvent(UserId UserId, Email Email) : DomainEvent;
