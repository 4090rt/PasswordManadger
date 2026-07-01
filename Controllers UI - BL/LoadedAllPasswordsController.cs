using Microsoft.Extensions.Logging;
using PasswordMenedger.BusinesLogic.LoggerFac;
using PasswordMenedger.DataBase.AllPasswordsSelect;
using PasswordMenedger.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordMenedger.Controllers_UI___BL
{
    public class LoadedAllPasswordsController
    {
        private readonly ILogger<LoadedAllPasswordsController> _logger = LogFac.LoggerCreate<LoadedAllPasswordsController>();
        private readonly SelectPasswords _selectPasswords;

        public LoadedAllPasswordsController()
        { 
            _selectPasswords = new SelectPasswords();
        }

        public async Task<List<SavePasswordModel>> RequestAllPasswords()
        {
            try
            {
                List<SavePasswordModel> list =  await _selectPasswords.Request().ConfigureAwait(false);
                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение в контроллере при попытке запроса всех паролей");
                return new List<SavePasswordModel>();
            }
        }
    }
}
