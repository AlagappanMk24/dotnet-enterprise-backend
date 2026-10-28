using Enterprise.BackgroundJobs.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace Enterprise.BackgroundJobs.Infrastructure.External
{
    public sealed class ExternalOrderClient(HttpClient httpClient, IConfiguration configuration) : IExternalOrderClient
    {
        public async Task NotifyOrderProcessedAsync(Guid orderId, CancellationToken cancellationToken)
        {
            var apiKey = configuration["ExternalOrderApi:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "ExternalOrderApi:ApiKey is not configured.");
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "api/orders/background-processing-complete")
            {
                Content = JsonContent.Create(new { orderId })
            };

            request.Headers.Add("X-Internal-Api-Key", apiKey);

            using var response = await httpClient.SendAsync(
                request,
                cancellationToken);

            response.EnsureSuccessStatusCode();
        }
    }
}