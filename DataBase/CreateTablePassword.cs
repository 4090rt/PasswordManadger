using Microsoft.Extensions.Logging;
using PasswordMenedger.DataBase.PoolSQLiteConnection;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PasswordMenedger.DataBase
{
    class CreateTablePassword
    {
        private readonly ILogger<CreateTablePassword> _logger;
        private readonly PoolSQLite _poolSQLite;
        private bool _isCheked = false;

        public CreateTablePassword(ILogger<CreateTablePassword> logger, PoolSQLite poolSQLite)
        { 
            _poolSQLite = poolSQLite;
            _logger = logger;
        }

        public async Task CreateTableCheck()
        { 
            if (_isCheked) return;

            if (_isCheked == false)
            {
                await CreateTable();
            }

            _isCheked = true;
        }

        public async Task<bool> CreateTable()
        { 
           SQLiteConnection connection = null;
            try
            {
                connection = _poolSQLite.ConntectionOpen();

                string command = "CREATE TABLE IF NOT EXISTS PasswordBase(" +
                    "Id INTEGER PRIMARY KEY AUTOINCREMENT," +
                    "Name TEXT," +
                    "URL TEXT," +
                    "Password TEXT NOT NULL," +
                    "Date TEXT NOT NULL)";

                await using (var sqlcomand = new SQLiteCommand(command, connection))
                {
                    var result = await sqlcomand.ExecuteNonQueryAsync().ConfigureAwait(false);
                    if (result == 0)
                    {
                        return true;
                    }
                    else
                    {
                        throw new Exception("Не удалось создать таблицу");
                    }
                }
            }
            catch (SQLiteException ex)
            {
                _logger.LogError("Не удалось создать таблицу для хранения паролей!" + ex.Message + ex.StackTrace + ex.InnerException);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError("Не удалось создать таблицу для хранения паролей! Не обработанное исключение" + ex.Message + ex.StackTrace + ex.InnerException);
                return false;
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
