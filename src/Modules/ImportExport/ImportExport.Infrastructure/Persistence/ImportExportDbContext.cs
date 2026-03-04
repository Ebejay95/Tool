using ImportExport.Application.Ports;
using ImportExport.Domain.Profiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel;

namespace ImportExport.Infrastructure.Persistence;

public sealed class ImportExportDbContext : DbContext, IImportExportUnitOfWork
{
    public ImportExportDbContext(DbContextOptions<ImportExportDbContext> options)
        : base(options) { }

    public DbSet<MappingProfile> MappingProfiles => Set<MappingProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("importexport");
        modelBuilder.ApplyConfiguration(new MappingProfileConfiguration());
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => base.SaveChangesAsync(cancellationToken);
}

internal sealed class MappingProfileConfiguration : IEntityTypeConfiguration<MappingProfile>
{
    public void Configure(EntityTypeBuilder<MappingProfile> builder)
    {
        builder.ToTable("MappingProfiles");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, v => MappingProfileId.From(v));

        builder.Property(p => p.UserId)
            .HasConversion(
                id => id.Value,
                v  => UserId.From(v))
            .IsRequired();

        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.EntityTypeName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Channel)
            .HasMaxLength(50)
            .IsRequired();

        // FieldRules als JSON-Kolonne persistieren
        builder.OwnsMany(p => p.FieldRules, r =>
        {
            r.ToJson();
            r.Property(x => x.SourceColumn).HasMaxLength(200);
            r.Property(x => x.TargetField).HasMaxLength(200);
            r.Property(x => x.TransformHint).HasMaxLength(200);
        });

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        builder.HasIndex(p => new { p.UserId, p.EntityTypeName });
    }
}
