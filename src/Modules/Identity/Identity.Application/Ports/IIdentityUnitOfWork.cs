using SharedKernel;

namespace Identity.Application.Ports;

/// <summary>
/// Identity module specific Unit of Work interface.
/// This avoids conflicts with other modules' UnitOfWork implementations.
/// </summary>
public interface IIdentityUnitOfWork : IUnitOfWork
{
    // Inherits all methods from IUnitOfWork but is module-specific
}