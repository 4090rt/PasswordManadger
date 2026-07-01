using Messangers.SQLite.DbPath;
using Microsoft.Extensions.Logging;
using PasswordMenedger.BusinesLogic.LoggerFac;
using PasswordMenedger.DataBase;
using PasswordMenedger.DataBase.PoolSQLiteConnection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PasswordMenedger.Controllers_UI___BL
{
    public class DeleteAllPasswordsController
    {
        private readonly ILogger<DeleteAllPasswordsController> _logger = LogFac.LoggerCreate<DeleteAllPasswordsController>();
        private readonly DeleteAllPasswards _deleteAllPasswards;

        public DeleteAllPasswordsController()
        { 
            _deleteAllPasswards = new DeleteAllPasswards();
        }

        public async Task DeletePasswordsController()
        {
            try
            {
                await _deleteAllPasswards.Requst().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение при попытке удаления всех паролей" + ex.Message + ex.StackTrace + ex.InnerException);
            }
        }
    }
}
