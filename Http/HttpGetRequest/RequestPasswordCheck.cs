using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PasswordMenedger.BusinesLogic.LoggerFac;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace PasswordMenedger.Http.HttpGetRequest
{
    public class RequestPasswordCheck
    {
        private readonly ILogger<RequestPasswordCheck> _logger = LogFac.LoggerCreate<RequestPasswordCheck>();
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1,1);

        public RequestPasswordCheck(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
        { 
            _httpClientFactory = httpClientFactory;
            _memoryCache = memoryCache;
        }

        public async Task<bool> CheckPassword(string password)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Client1Http2.0");

                using var ShA1 = SHA1.Create();
                var bytes = Encoding.UTF8.GetBytes(password);
                var hashbytes = ShA1.ComputeHash(bytes);
                var hash = BitConverter.ToString(hashbytes).Replace("-", "").ToUpperInvariant();

                var prefix = hash.Substring(0, 5);
                var suffics = hash.Substring(5,0);

                var options = new HttpRequestMessage(HttpMethod.Get, $"https://api.pwnedpasswords.com/range/{prefix}")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                HttpResponseMessage responseMessage = await client.SendAsync(options).ConfigureAwait(false);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var result = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (result != null)
                    {
                        var lines = result.Split('\n');

                        foreach (var line in lines)
                        {
                            var parts = line.Split(':');
                            if (parts.Length == 2 && parts[0].Trim() == suffics)
                            {
                                var count = int.Parse(parts[1].Trim());
                            }
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    _logger.LogError("Возникла ошибка при попытке проврки пароля. Статус код:" + responseMessage.StatusCode);
                    return false;
                }
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError("Операция отменена" + ex.Message + " Место" + ex.StackTrace + " полный стек" + ex.InnerException);
                return false;
            }
            catch (JsonException ex)
            {
                _logger.LogError("json исключение" + ex.Message + " Место" + ex.StackTrace + " полный стек" + ex.InnerException);
                return false;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("HTTP Иисключение" + ex.Message + " Место" + ex.StackTrace + " полный стек" + ex.InnerException);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError("Необработанное исключение" + ex.Message + " Место" + ex.StackTrace + " полный стек" + ex.InnerException);
                return false;
            }
        }
    }
}
