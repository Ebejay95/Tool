namespace Todos.Application.DTOs;

/// <summary>
/// DTO-Kopien der Domain-Enums für den Application-/Client-Layer.
///
/// Begründung: Client und Application dürfen nicht direkt von Todos.Domain
/// abhängen. Diese Enums sind strukturell identisch mit den Domain-Enums
/// (gleiche Integer-Werte) → einfaches (int)-Casting bei Mappings möglich.
/// </summary>
public enum TodoStatus
{
    Pending    = 0,
    InProgress = 1,
    Completed  = 2,
    Cancelled  = 3
}

public enum TodoPriority
{
    Low      = 0,
    Medium   = 1,
    High     = 2,
    Critical = 3
}
