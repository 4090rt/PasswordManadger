using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PasswordMenedger.BusinesLogic.LoggerFac;
using Polly;
using Polly.Timeout;

namespace PasswordMenedger.Http.ClientSettings
{
    public class HttpSettingsClient
    {
        private readonly ILogger<HttpSettingsClient> _logger = LogFac.LoggerCreate<HttpSettingsClient>();


        public void Setthings(IServiceCollection client)
        {
            var httpClientBuilder = client.AddHttpClient("Client1Http2.0", clienthttp =>
            {
                clienthttp.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                clienthttp.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
                clienthttp.DefaultRequestHeaders.Accept.ParseAdd("application/json");

                clienthttp.DefaultRequestVersion = HttpVersion.Version20;
                clienthttp.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
            });
            httpClientBuilder.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(
                TimeSpan.FromMinutes(0.30),
                TimeoutStrategy.Pessimistic,
                onTimeoutAsync: (context, timespan, task) =>
                {
                    _logger.LogWarning($"⏰ Request timed out after {timespan}");
                    return Task.CompletedTask;
                }));
            httpClientBuilder.AddTransientHttpErrorPolicy(polly => polly.CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(60),
                onHalfOpen: () =>
                {
                    _logger.LogWarning("⚠️ Circuit half-open");
                },
                onBreak: (outcome, timespan) =>
                {
                    _logger.LogWarning($"🔌 Circuit opened for {timespan}");
                },
                onReset: () =>
                {
                    _logger.LogWarning("✅ Circuit reset");
                }));
            httpClientBuilder.AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(3, retryCount =>
            TimeSpan.FromSeconds(Math.Pow(2, retryCount)) +
            TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100)),
            onRetryAsync: (outcome, timespan, retrycount, task) =>
            {
                Console.WriteLine($"⏰ Request timed out after {timespan}");
                return Task.CompletedTask;
            }));
            httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true,

                PooledConnectionLifetime = TimeSpan.FromMinutes(10),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(15),

                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.Brotli,

                MaxConnectionsPerServer = 10,
                MaxAutomaticRedirections = 15,
                UseCookies = false,
                AllowAutoRedirect = true
            });
        }
    }
}
