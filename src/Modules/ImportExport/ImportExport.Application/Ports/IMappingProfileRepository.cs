using SharedKernel;
using ImportExport.Domain.Profiles;

namespace ImportExport.Application.Ports;

public interface IMappingProfileRepository
{
    Task<MappingProfile?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<MappingProfile>> GetByUserAndTypeAsync(UserId userId, string entityTypeName, CancellationToken ct = default);
    Task<IReadOnlyList<MappingProfile>> GetByUserAsync(UserId userId, CancellationToken ct = default);
    void Add(MappingProfile profile);
    void Remove(MappingProfile profile);
}

public interface IImportExportUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
