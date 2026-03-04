using SharedKernel;

namespace ImportExport.Domain.Profiles;

public static class MappingProfileErrors
{
    public static readonly Error NameRequired =
        new("MappingProfile.NameRequired", "Der Name des Mapping-Profils darf nicht leer sein.");

    public static readonly Error EntityTypeNameRequired =
        new("MappingProfile.EntityTypeNameRequired", "Der Entity-Typ darf nicht leer sein.");

    public static readonly Error ChannelRequired =
        new("MappingProfile.ChannelRequired", "Der Channel darf nicht leer sein.");

    public static readonly Error NotFound =
        new("MappingProfile.NotFound", "Das Mapping-Profil wurde nicht gefunden.");

    public static readonly Error Forbidden =
        new("MappingProfile.Forbidden", "Sie haben keinen Zugriff auf dieses Mapping-Profil.");
}
