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
    public class HttpClientSettnigsClouadSynchronisationReserve
    {
        private readonly ILogger<HttpClientSettnigsClouadSynchronisationReserve> _logger = LogFac.LoggerCreate<HttpClientSettnigsClouadSynchronisationReserve>();

        public void ClientCloudSettingsReserve(IServiceCollection serviceDescriptors)
        {
            try
            {
                var clientServiceReserve = serviceDescriptors.AddHttpClient("CloudClientReserve", clientCloudRserve =>
                {
                    clientCloudRserve.DefaultRequestHeaders.Accept.ParseAdd("application/json");
                    clientCloudRserve.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
                    clientCloudRserve.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

                    clientCloudRserve.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth", "8cb29e75d113407da73eddc9e3c1cc1d");

                    clientCloudRserve.DefaultRequestVersion = HttpVersion.Version20;
                    clientCloudRserve.DefaultVersionPolicy = System.Net.Http.HttpVersionPolicy.RequestVersionOrHigher;
                });
                clientServiceReserve.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(
                    TimeSpan.FromMinutes(0.30),
                    Polly.Timeout.TimeoutStrategy.Pessimistic,
                    onTimeoutAsync: async (context, timespan, task) =>
                    {
                        _logger.LogWarning($"⏰ Request timed out after {timespan}");
                        await Task.CompletedTask;
                    }));
                clientServiceReserve.AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(
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
                    }));
                clientServiceReserve.AddTransientHttpErrorPolicy(polly => polly.WaitAndRetryAsync(3, retryCount =>
                TimeSpan.FromSeconds(Math.Pow(2, retryCount)) +
                TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100)),
                onRetryAsync: (outcome, timespan, retrycount, task) =>
                {
                    Console.WriteLine($"⏰ Request timed out after {timespan}");
                    return Task.CompletedTask;
                }));
                clientServiceReserve.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
                {
                    EnableMultipleHttp2Connections = true,

                    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
                    PooledConnectionLifetime = TimeSpan.FromMinutes(10),

                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,

                    MaxConnectionsPerServer = 10,
                    AllowAutoRedirect = true,
                    UseCookies = false, 
                    MaxAutomaticRedirections = 10
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
