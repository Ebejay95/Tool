namespace CMC.Todos.Application;

public sealed record TodoDto(Guid Id, string Title, bool IsCompleted, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
