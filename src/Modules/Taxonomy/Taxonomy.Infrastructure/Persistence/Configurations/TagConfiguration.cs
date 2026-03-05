using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel;
using Taxonomy.Domain.Tags;

namespace Taxonomy.Infrastructure.Persistence.Configurations;

public sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasConversion(
                id => id.Value,
                value => TagId.From(value))
            .ValueGeneratedNever();

        builder.HasIndex(t => t.Label).IsUnique();

        builder.Property(t => t.Label).HasMaxLength(100).IsRequired();
        builder.Property(t => t.Color).HasMaxLength(20).IsRequired();

        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt).IsRequired();

        builder.Ignore(t => t.DomainEvents);
    }
}
