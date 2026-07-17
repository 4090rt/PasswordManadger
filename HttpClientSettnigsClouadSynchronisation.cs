using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PasswordMenedger.BusinesLogic.LoggerFac;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PasswordMenedger.Http.ClientSettings
{
    public class HttpClientSettnigsClouadSynchronisation
    {
        private readonly ILogger<HttpClientSettnigsClouadSynchronisation> _logger = LogFac.LoggerCreate<HttpClientSettnigsClouadSynchronisation>();

        public void ClientCloudSettings(IServiceCollection services)
        {
            try
            {
                var clientService = services.AddHttpClient("CloudClient", clientCloud =>
                {
                    clientCloud.DefaultRequestHeaders.Accept.ParseAdd("application/json");
                    clientCloud.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
                    clientCloud.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

                    clientCloud.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth", "8cb29e75d113407da73eddc9e3c1cc1d");

                    clientCloud.DefaultRequestVersion = HttpVersion.Version20;
                    clientCloud.DefaultVersionPolicy = System.Net.Http.HttpVersionPolicy.RequestVersionOrHigher;
                });
                clientService.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(
                    TimeSpan.FromMinutes(0.30),
                    Polly.Timeout.TimeoutStrategy.Pessimistic,
                    onTimeoutAsync: async (context, timespan, task) =>
                    {
                        _logger.LogWarning($"⏰ Request timed out after {timespan}");
                        await Task.CompletedTask;
                    }
                ));
                clientService.AddTransientHttpErrorPolicy(polly => polly.CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(60),
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
                    }
                ));
                clientService.AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(3, retryCount =>
                TimeSpan.FromSeconds(Math.Pow(2, retryCount)) + 
                TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100)),
                onRetryAsync:  (outcome, timespan, retrycount, task) =>
                {
                    Console.WriteLine($"⏰ Request timed out after {timespan}");
                    return Task.CompletedTask;
                }));
                clientService.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
                {
                    EnableMultipleHttp2Connections = true,

                    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
                    PooledConnectionLifetime = TimeSpan.FromMinutes(10),

                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,

                    MaxConnectionsPerServer = 10,
                    UseCookies = false,
                    AllowAutoRedirect = true,
                    MaxAutomaticRedirections = 15,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникла ошибка в теле настройки клиента" + ex.Message + "Место" + ex.StackTrace + "Полный стэк исключения" + ex.InnerException);
                return;
            }
        }
    }
}
