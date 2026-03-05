using Measures.Domain.Measures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel;

namespace Measures.Infrastructure.Persistence.Configurations;

public sealed class MeasureConfiguration : IEntityTypeConfiguration<Measure>
{
    public void Configure(EntityTypeBuilder<Measure> builder)
    {
        builder.ToTable("Measures");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasConversion(
                id => id.Value,
                value => MeasureId.From(value))
            .ValueGeneratedNever();

        builder.Property(m => m.UserId)
            .HasConversion(
                userId => userId.Value,
                value => UserId.From(value))
            .IsRequired();

        builder.HasIndex(m => m.UserId);
        builder.HasIndex(m => new { m.IsoId, m.UserId }).IsUnique();

        builder.Property(m => m.IsoId).HasMaxLength(20).IsRequired();
        builder.Property(m => m.Name).HasMaxLength(500).IsRequired();

        builder.Property(m => m.CostEur).HasPrecision(18, 2).IsRequired();
        builder.Property(m => m.EffortHours).IsRequired();
        builder.Property(m => m.ImpactRisk).IsRequired();
        builder.Property(m => m.Confidence).IsRequired();

        builder.Property(m => m.Dependencies)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null)!)
            .HasColumnType("jsonb")
            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                (a, b) => a != null && b != null && a.SequenceEqual(b),
                v => v.Aggregate(0, (hash, s) => HashCode.Combine(hash, s.GetHashCode())),
                v => v.ToList()));

        builder.Property(m => m.Justification).HasMaxLength(2000);

        builder.Property(m => m.CategoryIds)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(v, (System.Text.Json.JsonSerializerOptions?)null)!)
            .HasColumnType("jsonb")
            .Metadata.SetValueComparer(new ValueComparer<List<Guid>>(
                (a, b) => a != null && b != null && a.SequenceEqual(b),
                v => v.Aggregate(0, (hash, g) => HashCode.Combine(hash, g.GetHashCode())),
                v => v.ToList()));

        builder.Property(m => m.TagIds)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(v, (System.Text.Json.JsonSerializerOptions?)null)!)
            .HasColumnType("jsonb")
            .Metadata.SetValueComparer(new ValueComparer<List<Guid>>(
                (a, b) => a != null && b != null && a.SequenceEqual(b),
                v => v.Aggregate(0, (hash, g) => HashCode.Combine(hash, g.GetHashCode())),
                v => v.ToList()));

        builder.Property(m => m.ConfDataQuality).IsRequired();
        builder.Property(m => m.ConfDataSourceCount).IsRequired();
        builder.Property(m => m.ConfDataRecency).IsRequired();
        builder.Property(m => m.ConfSpecificity).IsRequired();

        builder.Property(m => m.GraphDependentsCount).IsRequired();
        builder.Property(m => m.GraphImpactMultiplier).IsRequired();
        builder.Property(m => m.GraphTotalCost).HasPrecision(18, 2).IsRequired();
        builder.Property(m => m.GraphCostEfficiency).IsRequired();

        builder.Property(m => m.CreatedAt).IsRequired();
        builder.Property(m => m.UpdatedAt).IsRequired();

        builder.Ignore(m => m.DomainEvents);
    }
}
