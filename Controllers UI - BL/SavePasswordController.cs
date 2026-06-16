using Messangers.SQLite.DbPath;
using Microsoft.Extensions.Logging;
using PasswordMenedger.BusinesLogic.LoggerFac;
using PasswordMenedger.Controllers_UI___BL.SaveRequestBd;
using PasswordMenedger.DataBase.PoolSQLiteConnection;
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
        private readonly ILogger<PoolSQLite> _loggerpool = LogFac.LoggerCreate<PoolSQLite>();

        private readonly InsertPassword _insertPassword;
        private readonly DbPathClass _dbPathClass;
        private readonly PoolSQLite _pool;
        public SavePasswordController()
        {
            _dbPathClass = new DbPathClass();
            _pool = new PoolSQLite(_loggerpool, _dbPathClass);
            _insertPassword = new InsertPassword(_pool);
        }

        public async Task SavePasswords(SavePasswordModel savePasswordModel)
        {
            try
            {
                if (savePasswordModel != null && savePasswordModel.Password.Length > 8)
                {
                    await _insertPassword.SaveMethod(savePasswordModel).ConfigureAwait(false);
                    MessageBox.Show("Сохраняю");
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
