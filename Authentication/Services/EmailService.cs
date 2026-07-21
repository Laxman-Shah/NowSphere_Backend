using System.Globalization;
using System.Net;

using MailKit.Net.Smtp;
using MailKit.Security;

using Microsoft.Extensions.Options;

using MimeKit;
using smartApi.Authentication.Services.Interfaces;
using smartApi.Utility.Configurations;
using AppBadRequestException =
    smartApi.ExceptionHandling.Exceptions.Common.BadRequestException;

namespace smartApi.Authentication.Services
{
    public sealed class EmailService : IEmailService
    {
        private const string RegisterEmailVerificationPurpose =
            "REGISTER_EMAIL_VERIFICATION";

        private const string LoginTwoFactorPurpose =
            "LOGIN_TWO_FACTOR_AUTHENTICATION";

        private const string PasswordResetPurpose =
            "PASSWORD_RESET";

        private const int OtpExpiryMinutes = 10;

        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailOptions)
        {
            if (emailOptions == null)
            {
                throw new ArgumentNullException(nameof(emailOptions));
            }

            _emailSettings = emailOptions.Value;

            ValidateEmailSettings();
        }

        public async Task SendOtpEmailAsync(
            string toEmail,
            string otp,
            string purpose,
            CancellationToken cancellationToken = default)
        {
            ValidateRecipientEmail(toEmail);

            if (string.IsNullOrWhiteSpace(otp))
            {
                throw new ArgumentException(
                    "OTP cannot be empty.",
                    nameof(otp));
            }

            if (string.IsNullOrWhiteSpace(purpose))
            {
                throw new ArgumentException(
                    "Email purpose cannot be empty.",
                    nameof(purpose));
            }

            string subject;
            string heading;
            string description;
            string securityMessage;

            switch (purpose.Trim().ToUpperInvariant())
            {
                case RegisterEmailVerificationPurpose:
                    subject = "Verify your NowSphere email address";
                    heading = "Verify your email address";
                    description =
                        "Use the verification code below to complete " +
                        "your NowSphere registration.";

                    securityMessage =
                        "If you did not create a NowSphere account, " +
                        "you can safely ignore this email.";
                    break;

                case LoginTwoFactorPurpose:
                    subject = "Your NowSphere sign-in code";
                    heading = "Confirm your sign-in";
                    description =
                        "Use the verification code below to continue " +
                        "signing in to your NowSphere account.";

                    securityMessage =
                        "If you did not attempt to sign in, do not " +
                        "share this code with anyone.";
                    break;

                case PasswordResetPurpose:
                    subject = "Your NowSphere password-reset code";
                    heading = "Reset your password";
                    description =
                        "Use the verification code below to continue " +
                        "resetting your NowSphere password.";

                    securityMessage =
                        "If you did not request a password reset, " +
                        "you can safely ignore this email.";
                    break;

                default:
                    subject = "Your NowSphere verification code";
                    heading = "Verification required";
                    description =
                        "Use the verification code below to continue.";

                    securityMessage =
                        "If you did not request this code, " +
                        "you can safely ignore this email.";
                    break;
            }

            string safeOtp = WebUtility.HtmlEncode(otp);
            string safeDescription =
                WebUtility.HtmlEncode(description);
            string safeSecurityMessage =
                WebUtility.HtmlEncode(securityMessage);

            string htmlContent = $@"
                <p style=""margin:0 0 22px 0;
                          color:#475569;
                          font-size:15px;
                          line-height:24px;"">
                    {safeDescription}
                </p>

                <table role=""presentation""
                       width=""100%""
                       cellspacing=""0""
                       cellpadding=""0""
                       border=""0""
                       style=""margin:0 0 24px 0;"">
                    <tr>
                        <td align=""center""
                            style=""padding:26px 20px;
                                   background-color:#07182b;
                                   border:1px solid #1e4570;
                                   border-radius:12px;"">

                            <p style=""margin:0 0 10px 0;
                                      color:#94a3b8;
                                      font-size:11px;
                                      line-height:16px;
                                      font-weight:700;
                                      letter-spacing:0.12em;
                                      text-transform:uppercase;"">
                                Verification code
                            </p>

                            <p style=""margin:0;
                                      color:#f8fafc;
                                      font-family:'Courier New',
                                                  Courier,
                                                  monospace;
                                      font-size:34px;
                                      line-height:42px;
                                      font-weight:700;
                                      letter-spacing:0.20em;"">
                                {safeOtp}
                            </p>
                        </td>
                    </tr>
                </table>

                <p style=""margin:0 0 20px 0;
                          color:#475569;
                          font-size:14px;
                          line-height:22px;"">
                    This code expires in
                    <strong>{OtpExpiryMinutes} minutes</strong>.
                    Enter the code only on the official NowSphere
                    application.
                </p>

                {BuildSecurityNotice(safeSecurityMessage)}
            ";

            string textContent =
                heading + Environment.NewLine +
                Environment.NewLine +
                description + Environment.NewLine +
                Environment.NewLine +
                "Verification code: " + otp +
                Environment.NewLine +
                Environment.NewLine +
                "This code expires in " +
                OtpExpiryMinutes +
                " minutes." +
                Environment.NewLine +
                Environment.NewLine +
                securityMessage +
                Environment.NewLine +
                Environment.NewLine +
                "Never share this code with anyone." +
                Environment.NewLine +
                Environment.NewLine +
                "NowSphere";

            MimeMessage message = CreateMessage(
                toEmail,
                null,
                subject,
                heading,
                "Security code",
                htmlContent,
                textContent);

            await SendEmailAsync(
                message,
                cancellationToken);
        }

        public async Task SendEmailVerificationSuccessAsync(
            string toEmail,
            string fullName,
            CancellationToken cancellationToken = default)
        {
            ValidateRecipientEmail(toEmail);

            string safeName = EncodeOrFallback(
                fullName,
                "there");

            string htmlContent = $@"
                <p style=""margin:0 0 18px 0;
                          color:#475569;
                          font-size:15px;
                          line-height:24px;"">
                    Hello
                    <strong style=""color:#0f172a;"">
                        {safeName}
                    </strong>,
                </p>

                <p style=""margin:0 0 22px 0;
                          color:#475569;
                          font-size:15px;
                          line-height:24px;"">
                    Welcome to <strong>NowSphere</strong>! Your email
                    address has been verified successfully and your
                    account is now <strong>active</strong>.
                </p>

                {BuildInformationPanel(
                    "Registration Complete",
                    "You can now sign in to your account, complete your profile, and start exploring all the features NowSphere has to offer.",
                    "#22c55e",
                    "#f0fdf4",
                    "#bbf7d0",
                    "#166534",
                    "#15803d")}

                <p style=""margin:0 0 22px 0;
                          color:#475569;
                          font-size:15px;
                          line-height:24px;"">
                    <strong>What's next?</strong>
                </p>

                <table role=""presentation""
                       width=""100%""
                       cellspacing=""0""
                       cellpadding=""0""
                       border=""0""
                       style=""margin:0 0 24px 0;"">
                    <tr>
                        <td style=""padding:12px 0;
                                   border-bottom:1px solid #e2e8f0;"">
                            <span style=""display:inline-block;
                                         width:24px;
                                         height:24px;
                                         background-color:#3b82f6;
                                         color:#ffffff;
                                         border-radius:50%;
                                         text-align:center;
                                         line-height:24px;
                                         font-size:12px;
                                         font-weight:700;
                                         margin-right:12px;"">
                                1
                            </span>
                            <span style=""color:#475569;
                                         font-size:14px;
                                         line-height:22px;"">
                                Sign in to your account
                            </span>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding:12px 0;
                                   border-bottom:1px solid #e2e8f0;"">
                            <span style=""display:inline-block;
                                         width:24px;
                                         height:24px;
                                         background-color:#3b82f6;
                                         color:#ffffff;
                                         border-radius:50%;
                                         text-align:center;
                                         line-height:24px;
                                         font-size:12px;
                                         font-weight:700;
                                         margin-right:12px;"">
                                2
                            </span>
                            <span style=""color:#475569;
                                         font-size:14px;
                                         line-height:22px;"">
                                Complete your profile information
                            </span>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding:12px 0;"">
                            <span style=""display:inline-block;
                                         width:24px;
                                         height:24px;
                                         background-color:#3b82f6;
                                         color:#ffffff;
                                         border-radius:50%;
                                         text-align:center;
                                         line-height:24px;
                                         font-size:12px;
                                         font-weight:700;
                                         margin-right:12px;"">
                                3
                            </span>
                            <span style=""color:#475569;
                                         font-size:14px;
                                         line-height:22px;"">
                                Explore NowSphere features
                            </span>
                        </td>
                    </tr>
                </table>

                <p style=""margin:0 0 22px 0;
                          color:#64748b;
                          font-size:14px;
                          line-height:22px;"">
                    If you have any questions or need assistance,
                    feel free to reach out to our support team.
                </p>

                <p style=""margin:0;
                          color:#475569;
                          font-size:15px;
                          line-height:24px;"">
                    Thank you for joining NowSphere!
                </p>
            ";

            string textContent =
                "Welcome to NowSphere!" +
                Environment.NewLine +
                Environment.NewLine +
                "Hello " + (fullName ?? "there") + "," +
                Environment.NewLine +
                Environment.NewLine +
                "Your email address has been verified successfully." +
                Environment.NewLine +
                "Your NowSphere account is now active." +
                Environment.NewLine +
                Environment.NewLine +
                "What's next?" +
                Environment.NewLine +
                "1. Sign in to your account" +
                Environment.NewLine +
                "2. Complete your profile information" +
                Environment.NewLine +
                "3. Explore NowSphere features" +
                Environment.NewLine +
                Environment.NewLine +
                "Thank you for joining NowSphere!" +
                Environment.NewLine +
                Environment.NewLine +
                "NowSphere";

            MimeMessage message = CreateMessage(
                toEmail,
                fullName,
                "Welcome to NowSphere - Registration Complete",
                "Your account is ready",
                "Registration successful",
                htmlContent,
                textContent);

            await SendEmailAsync(
                message,
                cancellationToken);
        }

        public async Task SendLoginSuccessEmailAsync(
            string toEmail,
            string receiverName,
            DateTime loginTime,
            string? ipAddress,
            string? userAgent,
            CancellationToken cancellationToken = default)
        {
            ValidateRecipientEmail(toEmail);

            string safeName = EncodeOrFallback(
                receiverName,
                "there");

            string safeIpAddress = EncodeOrFallback(
                ipAddress,
                "Not available");

            string safeUserAgent = EncodeOrFallback(
                Truncate(userAgent, 220),
                "Not available");

            string formattedTime =
                FormatUtcDateTime(loginTime);

            string htmlContent = $@"
                <p style=""margin:0 0 18px 0;
                          color:#475569;
                          font-size:15px;
                          line-height:24px;"">
                    Hello
                    <strong style=""color:#0f172a;"">
                        {safeName}
                    </strong>,
                </p>

                <p style=""margin:0 0 22px 0;
                          color:#475569;
                          font-size:15px;
                          line-height:24px;"">
                    A successful sign-in to your NowSphere
                    account was recorded.
                </p>

                {BuildLoginDetails(
                    WebUtility.HtmlEncode(formattedTime),
                    safeIpAddress,
                    safeUserAgent)}

                {BuildInformationPanel(
                    "Was this you?",
                    "If you recognize this sign-in, no action " +
                    "is required. If you do not recognize it, " +
                    "change your password and review your " +
                    "active sessions.",
                    "#f59e0b",
                    "#fffbeb",
                    "#fde68a",
                    "#92400e",
                    "#78350f")}
            ";

            string textContent =
                "New sign-in to your NowSphere account" +
                Environment.NewLine +
                Environment.NewLine +
                "Hello " + receiverName + "," +
                Environment.NewLine +
                Environment.NewLine +
                "A successful sign-in was recorded." +
                Environment.NewLine +
                Environment.NewLine +
                "Sign-in time: " + formattedTime +
                Environment.NewLine +
                "IP address: " +
                (ipAddress ?? "Not available") +
                Environment.NewLine +
                "Device / client: " +
                (userAgent ?? "Not available") +
                Environment.NewLine +
                Environment.NewLine +
                "If you recognize this sign-in, no action " +
                "is required." +
                Environment.NewLine +
                Environment.NewLine +
                "NowSphere";

            MimeMessage message = CreateMessage(
                toEmail,
                receiverName,
                "New sign-in to your NowSphere account",
                "New account sign-in",
                "Security notice",
                htmlContent,
                textContent);

            await SendEmailAsync(
                message,
                cancellationToken);
        }

        public async Task SendPasswordChangedEmailAsync(
            string email,
            string displayName,
            DateTime changedAt,
            string? ipAddress,
            string? userAgent,
            CancellationToken cancellationToken = default)
        {
            ValidateRecipientEmail(email);

            string safeName = EncodeOrFallback(
                displayName,
                "there");

            string safeIpAddress = EncodeOrFallback(
                ipAddress,
                "Not available");

            string safeUserAgent = EncodeOrFallback(
                Truncate(userAgent, 220),
                "Not available");

            string formattedTime =
                FormatUtcDateTime(changedAt);

            string htmlContent = $@"
                <p style=""margin:0 0 18px 0;
                          color:#475569;
                          font-size:15px;
                          line-height:24px;"">
                    Hello
                    <strong style=""color:#0f172a;"">
                        {safeName}
                    </strong>,
                </p>

                <p style=""margin:0 0 22px 0;
                          color:#475569;
                          font-size:15px;
                          line-height:24px;"">
                    The password for your NowSphere account
                    was changed successfully.
                </p>

                {BuildLoginDetails(
                    WebUtility.HtmlEncode(formattedTime),
                    safeIpAddress,
                    safeUserAgent)}

                {BuildInformationPanel(
                    "Important security information",
                    "If you made this change, no action is " +
                    "required. If you did not change your " +
                    "password, begin account recovery immediately.",
                    "#ef4444",
                    "#fef2f2",
                    "#fecaca",
                    "#991b1b",
                    "#b91c1c")}
            ";

            string textContent =
                "Your NowSphere password was changed" +
                Environment.NewLine +
                Environment.NewLine +
                "Hello " + displayName + "," +
                Environment.NewLine +
                Environment.NewLine +
                "The password for your NowSphere account " +
                "was changed." +
                Environment.NewLine +
                Environment.NewLine +
                "Changed at: " + formattedTime +
                Environment.NewLine +
                "IP address: " +
                (ipAddress ?? "Not available") +
                Environment.NewLine +
                "Device / client: " +
                (userAgent ?? "Not available") +
                Environment.NewLine +
                Environment.NewLine +
                "If you did not make this change, begin " +
                "account recovery immediately." +
                Environment.NewLine +
                Environment.NewLine +
                "NowSphere";

            MimeMessage message = CreateMessage(
                email,
                displayName,
                "Your NowSphere password was changed",
                "Password changed",
                "Security update",
                htmlContent,
                textContent);

            await SendEmailAsync(
                message,
                cancellationToken);
        }

        private MimeMessage CreateMessage(
            string toEmail,
            string? receiverName,
            string subject,
            string heading,
            string statusLabel,
            string htmlContent,
            string textContent)
        {
            MimeMessage message = new MimeMessage();

            message.From.Add(
                new MailboxAddress(
                    _emailSettings.SenderName,
                    _emailSettings.SenderEmail));

            if (string.IsNullOrWhiteSpace(receiverName))
            {
                message.To.Add(
                    MailboxAddress.Parse(toEmail));
            }
            else
            {
                message.To.Add(
                    new MailboxAddress(
                        receiverName.Trim(),
                        toEmail));
            }

            message.Subject = subject;

            BodyBuilder bodyBuilder = new BodyBuilder
            {
                HtmlBody = BuildEmailLayout(
                    heading,
                    statusLabel,
                    htmlContent),

                TextBody = textContent
            };

            message.Body = bodyBuilder.ToMessageBody();

            return message;
        }

        private async Task SendEmailAsync(
            MimeMessage message,
            CancellationToken cancellationToken)
        {
            using SmtpClient smtpClient =
                new SmtpClient();

            try
            {
                await smtpClient.ConnectAsync(
                    _emailSettings.SmtpHost,
                    _emailSettings.SmtpPort,
                    SecureSocketOptions.StartTls,
                    cancellationToken);

                await smtpClient.AuthenticateAsync(
                    _emailSettings.SenderEmail,
                    _emailSettings.AppPassword,
                    cancellationToken);

                await smtpClient.SendAsync(
                    message,
                    cancellationToken);

                await smtpClient.DisconnectAsync(
                    true,
                    cancellationToken);
            }
            finally
            {
                if (smtpClient.IsConnected)
                {
                    await smtpClient.DisconnectAsync(
                        true,
                        CancellationToken.None);
                }
            }
        }

        private static string BuildEmailLayout(
            string heading,
            string statusLabel,
            string htmlContent)
        {
            string safeHeading =
                WebUtility.HtmlEncode(heading);

            string safeStatusLabel =
                WebUtility.HtmlEncode(statusLabel);

            int currentYear = DateTime.UtcNow.Year;

            return $@"
<!DOCTYPE html>
<html lang=""en""
      xmlns:v=""urn:schemas-microsoft-com:vml""
      xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport""
          content=""width=device-width, initial-scale=1"">
    <meta http-equiv=""X-UA-Compatible""
          content=""IE=edge"">

    <title>{safeHeading}</title>

    <style>
        html,
        body {{
            margin: 0 !important;
            padding: 0 !important;
            width: 100% !important;
            background-color: #f1f5f9;
        }}

        table,
        td {{
            border-collapse: collapse !important;
        }}

        @media screen and (max-width: 640px) {{
            .email-card {{
                width: 100% !important;
                border-radius: 0 !important;
            }}

            .mobile-padding {{
                padding-left: 24px !important;
                padding-right: 24px !important;
            }}

            .header-status {{
                display: none !important;
            }}

            .email-heading {{
                font-size: 26px !important;
                line-height: 34px !important;
            }}
        }}
    </style>
</head>

<body style=""
    margin:0;
    padding:0;
    width:100%;
    background-color:#f1f5f9;
    font-family:Arial,Helvetica,sans-serif;"">

    <center style=""
        width:100%;
        background-color:#f1f5f9;
        padding:40px 0;"">

        <table role=""presentation""
               width=""100%""
               cellspacing=""0""
               cellpadding=""0""
               border=""0"">
            <tr>
                <td align=""center""
                    style=""padding:0 16px;"">

                    <table role=""presentation""
                           width=""600""
                           cellspacing=""0""
                           cellpadding=""0""
                           border=""0""
                           class=""email-card""
                           style=""
                               width:600px;
                               max-width:600px;
                               background-color:#ffffff;
                               border:1px solid #dbe4ee;
                               border-radius:18px;
                               overflow:hidden;"">

                        <tr>
                            <td height=""6""
                                style=""
                                    height:6px;
                                    background-color:#3b82f6;
                                    font-size:0;
                                    line-height:0;"">
                                &nbsp;
                            </td>
                        </tr>

                        <tr>
                            <td class=""mobile-padding""
                                style=""
                                    padding:28px 42px 24px 42px;"">

                                <table role=""presentation""
                                       width=""100%""
                                       cellspacing=""0""
                                       cellpadding=""0""
                                       border=""0"">
                                    <tr>
                                        <td align=""left""
                                            style=""
                                                color:#0f172a;
                                                font-size:22px;
                                                font-weight:700;"">
                                            Now<span style=""
                                                color:#3b82f6;"">
                                                Sphere
                                            </span>
                                        </td>

                                        <td align=""right""
                                            class=""header-status""
                                            style=""
                                                color:#64748b;
                                                font-size:11px;
                                                font-weight:700;
                                                text-transform:uppercase;"">
                                            {safeStatusLabel}
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>

                        <tr>
                            <td class=""mobile-padding""
                                style=""
                                    padding:4px 42px 34px 42px;"">

                                <h1 class=""email-heading""
                                    style=""
                                        margin:0 0 18px 0;
                                        color:#0f172a;
                                        font-size:30px;
                                        line-height:38px;
                                        font-weight:700;"">
                                    {safeHeading}
                                </h1>

                                {htmlContent}
                            </td>
                        </tr>

                        <tr>
                            <td class=""mobile-padding""
                                style=""
                                    padding:26px 42px 30px 42px;
                                    background-color:#f8fafc;
                                    border-top:1px solid #e2e8f0;"">

                                <p style=""
                                    margin:0;
                                    color:#64748b;
                                    font-size:12px;
                                    line-height:19px;"">
                                    This is an automated account
                                    message from NowSphere.
                                    Please do not reply directly
                                    to this email.
                                </p>

                                <p style=""
                                    margin:16px 0 0 0;
                                    color:#94a3b8;
                                    font-size:11px;
                                    line-height:18px;"">
                                    © {currentYear} NowSphere.
                                    All rights reserved.
                                </p>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
    </center>
</body>
</html>";
        }

        private static string BuildLoginDetails(
            string formattedTime,
            string safeIpAddress,
            string safeUserAgent)
        {
            return $@"
                <table role=""presentation""
                       width=""100%""
                       cellspacing=""0""
                       cellpadding=""0""
                       border=""0""
                       style=""
                           width:100%;
                           margin:0 0 24px 0;
                           background-color:#f8fafc;
                           border:1px solid #e2e8f0;
                           border-radius:10px;"">

                    {BuildDetailRow(
                        "Time",
                        formattedTime)}

                    {BuildDetailRow(
                        "IP address",
                        safeIpAddress)}

                    {BuildDetailRow(
                        "Device / client",
                        safeUserAgent)}
                </table>
            ";
        }

        private static string BuildDetailRow(
            string label,
            string value)
        {
            return $@"
                <tr>
                    <td style=""
                        padding:11px 14px;
                        border-bottom:1px solid #e2e8f0;
                        color:#64748b;
                        font-size:13px;
                        width:34%;"">
                        {WebUtility.HtmlEncode(label)}
                    </td>

                    <td style=""
                        padding:11px 14px;
                        border-bottom:1px solid #e2e8f0;
                        color:#0f172a;
                        font-size:13px;
                        font-weight:600;
                        word-break:break-word;"">
                        {value}
                    </td>
                </tr>
            ";
        }

        private static string BuildInformationPanel(
            string title,
            string description,
            string accentColor,
            string backgroundColor,
            string borderColor,
            string titleColor,
            string textColor)
        {
            return $@"
                <table role=""presentation""
                       width=""100%""
                       cellspacing=""0""
                       cellpadding=""0""
                       border=""0""
                       style=""
                           width:100%;
                           margin:0 0 24px 0;
                           background-color:{backgroundColor};
                           border:1px solid {borderColor};
                           border-left:4px solid {accentColor};
                           border-radius:10px;"">
                    <tr>
                        <td style=""padding:16px 18px;"">
                            <p style=""
                                margin:0 0 5px 0;
                                color:{titleColor};
                                font-size:14px;
                                font-weight:700;"">
                                {WebUtility.HtmlEncode(title)}
                            </p>

                            <p style=""
                                margin:0;
                                color:{textColor};
                                font-size:13px;
                                line-height:20px;"">
                                {WebUtility.HtmlEncode(description)}
                            </p>
                        </td>
                    </tr>
                </table>
            ";
        }

        private static string BuildSecurityNotice(
            string safeMessage)
        {
            return $@"
                <table role=""presentation""
                       width=""100%""
                       cellspacing=""0""
                       cellpadding=""0""
                       border=""0""
                       style=""
                           width:100%;
                           background-color:#fff7ed;
                           border:1px solid #fed7aa;
                           border-radius:10px;"">
                    <tr>
                        <td style=""
                            padding:16px 18px;
                            color:#9a3412;
                            font-size:13px;
                            line-height:20px;"">
                            <strong>Security notice:</strong>
                            {safeMessage}
                            Never share this code with anyone.
                        </td>
                    </tr>
                </table>
            ";
        }

        private static string EncodeOrFallback(
            string? value,
            string fallback)
        {
            string result = string.IsNullOrWhiteSpace(value)
                ? fallback
                : value.Trim();

            return WebUtility.HtmlEncode(result);
        }

        private static string? Truncate(
            string? value,
            int maximumLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            string normalizedValue = value.Trim();

            if (normalizedValue.Length <= maximumLength)
            {
                return normalizedValue;
            }

            return normalizedValue.Substring(
                0,
                maximumLength) + "...";
        }

        private static string FormatUtcDateTime(
            DateTime dateTime)
        {
            DateTime utcDateTime;

            if (dateTime.Kind == DateTimeKind.Utc)
            {
                utcDateTime = dateTime;
            }
            else if (dateTime.Kind == DateTimeKind.Local)
            {
                utcDateTime = dateTime.ToUniversalTime();
            }
            else
            {
                utcDateTime = DateTime.SpecifyKind(
                    dateTime,
                    DateTimeKind.Utc);
            }

            return utcDateTime.ToString(
                "dd MMM yyyy, HH:mm:ss 'UTC'",
                CultureInfo.InvariantCulture);
        }

        private static void ValidateRecipientEmail(
            string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException(
                    "Recipient email cannot be empty.",
                    nameof(email));
            }

            try
            {
                MailboxAddress.Parse(email);
            }
            catch (ParseException exception)
            {
                throw new ArgumentException(
                    "The recipient email address is invalid.",
                    nameof(email),
                    exception);
            }
        }

        private void ValidateEmailSettings()
        {
            if (string.IsNullOrWhiteSpace(
                _emailSettings.SmtpHost))
            {
                throw new AppBadRequestException(
                    "SMTP host is not configured.",
                    "SMTP_HOST_NOT_CONFIGURED");
            }

            if (_emailSettings.SmtpPort <= 0)
            {
                throw new AppBadRequestException(
                    "SMTP port is invalid.",
                    "SMTP_PORT_INVALID");
            }

            if (string.IsNullOrWhiteSpace(
                _emailSettings.SenderName))
            {
                throw new AppBadRequestException(
                    "Sender name is not configured.",
                    "SENDER_NAME_NOT_CONFIGURED");
            }

            if (string.IsNullOrWhiteSpace(
                _emailSettings.SenderEmail))
            {
                throw new AppBadRequestException(
                    "Sender email is not configured.",
                    "SENDER_EMAIL_NOT_CONFIGURED");
            }

            if (string.IsNullOrWhiteSpace(
                _emailSettings.AppPassword))
            {
                throw new AppBadRequestException(
                    "SMTP application password is not configured.",
                    "SMTP_PASSWORD_NOT_CONFIGURED");
            }
        }
    }
}