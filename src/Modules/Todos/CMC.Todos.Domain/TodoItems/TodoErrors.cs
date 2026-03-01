using CMC.SharedKernel;

namespace CMC.Todos.Domain.TodoItems;

public static class TodoErrors
{
    public static readonly Error TitleRequired = new("Todo.TitleRequired", "Title is required");
    public static readonly Error TitleTooLong = new("Todo.TitleTooLong", "Title cannot exceed 200 characters");

    public static readonly Error DescriptionTooLong = new("Todo.DescriptionTooLong", "Description cannot exceed 2000 characters");

    public static readonly Error DueDateInPast = new("Todo.DueDateInPast", "Due date cannot be in the past");

    public static readonly Error NotFound = new("Todo.NotFound", "Todo item was not found");
    public static readonly Error AccessDenied = new("Todo.AccessDenied", "You do not have access to this todo item");

    public static readonly Error CannotChangeCompletedTodo = new("Todo.CannotChangeCompleted", "Cannot change a completed todo item");
    public static readonly Error CannotChangeCancelledTodo = new("Todo.CannotChangeCancelled", "Cannot change a cancelled todo item");
}
