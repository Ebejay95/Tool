using CMC.SharedKernel;
using CMC.Todos.Domain.TodoItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Todos.Infrastructure.Persistence.Configurations;

public sealed class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        builder.ToTable("TodoItems");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasConversion(
                id => id.Value,
                value => TodoId.From(value))
            .ValueGeneratedNever();

        builder.Property(t => t.UserId)
            .HasConversion(
                userId => userId.Value,
                value => UserId.From(value))
            .IsRequired();

        builder.HasIndex(t => t.UserId);

        builder.Property(t => t.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(t => t.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(t => t.Priority)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(t => t.DueDate);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .IsRequired();

        builder.Property(t => t.CompletedAt);

        // Ignore domain events (not persisted)
        builder.Ignore(t => t.DomainEvents);

        // Ignore computed properties
        builder.Ignore(t => t.IsOverdue);
    }
}
