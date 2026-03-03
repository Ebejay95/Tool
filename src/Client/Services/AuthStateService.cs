using Identity.Application.DTOs;

namespace App.Services;

/// <summary>
/// Hält den flüchtigen Zustand des mehrstufigen Login-Vorgangs im WASM-Speicher.
/// Der Zustand wird bei einem Page-Reload verloren – der Benutzer wird dann
/// zurück zur Login-Seite geleitet.
/// </summary>
public sealed class AuthStateService
{
    /// <summary>Kurzlebiger Pre-Auth-Token für 2FA-Setup oder 2FA-Validation.</summary>
    public string? PreAuthToken { get; set; }

    /// <summary>E-Mail-Adresse aus dem Login-Vorgang (für die Verify-Mail-Seite).</summary>
    public string? PendingEmail { get; set; }

    /// <summary>Aktuelle Phase des Login-Ablaufs.</summary>
    public LoginStage PendingStage { get; set; } = LoginStage.Complete;

    public bool HasPendingFlow =>
        PendingStage != LoginStage.Complete && !string.IsNullOrEmpty(PreAuthToken ?? PendingEmail);

    public void Clear()
    {
        PreAuthToken  = null;
        PendingEmail  = null;
        PendingStage  = LoginStage.Complete;
    }

    public void SetEmailVerification(string email)
    {
        PendingEmail  = email;
        PendingStage  = LoginStage.RequiresEmailVerification;
        PreAuthToken  = null;
    }

    public void SetTwoFactorSetup(string preAuthToken, string email)
    {
        PreAuthToken = preAuthToken;
        PendingEmail = email;
        PendingStage = LoginStage.RequiresTwoFactorSetup;
    }

    public void SetTwoFactorValidation(string preAuthToken, string email)
    {
        PreAuthToken = preAuthToken;
        PendingEmail = email;
        PendingStage = LoginStage.RequiresTwoFactorValidation;
    }
}
