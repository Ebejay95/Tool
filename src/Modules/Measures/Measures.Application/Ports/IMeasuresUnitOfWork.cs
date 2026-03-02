using SharedKernel;

namespace Measures.Application.Ports;

/// <summary>
/// Measures module specific Unit of Work interface.
/// This avoids conflicts with other modules' UnitOfWork implementations.
/// </summary>
public interface IMeasuresUnitOfWork : IUnitOfWork
{
    // Inherits all methods from IUnitOfWork but is module-specific
}
