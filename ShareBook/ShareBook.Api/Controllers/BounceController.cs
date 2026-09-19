using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShareBook.Api.ViewModels;
using ShareBook.Domain;
using ShareBook.Repository;
using ShareBook.Service.Server;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ShareBook.Api.Controllers
{
    /// <summary>
    /// Webhook de eventos de entrega/bounce do Stalwart.
    /// </summary>
    [Route("api/[controller]")]
    [EnableCors("AllowAllHeaders")]
    public class BounceController : ControllerBase
    {
        private const string WebhookTokenHeader = "X-Sharebook-Webhook-Token";

        private readonly ApplicationDbContext _ctx;
        private readonly ServerSettings _serverSettings;
        private readonly ILogger<BounceController> _logger;

        public BounceController(
            ApplicationDbContext ctx,
            IOptions<ServerSettings> serverSettings,
            ILogger<BounceController> logger)
        {
            _ctx = ctx;
            _serverSettings = serverSettings.Value;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Receive([FromBody] StalwartWebhookVM webhook, CancellationToken cancellationToken)
        {
            if (!IsAuthorized())
                return Unauthorized();

            if (webhook?.Events == null || webhook.Events.Count == 0)
                return Ok(new { received = 0, processed = 0, skipped = 0 });

            var processed = 0;
            var skipped = 0;

            foreach (var item in webhook.Events)
            {
                var bounce = BuildBounce(item);
                if (bounce == null)
                {
                    _logger.LogInformation("Webhook Stalwart ignorado. Type: {Type}. DataKeys: {DataKeys}.",
                        item.Type,
                        DataKeys(item.Data));
                    skipped++;
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(item.Id))
                {
                    var eventIdNeedle = $"\"id\":\"{item.Id}\"";
                    var alreadyProcessed = await _ctx.MailBounces
                        .AnyAsync(m => m.Body != null && EF.Functions.Like(m.Body, $"%{eventIdNeedle}%"), cancellationToken);

                    if (alreadyProcessed)
                    {
                        skipped++;
                        continue;
                    }
                }

                await _ctx.MailBounces.AddAsync(bounce, cancellationToken);
                processed++;
            }

            if (processed > 0)
                await _ctx.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Webhook Stalwart recebido. Eventos: {Received}. Processados: {Processed}. Ignorados: {Skipped}.",
                webhook.Events.Count, processed, skipped);

            return Ok(new { received = webhook.Events.Count, processed, skipped });
        }

        private bool IsAuthorized()
        {
            if (string.IsNullOrWhiteSpace(_serverSettings.JobExecutorToken))
                return false;

            if (!Request.Headers.TryGetValue(WebhookTokenHeader, out var token))
                return false;

            return string.Equals(token.ToString(), _serverSettings.JobExecutorToken, StringComparison.Ordinal);
        }

        private static MailBounce BuildBounce(StalwartWebhookEventVM item)
        {
            var email = GetFirstString(item.Data, "to", "rcptTo", "recipient", "recipients", "address", "email");
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                return null;

            var code = GetFirstString(item.Data, "code", "statusCode", "smtpCode", "replyCode");
            var reason = GetFirstString(item.Data, "reason", "error", "causedBy");
            var details = GetFirstString(item.Data, "details", "detail", "message", "response");

            if (string.IsNullOrWhiteSpace(code))
                code = ExtractStatusCode(reason, details);

            if (string.IsNullOrWhiteSpace(code))
                code = InferCode(item.Type, reason, details);

            if (string.IsNullOrWhiteSpace(code) || code.Length < 3)
                return null;

            if (code[0] != '4' && code[0] != '5')
                return null;

            var raw = JsonSerializer.Serialize(item);
            return new MailBounce($"Stalwart webhook: {item.Type}", raw)
            {
                Email = email,
                ErrorCode = code[..3],
                IsBounce = true,
                IsSoft = code[0] == '4',
            };
        }

        private static string InferCode(string eventType, string reason, string details)
        {
            var text = $"{eventType} {reason} {details}";

            if (text.Contains("temp", StringComparison.OrdinalIgnoreCase)
                || text.Contains("timeout", StringComparison.OrdinalIgnoreCase)
                || text.Contains("connect-error", StringComparison.OrdinalIgnoreCase)
                || text.Contains("failed", StringComparison.OrdinalIgnoreCase))
                return "421";

            if (text.Contains("null-mx", StringComparison.OrdinalIgnoreCase)
                || text.Contains("mx-lookup-failed", StringComparison.OrdinalIgnoreCase)
                || text.Contains("rejected", StringComparison.OrdinalIgnoreCase)
                || text.Contains("perm", StringComparison.OrdinalIgnoreCase))
                return "550";

            return null;
        }

        private static string ExtractStatusCode(params string[] values)
        {
            foreach (var value in values)
            {
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                var match = Regex.Match(value, @"\b(?<code>[45]\d{2})\b");
                if (match.Success)
                    return match.Groups["code"].Value;
            }

            return null;
        }

        private static string GetFirstString(JsonElement data, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                var value = GetStringRecursive(data, propertyName);
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }

            return null;
        }

        private static string GetStringRecursive(JsonElement data, string propertyName)
        {
            if (data.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in data.EnumerateObject())
                {
                    if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                    {
                        var ownValue = ScalarString(property.Value);
                        if (!string.IsNullOrWhiteSpace(ownValue))
                            return ownValue;
                    }

                    var nestedValue = GetStringRecursive(property.Value, propertyName);
                    if (!string.IsNullOrWhiteSpace(nestedValue))
                        return nestedValue;
                }
            }
            else if (data.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in data.EnumerateArray())
                {
                    var nestedValue = GetStringRecursive(item, propertyName);
                    if (!string.IsNullOrWhiteSpace(nestedValue))
                        return nestedValue;
                }
            }

            return null;
        }

        private static string ScalarString(JsonElement value)
        {
            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString(),
                JsonValueKind.Number => value.ToString(),
                JsonValueKind.Array => value.EnumerateArray()
                    .Select(item => item.ValueKind == JsonValueKind.String ? item.GetString() : item.ToString())
                    .FirstOrDefault(item => !string.IsNullOrWhiteSpace(item)),
                _ => value.ToString(),
            };
        }

        private static string DataKeys(JsonElement data)
        {
            if (data.ValueKind != JsonValueKind.Object)
                return data.ValueKind.ToString();

            return string.Join(",", data.EnumerateObject().Select(property => property.Name));
        }
    }
}
