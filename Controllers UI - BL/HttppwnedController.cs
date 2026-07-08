using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PasswordMenedger.BusinesLogic.LoggerFac;
using PasswordMenedger.DataModel;
using PasswordMenedger.Http.HttpGetRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PasswordMenedger.Controllers_UI___BL
{
    public class HttppwnedController
    {
        public ILogger<HttppwnedController> _logger = LogFac.LoggerCreate<HttppwnedController>();
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _memorycache;

        private readonly RequestPasswordCheck _requestPasswordCheck;
        private readonly RequestPasswordCheckкrReserve _requestPasswordCheckReserve;

        public HttppwnedController (IHttpClientFactory httpClientFactory, IMemoryCache memorycache)
        {
            _httpClientFactory = httpClientFactory;
            _memorycache = memorycache;

            _requestPasswordCheck = new RequestPasswordCheck(_httpClientFactory, _memorycache);
            _requestPasswordCheckReserve = new RequestPasswordCheckкrReserve(_httpClientFactory, _memorycache);
        }

        public async Task<List<CacheTestPassword>> RequestpwnedAndReserve(string password)
        {
            try
            {
                if (!string.IsNullOrEmpty(password))
                {
                    var resultRequest = await _requestPasswordCheck.CheckPasswordCheck(password);

                    if (resultRequest != null && resultRequest.Count > 0)
                    {
                        _logger.LogInformation("Успешно получены данные с pwned");
                        return resultRequest;
                    }
                    else
                    {
                        _logger.LogWarning("Ответ от pwned пуст");
                        return new List<CacheTestPassword>();
                    }
                }
                else
                {
                    _logger.LogWarning("Пароль пуст");
                    return new List<CacheTestPassword>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Возникло исключение при попытке запроса к pwned" + ex.Message + " Перехожу на резервный запрос");
                try
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        var resultRequestReserve = await _requestPasswordCheckReserve.CheckPasswordReserveCache(password);

                        if (resultRequestReserve != null && resultRequestReserve.Count > 0)
                        {
                            _logger.LogInformation("Успешно получены данные с pwned резерв");
                            return resultRequestReserve;
                        }
                        else
                        {
                            _logger.LogWarning("Ответ от pwned пуст резерв");
                            return new List<CacheTestPassword>();
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Пароль пуст");
                        return new List<CacheTestPassword>();
                    }
                }
                catch (Exception ex2)
                {
                    _logger.LogWarning("Оба запросы прерваличь исключением" + ex.Message + ex.StackTrace + ex.InnerException);
                    return new List<CacheTestPassword>();
                }
            }
        }
    }
}
