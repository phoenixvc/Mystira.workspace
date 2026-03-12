namespace Mystira.Core.Services;

/// <summary>
/// Builds magic signup email content.
/// </summary>
public class MagicSignupEmailBuilder
{
  public string Subject => "Your Mystira magic sign-in link";

  public string BuildVerificationEmail(string displayName, string verifyUrl)
  {
    var safeDisplayName = string.IsNullOrWhiteSpace(displayName) ? "there" : displayName;

    return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
  <title>Mystira Magic Link</title>
</head>
<body style=""font-family: Arial, sans-serif; background-color: #f8fafc; margin: 0; padding: 24px;"">
  <div style=""max-width: 560px; margin: 0 auto; background: #ffffff; border-radius: 12px; padding: 24px;"">
    <h2 style=""margin-top: 0; color: #1f2937;"">Welcome to Mystira, {safeDisplayName}!</h2>
    <p style=""color: #374151;"">Use the button below to verify your email and continue sign-in.</p>
    <p style=""margin: 24px 0;"">
      <a href=""{verifyUrl}"" style=""background: #2563eb; color: #ffffff; text-decoration: none; padding: 12px 18px; border-radius: 8px; display: inline-block;"">Verify email and continue</a>
    </p>
    <p style=""color: #6b7280; font-size: 14px;"">This link expires in 30 minutes.</p>
    <p style=""color: #9ca3af; font-size: 12px;"">If you did not request this link, you can safely ignore this email.</p>
  </div>
</body>
</html>";
  }
}
