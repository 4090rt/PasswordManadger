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
    public class HttpSettingsClientReserve
    {
        private ILogger<HttpSettingsClientReserve> _logger = LogFac.LoggerCreate<HttpSettingsClientReserve>();

        public void SetthingsReserve(IServiceCollection client)
        {
            var httpclienthandler = client.AddHttpClient("ClientReserve", client =>
            {
                client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
                client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

                client.DefaultRequestVersion = HttpVersion.Version20;
                client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
            });
            httpclienthandler.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(
                TimeSpan.FromMinutes(0.45),
                TimeoutStrategy.Pessimistic,
                onTimeoutAsync: (context, timespan, task) =>
                {
                    _logger.LogWarning($"⏰ Request timed out after {timespan}");
                    return Task.CompletedTask;
                }));
            httpclienthandler.AddTransientHttpErrorPolicy(polly => polly.CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromMinutes(1, 5),
                onBreak: (outcome, timespan) =>
                {
                    _logger.LogWarning($"🔌 Circuit opened for {timespan}");
                },
                onHalfOpen: () =>
                {
                    _logger.LogWarning("⚠️ Circuit half-open");
                },
                onReset: () =>
                {
                    _logger.LogWarning("✅ Circuit reset");
                }));
            httpclienthandler.AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(4, retrycount =>
                TimeSpan.FromSeconds(Math.Pow(2, retrycount)) +
                TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100)),
                onRetryAsync: (outcome, timespan, retrycount, task) =>
                {
                    _logger.LogWarning($"⏰ Request timed out after {timespan}");
                    return Task.CompletedTask;
                }));
            httpclienthandler.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true,

                PooledConnectionLifetime = TimeSpan.FromMinutes(8),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(10),

                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.Brotli,

                MaxConnectionsPerServer = 8,
                MaxAutomaticRedirections = 20,
                UseCookies = false,
                AllowAutoRedirect = true,
            });
        }
    }
}
