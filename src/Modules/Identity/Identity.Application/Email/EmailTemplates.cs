namespace Identity.Application.Mailing;

/// <summary>
/// Zentrale HTML-E-Mail-Templates für alle system-generierten E-Mails.
/// Nutzt ein responsives, Outlook-kompatibles Table-Layout mit Inline-Styles.
/// </summary>
public static class EmailTemplates
{
    // ── Farbpalette ────────────────────────────────────────────────────────────
    private const string ColorPrimary  = "#4F46E5"; // Indigo
    private const string ColorText     = "#1F2937";
    private const string ColorMuted    = "#6B7280";
    private const string ColorBg       = "#F9FAFB";
    private const string ColorCard     = "#FFFFFF";
    private const string ColorBorder   = "#E5E7EB";
    private const string ColorCode     = "#F3F4F6";
    private const string ColorCodeText = "#1F2937";

    // ── Öffentliche Template-Methoden ──────────────────────────────────────────

    /// <summary>
    /// Bestätigungs-E-Mail nach Registrierung.
    /// Enthält Bestätigungscode und direkten Link zur Verifizierungsseite.
    /// </summary>
    public static string EmailVerification(string verificationToken, string verifyUrl)
    {
        var content = $"""
            <p style="margin:0 0 16px 0;font-size:16px;line-height:1.6;color:{ColorText};">
              Vielen Dank für Ihre Registrierung! Bitte bestätigen Sie Ihre E-Mail-Adresse,
              um sich anmelden zu können.
            </p>

            <p style="margin:0 0 8px 0;font-size:14px;color:{ColorMuted};">Ihr Bestätigungscode:</p>
            {CodeBlock(verificationToken)}

            <p style="margin:16px 0;font-size:14px;color:{ColorMuted};text-align:center;">
              — oder klicken Sie direkt auf den Button —
            </p>

            {PrimaryButton("E-Mail-Adresse bestätigen", verifyUrl)}

            {Divider()}

            <p style="margin:16px 0 0 0;font-size:13px;color:{ColorMuted};">
              ⚠️ Dieser Code ist <strong>24 Stunden</strong> gültig.<br>
              Falls Sie sich nicht registriert haben, können Sie diese E-Mail ignorieren.
            </p>
            """;

        return WrapInLayout("E-Mail-Adresse bestätigen", content);
    }

    /// <summary>
    /// Passwort-Reset-E-Mail mit Einmal-Link.
    /// </summary>
    public static string PasswordReset(string resetUrl)
    {
        var content = $"""
            <p style="margin:0 0 16px 0;font-size:16px;line-height:1.6;color:{ColorText};">
              Sie haben eine Passwortzurücksetzung für Ihren Account angefordert.
              Klicken Sie auf den Button, um ein neues Passwort zu vergeben.
            </p>

            {PrimaryButton("Passwort jetzt zurücksetzen", resetUrl)}

            {Divider()}

            <p style="margin:16px 0 0 0;font-size:13px;color:{ColorMuted};">
              ⚠️ Dieser Link ist <strong>1 Stunde</strong> gültig und kann nur einmal verwendet werden.<br>
              Falls Sie keine Passwortzurücksetzung angefordert haben,
              ignorieren Sie diese E-Mail bitte. Ihr Passwort bleibt unverändert.
            </p>
            """;

        return WrapInLayout("Passwort zurücksetzen", content);
    }

    /// <summary>
    /// Willkommens-E-Mail nach erfolgreicher Registrierung.
    /// Wird zusätzlich zur Verifizierungs-E-Mail versendet.
    /// </summary>
    public static string Welcome(string firstName)
    {
        var displayName = string.IsNullOrWhiteSpace(firstName) ? "" : $" {firstName}";
        var content = $"""
            <p style="margin:0 0 16px 0;font-size:16px;line-height:1.6;color:{ColorText};">
              Hallo{displayName}, herzlich willkommen!<br>
              Ihr Account wurde erfolgreich erstellt.
            </p>

            <table role="presentation" cellspacing="0" cellpadding="0" border="0" width="100%" style="margin:0 0 20px 0;">
              <tr>
                <td style="background:{ColorCode};border-radius:8px;padding:16px;">
                  <p style="margin:0 0 8px 0;font-size:14px;font-weight:600;color:{ColorText};">Nächste Schritte:</p>
                  <p style="margin:0 0 6px 0;font-size:14px;color:{ColorText};">
                    1. Bestätigen Sie Ihre E-Mail-Adresse (separate E-Mail wurde gesendet).
                  </p>
                  <p style="margin:0;font-size:14px;color:{ColorText};">
                    2. Melden Sie sich nach der Bestätigung an.
                  </p>
                </td>
              </tr>
            </table>

            {Divider()}

            <p style="margin:16px 0 0 0;font-size:13px;color:{ColorMuted};">
              Falls Sie sich nicht registriert haben, ignorieren Sie diese E-Mail bitte.
            </p>
            """;

        return WrapInLayout("Willkommen!", content);
    }

    // ── Private Hilfsmethoden ─────────────────────────────────────────────────

    private static string WrapInLayout(string subject, string innerContent) => $"""
        <!DOCTYPE html>
        <html lang="de">
        <head>
          <meta charset="UTF-8">
          <meta name="viewport" content="width=device-width, initial-scale=1.0">
          <title>{subject}</title>
        </head>
        <body style="margin:0;padding:0;background-color:{ColorBg};font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
          <table role="presentation" cellspacing="0" cellpadding="0" border="0" width="100%" style="background-color:{ColorBg};">
            <tr>
              <td align="center" style="padding:40px 16px;">

                <!-- Card -->
                <table role="presentation" cellspacing="0" cellpadding="0" border="0" width="560"
                       style="max-width:560px;background-color:{ColorCard};border-radius:12px;
                              box-shadow:0 1px 3px rgba(0,0,0,0.08);overflow:hidden;">

                  <!-- Header -->
                  <tr>
                    <td style="background-color:{ColorPrimary};padding:28px 32px;text-align:center;">
                      <p style="margin:0;font-size:22px;font-weight:700;color:#FFFFFF;letter-spacing:-0.5px;">CMC</p>
                    </td>
                  </tr>

                  <!-- Subject line -->
                  <tr>
                    <td style="padding:28px 32px 0 32px;">
                      <h1 style="margin:0 0 20px 0;font-size:20px;font-weight:600;color:{ColorText};">{subject}</h1>
                    </td>
                  </tr>

                  <!-- Body -->
                  <tr>
                    <td style="padding:0 32px 32px 32px;">
                      {innerContent}
                    </td>
                  </tr>

                  <!-- Footer -->
                  <tr>
                    <td style="padding:20px 32px;border-top:1px solid {ColorBorder};text-align:center;">
                      <p style="margin:0;font-size:12px;color:{ColorMuted};">
                        Diese E-Mail wurde automatisch generiert. Bitte antworten Sie nicht darauf.
                      </p>
                      <p style="margin:6px 0 0 0;font-size:12px;color:{ColorMuted};">
                        © {DateTime.UtcNow.Year} CMC — Alle Rechte vorbehalten
                      </p>
                    </td>
                  </tr>

                </table>
                <!-- /Card -->

              </td>
            </tr>
          </table>
        </body>
        </html>
        """;

    private static string CodeBlock(string code) => $"""
        <table role="presentation" cellspacing="0" cellpadding="0" border="0" width="100%" style="margin:0 0 20px 0;">
          <tr>
            <td align="center" style="background:{ColorCode};border:1px solid {ColorBorder};
                border-radius:8px;padding:20px;">
              <p style="margin:0;font-size:28px;font-weight:700;letter-spacing:6px;
                        color:{ColorCodeText};font-family:'Courier New',Courier,monospace;">{code}</p>
            </td>
          </tr>
        </table>
        """;

    private static string PrimaryButton(string label, string url) => $"""
        <table role="presentation" cellspacing="0" cellpadding="0" border="0" style="margin:0 auto 20px auto;">
          <tr>
            <td align="center" style="border-radius:8px;background:{ColorPrimary};">
              <a href="{url}"
                 style="display:inline-block;padding:14px 28px;font-size:15px;font-weight:600;
                        color:#FFFFFF;text-decoration:none;border-radius:8px;
                        background:{ColorPrimary};">
                {label}
              </a>
            </td>
          </tr>
        </table>
        """;

    private static string Divider() => $"""
        <table role="presentation" cellspacing="0" cellpadding="0" border="0" width="100%" style="margin:20px 0 0 0;">
          <tr>
            <td style="height:1px;background:{ColorBorder};font-size:0;line-height:0;">&nbsp;</td>
          </tr>
        </table>
        """;
}
