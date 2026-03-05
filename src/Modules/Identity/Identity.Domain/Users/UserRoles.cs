namespace Identity.Domain.Users;

/// <summary>Definierte Rollen innerhalb der Applikation.</summary>
public static class UserRoles
{
    /// <summary>Standardrolle für alle registrierten Benutzer.</summary>
    public const string User = "user";

    /// <summary>Administratoren – übergreifende Verwaltung aller Ressourcen (später).</summary>
    public const string Admin = "admin";

    /// <summary>App-Owner – können alle Ressourcen aller Nutzer verwalten.</summary>
    public const string SuperAdmin = "super-admin";

    /// <summary>System-Master – ausschließlich für initiales Seeding. Darf nur Rollen vergeben.</summary>
    public const string Master = "master";

    /// <summary>Alle gültigen Rollen die einem Nutzer zugewiesen werden dürfen (exkl. master).</summary>
    public static readonly IReadOnlyList<string> AssignableRoles =
        [User, Admin, SuperAdmin];
}
