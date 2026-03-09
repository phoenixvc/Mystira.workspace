using Microsoft.Extensions.Configuration;

namespace Mystira.App.Application.Services;

/// <summary>
/// Builds COPPA-compliant consent verification emails.
/// </summary>
public class ConsentEmailBuilder
{
    private readonly string _baseUrl;
    private readonly string _privacyPolicyUrl;

    public ConsentEmailBuilder(IConfiguration configuration)
    {
        _baseUrl = configuration["Coppa:VerificationBaseUrl"]
            ?? configuration["AppSettings:BaseUrl"]
            ?? "https://mystira.app";
        _privacyPolicyUrl = configuration["Coppa:PrivacyPolicyUrl"]
            ?? $"{_baseUrl}/privacy";
    }

    public string Subject => "Mystira - Parental Consent Required";

    public string BuildVerificationEmail(string childDisplayName, string verificationToken)
    {
        var verificationUrl = $"{_baseUrl}/api/coppa/consent/verify?token={Uri.EscapeDataString(verificationToken)}";

        return $"""
        <!DOCTYPE html>
        <html>
        <head><meta charset="utf-8"></head>
        <body style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;">
            <h2 style="color: #2c3e50;">Mystira - Parental Consent Request</h2>

            <p>Hello,</p>

            <p>Your child <strong>{HtmlEncode(childDisplayName)}</strong> would like to use Mystira,
            an interactive storytelling platform for children. Under the Children's Online Privacy Protection Act (COPPA), we need your consent before collecting any personal information.</p>

            <h3>What information we collect:</h3>
            <ul>
                <li>Display name (pseudonym, not real name)</li>
                <li>Age group (for age-appropriate content)</li>
                <li>Game progress and achievements</li>
                <li>Parent email address (hashed, for consent management only)</li>
            </ul>

            <h3>How we use this information:</h3>
            <ul>
                <li>To provide age-appropriate storytelling content</li>
                <li>To track progress through interactive scenarios</li>
                <li>To award educational badges and achievements</li>
            </ul>

            <p>We <strong>never</strong> share children's data with third parties for advertising purposes.</p>

            <p style="margin: 30px 0;">
                <a href="{verificationUrl}"
                   style="background-color: #3498db; color: white; padding: 12px 24px;
                          text-decoration: none; border-radius: 5px; font-size: 16px;">
                    I Consent - Verify My Identity
                </a>
            </p>

            <p><small>This link expires in 48 hours. If you did not request this, please ignore this email.</small></p>

            <hr style="border: none; border-top: 1px solid #eee; margin: 20px 0;">

            <p><small>
                <a href="{_privacyPolicyUrl}">Privacy Policy</a> |
                You can revoke consent at any time from the Parent Dashboard.
                Questions? Contact us at support@mystira.app
            </small></p>
        </body>
        </html>
        """;
    }

    private static string HtmlEncode(string value)
    {
        return System.Net.WebUtility.HtmlEncode(value);
    }
}
