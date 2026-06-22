using Messangers.SQLite.DbPath;
using Microsoft.Extensions.Logging;
using PasswordMenedger.BusinesLogic.LoggerFac;
using PasswordMenedger.DataBase.AllPasswordsSelect;
using PasswordMenedger.DataBase.PoolSQLiteConnection;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordMenedger.DataBase
{
    public class DeleteConcretePassword
    {
        private readonly ILogger<DeleteConcretePassword> _logger = LogFac.LoggerCreate<DeleteConcretePassword>();
        private readonly ILogger<PoolSQLite> _loggerpool = LogFac.LoggerCreate<PoolSQLite>();

        private readonly PoolSQLite _pool;
        private readonly DbPathClass _dbpath;

        public DeleteConcretePassword()
        {
            _dbpath = new DbPathClass();
            _pool = new PoolSQLite(_loggerpool, _dbpath);
        }

        public async Task<bool> DeleteConcrectPass(int id)
        {
            SQLiteConnection connection = null;
            SQLiteTransaction transaction = null;
            try
            {
                connection = _pool.ConntectionOpen();
                transaction = connection.BeginTransaction();

                string command = "DELETE FROM  PasswordBase WHERE Id = @id";

                await using (var commandsq = new SQLiteCommand(command, connection, transaction))
                { 
                    commandsq.Parameters.AddWithValue("id", id);

                    var result = await commandsq.ExecuteNonQueryAsync().ConfigureAwait(false);
                    if (result > 0)
                    {
                        await transaction.CommitAsync().ConfigureAwait(false);
                        _logger.LogInformation("Удалено");
                        return true;
                    }
                    else
                    {
                        _logger.LogError("Ошибка удаления");
                        return false;
                    }
                }
            }
            catch (SQLiteException ex)
            {
                _logger.LogError("Возникло SQL исключение при выполениии удаления пароля" + ex.Message + ex.InnerException + ex.StackTrace);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло НЕОБРАБОТАННОЕ исключение при выполениии удаления пароля" + ex.Message + ex.InnerException + ex.StackTrace);
                return false;
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
