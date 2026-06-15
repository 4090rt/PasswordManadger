using Microsoft.Extensions.Logging;
using PasswordMenedger.BusinesLogic.LoggerFac;
using PasswordMenedger.Controllers_UI___BL.SaveRequestBd;
using PasswordMenedger.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PasswordMenedger.Controllers_UI___BL
{
    public class SavePasswordController
    {
        private readonly ILogger<SavePasswordModel> _logger = LogFac.LoggerCreate<SavePasswordModel>();
        private readonly InsertPassword _insertPassword;

        public SavePasswordController(InsertPassword insertPassword)
        { 
            _insertPassword = insertPassword;
        }

        public async Task SavePasswords(SavePasswordModel savePasswordModel)
        {
            try
            {
                if (savePasswordModel != null && savePasswordModel.Password.Length > 8)
                {
                    await _insertPassword.SaveMethod(savePasswordModel).ConfigureAwait(false);
                }
                else
                {

                    MessageBox.Show("Данные пусты");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение при попытке сохраениня пароля" + ex.Message + ex.InnerException + ex.StackTrace);
            }
        }
    }
}
