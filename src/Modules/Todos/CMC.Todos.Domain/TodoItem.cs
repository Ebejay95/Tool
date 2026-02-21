namespace CMC.Todos.Domain;

public sealed class TodoItem
{
    public Guid Id { get; set; }
    public string OwnerUserId { get; set; } = "";
    public string Title { get; set; } = "";
    public bool IsCompleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
