using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel;
using Taxonomy.Domain.Categories;

namespace Taxonomy.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion(
                id => id.Value,
                value => CategoryId.From(value))
            .ValueGeneratedNever();

        builder.HasIndex(c => c.Label).IsUnique();

        builder.Property(c => c.Label).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Color).HasMaxLength(20).IsRequired();

        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt).IsRequired();

        builder.Ignore(c => c.DomainEvents);
    }
}
