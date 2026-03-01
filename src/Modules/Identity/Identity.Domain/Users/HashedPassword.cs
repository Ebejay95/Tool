using SharedKernel;

namespace Identity.Domain.Users;

public sealed record HashedPassword : ValueObject
{
    private HashedPassword(string value) => Value = value;

    public string Value { get; }

    public static HashedPassword Create(string plainTextPassword)
    {
        Debug.Log("=== HashedPassword.Create Debug ===");
        Debug.Log($"Input password length: {plainTextPassword?.Length ?? 0}");
        Debug.Log($"Input password: '{plainTextPassword}'");

        var hash = BCrypt.Net.BCrypt.HashPassword(plainTextPassword, 12); // High cost for security

        Debug.Log($"Generated hash length: {hash?.Length ?? 0}");
        Debug.Log($"Generated hash: '{hash}'");

        return new HashedPassword(hash);
    }

    public static HashedPassword FromHash(string hash)
    {
        return new HashedPassword(hash);
    }

    public bool Verify(string plainTextPassword)
    {
        Debug.Log("=== HashedPassword.Verify Debug ===");
        Debug.Log($"Input password: '{plainTextPassword}'");
        Debug.Log($"Input password length: {plainTextPassword?.Length ?? 0}");
        Debug.Log($"Stored hash: '{Value}'");
        Debug.Log($"Stored hash length: {Value?.Length ?? 0}");

        try
        {
            var result = BCrypt.Net.BCrypt.Verify(plainTextPassword, Value);
            Debug.Log($"BCrypt.Verify result: {result}");
            return result;
        }
        catch (Exception ex)
        {
            Debug.Log($"BCrypt.Verify exception: {ex.Message}");
            return false;
        }
    }

    public static implicit operator string(HashedPassword hashedPassword) => hashedPassword.Value;
}
