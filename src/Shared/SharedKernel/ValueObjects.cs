namespace SharedKernel;

public abstract record ValueObject;

public sealed record Email : ValueObject
{
    private Email(string value) => Value = value;

    public string Value { get; }

    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<Email>(new Error("Email.Empty", "Email cannot be empty"));

        if (!email.Contains('@') || !email.Contains('.'))
            return Result.Failure<Email>(new Error("Email.Invalid", "Email format is invalid"));

        return Result.Success(new Email(email.Trim().ToLowerInvariant()));
    }

    public static implicit operator string(Email email) => email.Value;
}

public sealed record UserId : ValueObject
{
    private UserId(Guid value) => Value = value;

    public Guid Value { get; }

    public static UserId New() => new(Guid.NewGuid());
    public static UserId From(Guid value) => new(value);

    public static implicit operator Guid(UserId userId) => userId.Value;
}

public sealed record TodoId : ValueObject
{
    private TodoId(Guid value) => Value = value;

    public Guid Value { get; }

    public static TodoId New() => new(Guid.NewGuid());
    public static TodoId From(Guid value) => new(value);

    public static implicit operator Guid(TodoId todoId) => todoId.Value;
}
