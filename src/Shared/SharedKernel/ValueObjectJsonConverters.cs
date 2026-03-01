using System.Text.Json;
using System.Text.Json.Serialization;

namespace SharedKernel;

/// <summary>
/// Generische Basisklasse für Guid-basierte Value Objects.
/// Neues VO: eine Zeile ableiten, fertig.
/// </summary>
public abstract class GuidValueObjectConverter<T>(Func<Guid, T> factory, Func<T, Guid> getter)
    : JsonConverter<T> where T : ValueObject
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => factory(reader.GetGuid());

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        => writer.WriteStringValue(getter(value));
}

// --- Konkrete Converter ---

public sealed class UserIdJsonConverter()
    : GuidValueObjectConverter<UserId>(UserId.From, id => id.Value);

public sealed class TodoIdJsonConverter()
    : GuidValueObjectConverter<TodoId>(TodoId.From, id => id.Value);

public sealed class EmailJsonConverter : JsonConverter<Email>
{
    public override Email Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var raw = reader.GetString() ?? string.Empty;
        var result = Email.Create(raw);
        return result.IsSuccess
            ? result.Value
            : throw new JsonException($"Ungültiger Email-Wert '{raw}'.");
    }

    public override void Write(Utf8JsonWriter writer, Email value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);
}
