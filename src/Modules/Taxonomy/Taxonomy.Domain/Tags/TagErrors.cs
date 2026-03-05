using SharedKernel;

namespace Taxonomy.Domain.Tags;

public static class TagErrors
{
    public static readonly Error LabelRequired      = new("Tag.LabelRequired", "Der Tag-Name darf nicht leer sein.");
    public static readonly Error LabelTooLong       = new("Tag.LabelTooLong",  "Der Tag-Name darf max. 100 Zeichen lang sein.");
    public static readonly Error ColorRequired      = new("Tag.ColorRequired", "Die Farbe darf nicht leer sein.");
    public static readonly Error NotFound           = new("Tag.NotFound",      "Der Tag wurde nicht gefunden.");
    public static readonly Error LabelAlreadyExists = new("Tag.LabelExists",   "Ein Tag mit diesem Namen existiert bereits.");
}
