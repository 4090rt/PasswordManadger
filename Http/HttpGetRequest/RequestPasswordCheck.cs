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
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using static System.Net.WebRequestMethods;


namespace PasswordMenedger.Http.HttpGetRequest
{
    public class RequestPasswordCheck
    {
        private readonly ILogger<RequestPasswordCheck> _logger = LogFac.LoggerCreate<RequestPasswordCheck>();
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly IMemoryCache _memorycache;
        private  bool _seachpass = false;

        public RequestPasswordCheck(IHttpClientFactory httpClientFactory, IMemoryCache memorycache)
        { 
            _httpClientFactory = httpClientFactory;
            _memorycache = memorycache;
        }

        public async Task<List<CacheTestPassword>> CheckPasswordCheck(string password)
        {
            string cached_key = $"Cache_key_{password}";
            string stale_cache_key = $"stale_key_{cached_key}";
            List<CacheTestPassword> oldcache = null;

            if (_memorycache.TryGetValue(cached_key, out List<CacheTestPassword> cached) && cached != null)
            {
                oldcache = cached;
                return cached;
            }

            await _semaphoreSlim.WaitAsync().ConfigureAwait(false);

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
                onFallbackAsync: async (outcome, ctx ) =>
                {
                    _logger.LogError($"🆘 Fallback сработал: {outcome.Exception?.Message}");
                    await Task.CompletedTask;  
                });

                var fallbackresult = await fallback.ExecuteAsync(async () =>
                {
                    var result = await CheckPassword(password).ConfigureAwait(false);
                    List<CacheTestPassword> list = new List<CacheTestPassword>();
                    list.Add(new CacheTestPassword
                    {
                        checkPasswordsList = result.Item1,
                        checkPasswordEnabled = result.Item2,
                    });
                    if (list != null && list.Count > 0)
                    {
                        var cacheoptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));

                        _memorycache.Set(cached_key, list, cacheoptions);

                        var staleoptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(15))
                        .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                        _memorycache.Set(stale_cache_key, list, staleoptions);

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
                _semaphoreSlim.Release();
            }
        }

        public async Task<(List<CheckPassword>, bool)> CheckPassword(string password)
        {
            try
            {
                List<CheckPassword> checkPasswordsList = new List<CheckPassword>();
                var client = _httpClientFactory.CreateClient("Client1Http2.0");

                using var ShA1 = SHA1.Create();
                var bytes = Encoding.UTF8.GetBytes(password);
                var hashbytes = ShA1.ComputeHash(bytes);
                var hash = BitConverter.ToString(hashbytes).Replace("-", "").ToUpperInvariant();

                var prefix = hash.Substring(0, 5);
                var suffix = hash.Substring(5);

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
                        int count;
                        string passwordHash;
                        foreach (var line in lines)
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
                    _logger.LogError("Запрос вернул ошибку. Посткод:" + responseMessage.StatusCode);
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
