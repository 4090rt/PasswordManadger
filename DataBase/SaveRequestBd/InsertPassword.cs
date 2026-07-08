using Microsoft.Extensions.Logging;
using PasswordMenedger.BusinesLogic.LoggerFac;
using PasswordMenedger.DataBase.PoolSQLiteConnection;
using PasswordMenedger.DataModel;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PasswordMenedger.Controllers_UI___BL.SaveRequestBd
{
    public class InsertPassword
    {
        private readonly ILogger<InsertPassword> _logger = LogFac.LoggerCreate<InsertPassword>();

        private readonly PoolSQLite _poolSQLite;

        public InsertPassword(PoolSQLite poolSQLite)
        { 
            _poolSQLite = poolSQLite;   
        }

        public async Task<bool> SaveMethod(SavePasswordModel savePasswordModel)
        {
            SQLiteConnection connection = null;
            SQLiteTransaction transaction = null;
            try
            {
                connection = _poolSQLite.ConntectionOpen(); 
                transaction = connection.BeginTransaction();

                string command = "INSERT INTO [PasswordBase] (Name, URL, Password, Date, Icon) VALUES (@N, @U, @P, @D, @I)";

                await using (var sqlcommand = new SQLiteCommand(command, connection, transaction))
                {
                    sqlcommand.Parameters.AddWithValue("@N", savePasswordModel.Name);
                    sqlcommand.Parameters.AddWithValue("@U", savePasswordModel.URL);
                    sqlcommand.Parameters.AddWithValue("@P", savePasswordModel.Password);
                    sqlcommand.Parameters.AddWithValue("@D", savePasswordModel.Date);
                    sqlcommand.Parameters.AddWithValue("@I", savePasswordModel.Icon);

                    var result = await sqlcommand.ExecuteNonQueryAsync().ConfigureAwait(false);

                    if (result != null)
                    {
                        await transaction.CommitAsync().ConfigureAwait(false);
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("не сохранено");
                        return true;
                    }
                }
            }
            catch (SQLiteException ex)
            {
                if (ex.ResultCode == SQLiteErrorCode.Constraint_Unique)
                {
                    MessageBox.Show("Этот пароль  уже был сгенерирован и сохранен. Пожалуйста перегенерируйте пароль");
                    await (transaction?.RollbackAsync() ?? Task.CompletedTask);
                    return false;
                }
                _logger.LogError("Не удалось сохранить пароль" + ex.Message + ex.InnerException + ex.StackTrace);
                await (transaction?.RollbackAsync() ?? Task.CompletedTask);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError("Не удалось сохранить пароль. Необработанное исключение" + ex.Message + ex.InnerException + ex.StackTrace);
                await (transaction?.RollbackAsync() ?? Task.CompletedTask);
                return false;
            }
            finally
            {
                if (connection != null)
                {
                    _poolSQLite.ConnectionClose(connection);
                }
                if (transaction != null)
                {
                    transaction.Dispose();
                }
            }
        }
    }
}
