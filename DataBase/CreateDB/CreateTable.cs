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
using System.Windows.Documents;

namespace PasswordMenedger.Controllers_UI___BL.CreateDB
{
    public class CreateTable
    {
        private readonly ILogger<CreateTable> _logger = LogFac.LoggerCreate<CreateTable>();
        private readonly ILogger<PoolSQLite> _loggerpool = LogFac.LoggerCreate<PoolSQLite>();
        private readonly ILogger<CreateTablePassword> _loggerpassword = LogFac.LoggerCreate<CreateTablePassword>();

        private DbPathClass _dbpath;
        private PoolSQLite _poolSQLite;
        private CreateTablePassword _password;

        public async Task CreateTableMethod()
        {
            try
            {
                _dbpath = new DbPathClass();
                _poolSQLite = new PoolSQLite(_loggerpool, _dbpath);
                _password = new CreateTablePassword(_loggerpassword, _poolSQLite);

                await _password.CreateTableCheck();
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение при создании таблицы" + ex.Message + ex.StackTrace);
            }
        }
    }
}
