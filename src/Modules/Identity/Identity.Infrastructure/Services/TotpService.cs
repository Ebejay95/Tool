using Identity.Application.Ports;
using OtpNet;
using QRCoder;
using System.Security.Cryptography;

namespace Identity.Infrastructure.Services;

/// <summary>
/// TOTP-Dienst basierend auf RFC 6238 (Time-based One-Time Passwords).
/// Verwendet OtpNet für die Code-Generierung/-Validierung und QRCoder für den QR-Code.
/// </summary>
public sealed class TotpService : ITotpService
{
    private const int SecretLengthBytes = 20; // 160 Bit – TOTP-Standard
    private const int ToleranceWindows  = 1;  // ±1 Zeitfenster (je 30 Sekunden)

    public string GenerateSecret()
    {
        var secretBytes = RandomNumberGenerator.GetBytes(SecretLengthBytes);
        return Base32Encoding.ToString(secretBytes);
    }

    public string GetOtpAuthUri(string secret, string email, string issuer)
    {
        var encodedIssuer = Uri.EscapeDataString(issuer);
        var encodedEmail  = Uri.EscapeDataString(email);
        var cleanSecret   = secret.Replace(" ", "").ToUpperInvariant();

        return $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={cleanSecret}&issuer={encodedIssuer}&algorithm=SHA1&digits=6&period=30";
    }

    public string GenerateQrCodeBase64(string otpAuthUri)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrData      = qrGenerator.CreateQrCode(otpAuthUri, QRCodeGenerator.ECCLevel.Q);
        using var qrCode      = new PngByteQRCode(qrData);

        var pngBytes = qrCode.GetGraphic(5); // 5px pro Modul
        return Convert.ToBase64String(pngBytes);
    }

    public bool ValidateCode(string secret, string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 6)
            return false;

        if (!long.TryParse(code, out _))
            return false;

        try
        {
            var secretBytes = Base32Encoding.ToBytes(secret.Replace(" ", "").ToUpperInvariant());
            var totp        = new Totp(secretBytes);

            return totp.VerifyTotp(code, out _, new VerificationWindow(ToleranceWindows, ToleranceWindows));
        }
        catch
        {
            return false;
        }
    }
}
