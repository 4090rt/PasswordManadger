using Messangers.SQLite.DbPath;
using Microsoft.Extensions.Logging;
using PasswordMenedger.BusinesLogic.LoggerFac;
using PasswordMenedger.DataBase.PoolSQLiteConnection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordMenedger.DataBase.UpdateUserIcon
{
    public class UpdateUsersI
    {
        private readonly ILogger<UpdateUsersI> _logger = LogFac.LoggerCreate<UpdateUsersI>();
        private readonly ILogger<PoolSQLite> _loggerpool = LogFac.LoggerCreate<PoolSQLite>();

        private readonly PoolSQLite _pool;
        private readonly DbPathClass _dbPathClass;

        public UpdateUsersI()
        {
            _dbPathClass = new DbPathClass();
            _pool = new PoolSQLite(_loggerpool,_dbPathClass);
        }

        public async Task UpdateClass(int id, byte[] bytes)
        {
            SQLiteConnection connection = null;
            SQLiteTransaction transaction = null;
            try
            {
                connection = _pool.ConntectionOpen();
                transaction = connection.BeginTransaction();

                string command = "UPDATE PasswordBase SET IconUser = @I WHERE Id = @Id";

                await using (var sqlcommand = new SQLiteCommand(command, connection, transaction))
                { 
                    sqlcommand.Parameters.AddWithValue("@Id", id);
                    sqlcommand.Parameters.AddWithValue("@I", bytes);

                    await sqlcommand.ExecuteNonQueryAsync().ConfigureAwait(false);

                    await transaction.CommitAsync().ConfigureAwait(false);
                }
            }
            catch (SQLiteException ex)
            {
                _logger.LogError("Возникло SQL исключение при обновлении пользователськой иконки" + ex.Message + ex.StackTrace + ex.InnerException);
                try
                {
                    if (transaction != null && transaction.Connection != null &&
                        transaction.Connection.State == ConnectionState.Open)
                    {
                        await transaction.RollbackAsync().ConfigureAwait(false);
                        _logger.LogInformation("Транзакция успешно откачена");
                    }
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError($"Ошибка при откате транзакции: {rollbackEx.Message}", rollbackEx);
                }
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение при обновлении пользователськой иконки" + ex.Message + ex.StackTrace + ex.InnerException);
                try
                {
                    if (transaction != null && transaction.Connection != null &&
                        transaction.Connection.State == ConnectionState.Open)
                    {
                        await transaction.RollbackAsync().ConfigureAwait(false);
                        _logger.LogInformation("Транзакция успешно откачена");
                    }
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError($"Ошибка при откате транзакции: {rollbackEx.Message}", rollbackEx);
                }
                return;
            }
            finally
            {
                if (connection != null && transaction != null)
                { 
                    _pool.ConnectionClose(connection);
                    transaction.Dispose();
                }
            }
        }
    }
}
