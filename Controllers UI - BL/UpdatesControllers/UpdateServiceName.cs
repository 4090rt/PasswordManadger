using Microsoft.Extensions.Logging;
using PasswordMenedger.BusinesLogic.LoggerFac;
using PasswordMenedger.DataBase.UpdateServiceName;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordMenedger.Controllers_UI___BL.UpdatesControllers
{
    public class UpdateServiceName
    {
        private readonly ILogger<UpdateServiceName> _logger = LogFac.LoggerCreate<UpdateServiceName>();
        private readonly UpdateName _updateName;
        public UpdateServiceName()
        { 
            _updateName = new UpdateName();
        }

        public async Task UpdateName(int id, string newname)
        {
            try
            {
                if (id >= 0 && !string.IsNullOrEmpty(newname))
                {
                    await _updateName.UpdateMethod(id, newname);
                }
                else
                {
                    _logger.LogError("Возникла ошибка при попытке обновления имени сервиса. Данные пусты");
                    return;
                }
            }
            catch(Exception ex)
            {
                _logger.LogError("Возникло исключение в контроллере при поыптке обновить имя сервиса" + ex.Message + ex.StackTrace + ex.InnerException);
                return;
            }
        }
    }
}
