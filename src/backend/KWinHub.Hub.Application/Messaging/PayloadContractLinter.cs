using System.Text.RegularExpressions;

namespace KWinHub.Hub.Application.Messaging;

public static class PayloadContractLinter
{
    private static readonly Regex UrlPattern = new(
        @"https?://", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex SecretPattern = new(
        @"(?i)(secret|password|token|apikey|api_key|accesskey|access_key|privatekey|private_key)",
        RegexOptions.Compiled);

    public static void Validate(string payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            return;
        }

        if (UrlPattern.IsMatch(payloadJson))
        {
            throw new InvalidPayloadException(
                "Outbox payload must not contain URLs. Use IDs/references/versions instead.");
        }

        if (SecretPattern.IsMatch(payloadJson))
        {
            throw new InvalidPayloadException(
                "Outbox payload must not contain secrets (passwords, tokens, keys).");
        }
    }
}

public class InvalidPayloadException : Exception
{
    public InvalidPayloadException(string message) : base(message)
    {
    }
}