using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PasswordMenedger.BusinesLogic.LoggerFac;
using PasswordMenedger.DataModel;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace PasswordMenedger.Http.HttpGetRequest
{
    public class RequestPasswordCheckкrReserve
    {
        private readonly ILogger<RequestPasswordCheckкrReserve> _logger = LogFac.LoggerCreate<RequestPasswordCheckкrReserve>();
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _memorycache;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private bool _seachpass = false;

        public RequestPasswordCheckкrReserve(IHttpClientFactory httpClientFactory, IMemoryCache memorycache)
        { 
            _httpClientFactory = httpClientFactory;
            _memorycache = memorycache;
        }

        public async Task<List<CacheTestPassword>> CheckPasswordReserveCache(string password)
        {
            string cached_key = $"Cache_key_Reserve{password}";
            string stale_cache_key = $"stale_key_Reserve{cached_key}";
            List<CacheTestPassword> oldcache = null;

            if (_memorycache.TryGetValue(cached_key, out List<CacheTestPassword> cached) && cached != null)
            { 
                oldcache = cached;
                return cached;
            }

            await _semaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                if (_memorycache.TryGetValue(cached_key, out List<CacheTestPassword> cached2) && cached2 != null)
                {
                    oldcache = cached2;
                    return cached2; 
                }


                var fallback = Policy<List<CacheTestPassword>>
                .Handle<Exception>()
                .OrResult(r => r == null)
                .FallbackAsync(
                fallbackAction: async (outcome, context, ctx) =>
                {
                    var eception = outcome.Exception;
                    var isEmpty = outcome.Result == null;

                    if (eception != null)
                    {
                        _logger.LogWarning($"⚠️ Fallback by exception: {eception.Message}");
                    }
                    if (isEmpty)
                    {
                        _logger.LogWarning($"⚠️ Fallback by empty result");
                    }
                    if (oldcache != null)
                    {
                        _logger.LogInformation("✅ Fallback: возвращаю старые данные из кэша");
                        return oldcache;
                    }
                    if (_memorycache.TryGetValue(stale_cache_key, out List<CacheTestPassword> stalecached))
                    {
                        _logger.LogInformation($"✅ Returning stale copy for {stalecached}");
                        return stalecached;
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ Fallback: кэш пуст, возвращаю default");
                        return default;
                    }
                },
                onFallbackAsync: async (outcome, ctx) =>
                {
                    _logger.LogError($"🆘 Fallback сработал: {outcome.Exception?.Message}");
                    await Task.CompletedTask;
                });

                var fallbackresult = await fallback.ExecuteAsync(async () =>
                {
                    var result = await CheckPasswordReserve(password).ConfigureAwait(false);
                    List<CacheTestPassword> list = new List<CacheTestPassword>();
                    list.Add(new CacheTestPassword
                    {
                        checkPasswordsList = result.Item1,
                        checkPasswordEnabled = result.Item2,
                    });

                    if (list != null || list.Count > 0)
                    {
                        var optionscache = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(15))
                        .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                        _memorycache.Set(cached_key, list, optionscache);

                        var optionsstale = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(15))
                        .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                        _memorycache.Set(stale_cache_key, list, optionsstale);

                        return list;
                    }
                    else
                    {
                        _logger.LogError("Результат запроса пуст");
                        return default;
                    }
                });
                return fallbackresult;
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло необработанное исключение" + ex.Message + ex.StackTrace + ex.InnerException);
                return new List<CacheTestPassword>();
            }
            finally
            {
                 _semaphore.Release();
            }
        }

        public async Task<(List<CheckPassword>, bool)> CheckPasswordReserve(string password)
        {
            try
            {
                List<CheckPassword> checkPasswordsList = new List<CheckPassword>();

                var client = _httpClientFactory.CreateClient("ClientReserve");

                var Sha1 = SHA1.Create();
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                var sha1S = Sha1.ComputeHash(bytes);
                var allbytesHash = BitConverter.ToString(sha1S).Replace("-", "").ToUpperInvariant();

                var prefix = allbytesHash.Substring(0, 5);
                var suffix = allbytesHash.Substring(5, 0);

                var options = new HttpRequestMessage(HttpMethod.Get, $"https://api.pwnedpasswords.com/range/{prefix}")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                HttpResponseMessage response = await client.SendAsync(options).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (result != null)
                    {
                        int count;
                        string passwordHash;
                        var items = result.Split('\n');
                        foreach (var line in items)
                        {
                            if (string.IsNullOrWhiteSpace(line)) continue;
                            var parts = line.Split(':');
                            if (parts.Length == 2)
                            {
                                foreach (var part in parts)
                                {
                                    var fullHash = parts[0].Trim();
                                    passwordHash = parts[0].ToString();
                                    count = int.Parse(parts[1]);
                                    var objects = new CheckPassword
                                    {
                                        PasswordHash = passwordHash,
                                        Count = count
                                    };
                                    checkPasswordsList.Add(objects);
                                    if (fullHash == suffix)
                                    {
                                        _seachpass = true;
                                    }
                                }
                            }
                        }
                        if (_seachpass == true)
                        {
                            return (checkPasswordsList, true);
                        }
                        else
                        {
                            return (checkPasswordsList, false);
                        }
                    }
                    else
                    {
                        _logger.LogError("Ответ null");
                        return (new List<CheckPassword>(), false);
                    }
                }
                else
                {
                    _logger.LogError("Запрос вернул ошибку. Посткод:" + response.StatusCode);
                    return (new List<CheckPassword>(), false);
                }
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError("Операция отменена" + ex.Message + " Место" + ex.StackTrace + " полный стек" + ex.InnerException);
                return (new List<CheckPassword>(), false);
            }
            catch (JsonException ex)
            {
                _logger.LogError("json исключение" + ex.Message + " Место" + ex.StackTrace + " полный стек" + ex.InnerException);
                return (new List<CheckPassword>(), false);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("HTTP Иисключение" + ex.Message + " Место" + ex.StackTrace + " полный стек" + ex.InnerException);
                return (new List<CheckPassword>(), false);
            }
            catch (Exception ex)
            {
                _logger.LogError("Необработанное исключение" + ex.Message + " Место" + ex.StackTrace + " полный стек" + ex.InnerException);
                return (new List<CheckPassword>(), false);
            }
        }
    }
}
