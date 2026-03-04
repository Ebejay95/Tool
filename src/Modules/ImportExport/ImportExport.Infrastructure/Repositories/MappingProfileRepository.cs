using ImportExport.Application.Ports;
using ImportExport.Domain.Profiles;
using ImportExport.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace ImportExport.Infrastructure.Repositories;

public sealed class MappingProfileRepository : IMappingProfileRepository
{
    private readonly ImportExportDbContext _db;

    public MappingProfileRepository(ImportExportDbContext db) => _db = db;

    public Task<MappingProfile?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.MappingProfiles
              .FirstOrDefaultAsync(p => p.Id == MappingProfileId.From(id), ct);

    public async Task<IReadOnlyList<MappingProfile>> GetByUserAndTypeAsync(
        UserId userId, string entityTypeName, CancellationToken ct)
        => await _db.MappingProfiles
                    .Where(p => p.UserId == userId && p.EntityTypeName == entityTypeName)
                    .OrderBy(p => p.Name)
                    .ToListAsync(ct);

    public async Task<IReadOnlyList<MappingProfile>> GetByUserAsync(
        UserId userId, CancellationToken ct)
        => await _db.MappingProfiles
                    .Where(p => p.UserId == userId)
                    .OrderBy(p => p.EntityTypeName)
                    .ThenBy(p => p.Name)
                    .ToListAsync(ct);

    public void Add(MappingProfile profile)    => _db.MappingProfiles.Add(profile);
    public void Remove(MappingProfile profile) => _db.MappingProfiles.Remove(profile);
}
