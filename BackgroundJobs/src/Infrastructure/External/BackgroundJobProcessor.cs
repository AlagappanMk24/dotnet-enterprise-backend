using Enterprise.BackgroundJobs.Application.DTOs.Records;
using Enterprise.BackgroundJobs.Application.Interfaces;
using Enterprise.BackgroundJobs.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Enterprise.BackgroundJobs.Infrastructure.External
{
    public sealed class BackgroundJobProcessor(
     IExternalOrderClient externalClient,
     ILogger<BackgroundJobProcessor> logger) : IBackgroundJobProcessor
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public async Task ProcessAsync(BackgroundJobType type, string payload, CancellationToken cancellationToken)
        {
            switch (type)
            {
                case BackgroundJobType.ProcessOrder:
                    {
                        var request = JsonSerializer.Deserialize<ProcessOrderPayload>(payload, JsonOptions)
                            ?? throw new InvalidOperationException("Invalid process-order payload.");

                        logger.LogInformation("Processing order {OrderId}", request.OrderId);
                        await externalClient.NotifyOrderProcessedAsync(request.OrderId, cancellationToken);
                        logger.LogInformation("Processed order {OrderId}", request.OrderId);
                        break;
                    }
                case BackgroundJobType.SendEmail:
                    {
                        var email = JsonSerializer.Deserialize<SendEmailPayload>(payload, JsonOptions)
                            ?? throw new InvalidOperationException("Invalid email payload.");

                        logger.LogInformation("Email job accepted for {Recipient}, subject {Subject}", email.To, email.Subject);
                        await Task.Delay(150, cancellationToken);
                        break;
                    }
                default: throw new NotSupportedException($"Unsupported job type {type}.");
            }
        }
    }
}
