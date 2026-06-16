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

namespace PasswordMenedger.DataBase.AllPasswordsSelect
{
    public class SelectPasswords
    {
        private readonly ILogger<SelectPasswords> _logger = LogFac.LoggerCreate<SelectPasswords>();
        private readonly ILogger<PoolSQLite> _loggerpool = LogFac.LoggerCreate<PoolSQLite>();

        private readonly PoolSQLite _pool;
        private readonly DbPathClass _dbpath;

        public SelectPasswords()
        {
            _dbpath = new DbPathClass();
            _pool = new PoolSQLite(_loggerpool,_dbpath);
        }

        public async Task<List<SavePasswordModel>> Request()
        {
            SQLiteConnection connection = null;
            List<SavePasswordModel> list = null;
            try
            {
                connection = _pool.ConntectionOpen();

                string command = "SELECT * FROM PasswordBase";

                await using (var sqlcommand = new SQLiteCommand(command, connection))
                {
                    await using (var read = await sqlcommand.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        if (read != null)
                        {
                            var name = read.GetOrdinal("Name");
                            var url = read.GetOrdinal("URL");
                            var password = read.GetOrdinal("Password");
                            var date = read.GetOrdinal("Date");

                            while (await read.ReadAsync().ConfigureAwait(false))
                            {
                                var data = new SavePasswordModel
                                {
                                    Name = read.IsDBNull(name) ? "" : read.GetString(name),
                                    URL = read.IsDBNull(url) ? "" : read.GetString(url),
                                    Password = read.IsDBNull(password) ? "" : read.GetString(password),
                                    Date = read.IsDBNull(date) ? "" : read.GetString(date)
                                };
                                list?.Add(data);
                            }
                            return list;
                        }
                        else
                        {
                            _logger.LogError("Нет данных");
                            return new List<SavePasswordModel>();
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                _logger.LogError("Возинкло исключение при попытке запроса списка паролей" + ex.Message + ex.InnerException + ex.StackTrace);
                return new List<SavePasswordModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError("Возинкло необработанное исключение при попытке запроса списка паролей" + ex.Message + ex.InnerException + ex.StackTrace);
                return new List<SavePasswordModel>();
            }
            finally
            {
                if (connection != null)
                { 
                    _pool.ConnectionClose(connection);
                }
            }
        }
    }
}
