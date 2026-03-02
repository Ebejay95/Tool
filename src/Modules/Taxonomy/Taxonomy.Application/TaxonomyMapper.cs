using Taxonomy.Application.DTOs;
using Taxonomy.Domain.Categories;
using Taxonomy.Domain.Tags;

namespace Taxonomy.Application;

public static class TaxonomyMapper
{
    public static CategoryDto ToDto(Category c) => new(
        Id:        c.Id.Value.ToString(),
        UserId:    c.UserId?.Value.ToString(),
        Label:     c.Label,
        Color:     c.Color,
        IsGlobal:  c.IsGlobal,
        CreatedAt: c.CreatedAt,
        UpdatedAt: c.UpdatedAt);

    public static TagDto ToDto(Tag t) => new(
        Id:        t.Id.Value.ToString(),
        UserId:    t.UserId?.Value.ToString(),
        Label:     t.Label,
        Color:     t.Color,
        IsGlobal:  t.IsGlobal,
        CreatedAt: t.CreatedAt,
        UpdatedAt: t.UpdatedAt);
}
