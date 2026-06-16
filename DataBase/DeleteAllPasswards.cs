using Messangers.SQLite.DbPath;
using Microsoft.Extensions.Logging;
using PasswordMenedger.BusinesLogic.LoggerFac;
using PasswordMenedger.DataBase.AllPasswordsSelect;
using PasswordMenedger.DataBase.PoolSQLiteConnection;
using PasswordMenedger.DataModel;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordMenedger.DataBase
{
    class DeleteAllPasswards
    {
        private readonly ILogger<DeleteAllPasswards> _logger = LogFac.LoggerCreate<DeleteAllPasswards>();
        private readonly ILogger<PoolSQLite> _loggerpool = LogFac.LoggerCreate<PoolSQLite>();

        private readonly PoolSQLite _pool;
        private readonly DbPathClass _dbpath;

        public DeleteAllPasswards()
        {
            _dbpath = new DbPathClass();
            _pool = new PoolSQLite(_loggerpool, _dbpath);
        }

        public async Task Requst()
        {
            SQLiteConnection connection = null;
            SQLiteTransaction transaction = null;
            try
            {
                connection = _pool.ConntectionOpen();
                transaction = connection.BeginTransaction();

                string command = "DELETE * PasswordBase";

                await using (var sqlcommand = new SQLiteCommand(command, connection, transaction))
                { 
                    await sqlcommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
                await transaction.CommitAsync().ConfigureAwait(false);
            }
            catch (SQLiteException ex)
            {
                _logger.LogError("Возинкло исключение при попытке запроса списка паролей" + ex.Message + ex.InnerException + ex.StackTrace);
                await (transaction?.RollbackAsync() ?? Task.CompletedTask);
                throw new Exception();
            }
            catch (Exception ex)
            {
                _logger.LogError("Возинкло необработанное исключение при попытке запроса списка паролей" + ex.Message + ex.InnerException + ex.StackTrace);
                await (transaction?.RollbackAsync() ?? Task.CompletedTask);
                throw new Exception();
            }
            finally
            {
                if (connection != null)
                {
                    _pool.ConnectionClose(connection);
                }
                if (transaction != null)
                {
                    transaction.Dispose();
                }
            }
        }
    }
}
