using Microsoft.Extensions.Logging;
using PasswordMenedger.BusinesLogic.LoggerFac;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PasswordMenedger.Http.HttpGetRequest
{
    public class GetIcoт
    {
        private readonly ILogger<GetIcoт> _logger = LogFac.LoggerCreate<GetIcoт>();
        private readonly IHttpClientFactory _httpClientFactory;

        public GetIcoт(IHttpClientFactory httpClientFactory)
        { 
            _httpClientFactory = httpClientFactory;
        }

        public async Task<byte[]> RequestIcon(string domain)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Client1Http2.0");

                var options = new HttpRequestMessage(HttpMethod.Get, $"https://www.google.com/s2/favicons?domain={domain}.com")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                HttpResponseMessage response = await client.SendAsync(options).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    byte[] bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                    if (bytes != null)
                    {
                        return bytes;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    _logger.LogWarning("Запрос иконки вернул ошибку. статускод:" + response.StatusCode);
                    return null;
                }
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError("Превышено время ожидания" + ex.Message + ex.InnerException + ex.StackTrace);
                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Возникло htpp исключение" + ex.Message + ex.InnerException + ex.StackTrace);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError("Неизвестное исключение" + ex.Message + ex.InnerException + ex.StackTrace);
                return null;
            }
        }
    }
}
