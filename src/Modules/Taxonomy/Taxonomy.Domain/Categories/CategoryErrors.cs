using SharedKernel;

namespace Taxonomy.Domain.Categories;

public static class CategoryErrors
{
    public static readonly Error LabelRequired      = new("Category.LabelRequired", "Der Kategorie-Name darf nicht leer sein.");
    public static readonly Error LabelTooLong       = new("Category.LabelTooLong",  "Der Kategorie-Name darf max. 100 Zeichen lang sein.");
    public static readonly Error ColorRequired      = new("Category.ColorRequired", "Die Farbe darf nicht leer sein.");
    public static readonly Error NotFound           = new("Category.NotFound",      "Die Kategorie wurde nicht gefunden.");
    public static readonly Error LabelAlreadyExists = new("Category.LabelExists",   "Eine Kategorie mit diesem Namen existiert bereits.");
}
