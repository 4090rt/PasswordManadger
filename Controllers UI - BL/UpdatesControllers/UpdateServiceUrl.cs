using Microsoft.Extensions.Logging;
using PasswordMenedger.BusinesLogic.LoggerFac;
using PasswordMenedger.DataBase.UpdateServiceURL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordMenedger.Controllers_UI___BL.UpdatesControllers
{
    public class UpdateServiceUrl
    {
        private readonly ILogger<UpdateServiceUrl> _logger = LogFac.LoggerCreate<UpdateServiceUrl>();
        private readonly UpdateURL _urlUpdate;

        public UpdateServiceUrl()
        { 
            _urlUpdate = new UpdateURL();
        }

        public async Task UpdateUrl(int id, string newUrl)
        {
            try
            {
                if (id > 0 && !string.IsNullOrEmpty(newUrl))
                {
                    await _urlUpdate.UpdateMethod(id, newUrl);
                }
                else
                {
                    _logger.LogError("Возникла ошибка при попытке обновления url сервиса. Данные пусты");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение в контроллере при попытке обновить url сервиса" + ex.Message + ex.StackTrace + ex.InnerException);
                return;
            }
        }
    }
}
