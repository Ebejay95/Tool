using SharedKernel;

namespace Todos.Application.Ports;

/// <summary>
/// Todos module specific Unit of Work interface.
/// This avoids conflicts with other modules' UnitOfWork implementations.
/// </summary>
public interface ITodosUnitOfWork : IUnitOfWork
{
    // Inherits all methods from IUnitOfWork but is module-specific
}