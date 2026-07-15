using Messangers.SQLite.DbPath;
using Microsoft.Extensions.Logging;
using PasswordMenedger.BusinesLogic.LoggerFac;
using PasswordMenedger.DataBase.PoolSQLiteConnection;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordMenedger.DataBase.UpdateServiceName
{
    public class UpdateName
    {
        private readonly ILogger<UpdateName> _logger = LogFac.LoggerCreate<UpdateName>();
        private readonly ILogger<PoolSQLite> _loggerpool = LogFac.LoggerCreate<PoolSQLite>();

        private readonly DbPathClass _dbPathClass;
        private readonly PoolSQLite _poolSQLite;

        public UpdateName()
        { 
            _dbPathClass = new DbPathClass();
            _poolSQLite = new PoolSQLite(_loggerpool, _dbPathClass);
        }

        public async Task UpdateMethod(int id, string newname)
        {
            SQLiteConnection connection = null;
            SQLiteTransaction transaction = null;
            try
            {
                connection = _poolSQLite.ConntectionOpen();
                transaction  = connection.BeginTransaction();

                string command = "UPDATE PasswordBase SET Name = @N WHERE Id = @Id";

                await using (var sqlcommand = new SQLiteCommand(command, connection, transaction))
                {
                    sqlcommand.Parameters.AddWithValue("@N", newname);
                    sqlcommand.Parameters.AddWithValue("@Id", id);

                    await sqlcommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                    await transaction.CommitAsync().ConfigureAwait(false);
                }
            }
            catch (SQLiteException ex)
            {
                _logger.LogError("Возникло SQL исключение при обновлении имени сервиса" + ex.Message + ex.StackTrace + ex.InnerException);
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
                _logger.LogError("Возникло исключение при обновлении имени сервиса" + ex.Message + ex.StackTrace + ex.InnerException);
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
