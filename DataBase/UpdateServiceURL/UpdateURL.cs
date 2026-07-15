using Messangers.SQLite.DbPath;
using Microsoft.Extensions.Logging;
using PasswordMenedger.BusinesLogic.LoggerFac;
using PasswordMenedger.DataBase.PoolSQLiteConnection;
using PasswordMenedger.DataBase.UpdateServiceName;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordMenedger.DataBase.UpdateServiceURL
{
    public class UpdateURL
    {
        private readonly ILogger<UpdateURL> _logger = LogFac.LoggerCreate<UpdateURL>();
        private readonly ILogger<PoolSQLite> _loggerpool = LogFac.LoggerCreate<PoolSQLite>();

        private readonly DbPathClass _dbPathClass;
        private readonly PoolSQLite _poolSQLite;

        public UpdateURL()
        {
            _dbPathClass = new DbPathClass();
            _poolSQLite = new PoolSQLite(_loggerpool, _dbPathClass);
        }

        public async Task UpdateMethod(int id, string newurl)
        {
            SQLiteConnection connection = null;
            SQLiteTransaction transaction = null;
            try
            {
                connection = _poolSQLite.ConntectionOpen();
                transaction = connection.BeginTransaction();

                string command = "UPDATE PasswordBase SET URL = @URL WHERE Id = @Id";

                await using (var sqlcommand = new SQLiteCommand(command, connection, transaction))
                { 
                    sqlcommand.Parameters.AddWithValue("@Id", id);
                    sqlcommand.Parameters.AddWithValue("@URL", newurl);

                    await sqlcommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                    await transaction.CommitAsync().ConfigureAwait(false);
                }
            }
            catch (SQLiteException ex)
            {
                _logger.LogError("Возникло SQL исключение при обновлении url сервиса" + ex.Message + ex.StackTrace + ex.InnerException);
                try
                {
                    if (transaction != null && transaction.Connection != null &&
                        transaction.Connection.State == System.Data.ConnectionState.Open)
                    {
                        await transaction.RollbackAsync().ConfigureAwait(false);
                        _logger.LogInformation("Транзакция успешно откачена");
                    }
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError($"Ошибка при откате транзакции: {rollbackEx.Message}", rollbackEx);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение при обновлении url сервиса" + ex.Message + ex.StackTrace + ex.InnerException);
                try
                {
                    if (transaction != null && transaction.Connection != null &&
                        transaction.Connection.State == System.Data.ConnectionState.Open)
                    { 
                        await transaction.RollbackAsync().ConfigureAwait(false);
                        _logger.LogInformation("Транзакция успешно откачена");
                    }
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError($"Ошибка при откате транзакции: {rollbackEx.Message}", rollbackEx);
                }
            }
            finally
            {
                if (connection != null && transaction != null)
                { 
                    _poolSQLite.ConnectionClose(connection);
                    transaction.Dispose();
                }
            }
        }
    }
}
