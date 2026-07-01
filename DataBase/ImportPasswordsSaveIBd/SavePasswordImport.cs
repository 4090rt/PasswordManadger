using Messangers.SQLite.DbPath;
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

namespace PasswordMenedger.DataBase.ImportPasswordsSaveIBd
{
    public class SavePassword
    {
        private readonly ILogger<SavePassword> _logger = LogFac.LoggerCreate<SavePassword>();
        private readonly ILogger<PoolSQLite> _loggerpool = LogFac.LoggerCreate<PoolSQLite>();

        private readonly PoolSQLite _poolSQLite;
        private readonly DbPathClass _dbPathClass;

        public SavePassword()
        {
            _dbPathClass = new DbPathClass();
            _poolSQLite = new PoolSQLite(_loggerpool, _dbPathClass);
        }

        public async Task SaveImportPasswords(List<SavePasswordModel> list)
        {
            SQLiteConnection connection = null;
            try
            {
                connection = _poolSQLite.ConntectionOpen();

                string command = "INSERT INTO [PasswordBase] (Name, URL, Password, Date) VALUES (@N, @U, @P, @D)";

                await using (var sqlcommand = new SQLiteCommand(command, connection))
                {
                    foreach (var item in list)
                    {
                        sqlcommand.Parameters.AddWithValue("@N", item.Name);
                        sqlcommand.Parameters.AddWithValue("@U", item.URL);
                        sqlcommand.Parameters.AddWithValue("@P", item.Password);
                        sqlcommand.Parameters.AddWithValue("D", item.Date);

                        try
                        {
                            await sqlcommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                        catch (SQLiteException ex)
                        {
                            if (ex.ResultCode == SQLiteErrorCode.Constraint_Unique)
                            {
                                continue;
                            }
                        }
                    }
                    MessageBox.Show("Сохранено в бд!1" + list.Count);
                }
            }
            catch (SQLiteException ex)
            {
                if (ex.ResultCode == SQLiteErrorCode.Constraint_Unique)
                {
                    return;   
                }
                _logger.LogError("Возникло SQL исключение при сохранении в бд импортных паролей" + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло  исключение при сохранении в бд импортных паролей" + ex.Message);
            }
            finally
            {
                if (connection != null)
                { 
                    _poolSQLite.ConnectionClose(connection);
                }
            }
        }
    }
}
