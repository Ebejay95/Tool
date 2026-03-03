using SharedKernel;

namespace Identity.Domain.Users;

public sealed record HashedPassword : ValueObject
{
    private HashedPassword(string value) => Value = value;

    public string Value { get; }

    public static HashedPassword Create(string plainTextPassword)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(plainTextPassword, 12);
        return new HashedPassword(hash);
    }

    public static HashedPassword FromHash(string hash)
    {
        return new HashedPassword(hash);
    }

    public bool Verify(string plainTextPassword)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(plainTextPassword, Value);
        }
        catch
        {
            return false;
        }
    }

    public static implicit operator string(HashedPassword hashedPassword) => hashedPassword.Value;
}
