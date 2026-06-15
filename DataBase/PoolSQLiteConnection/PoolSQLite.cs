using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Microsoft.Extensions.Logging;
using Messangers.SQLite.DbPath;

namespace PasswordMenedger.DataBase.PoolSQLiteConnection
{
    public class PoolSQLite
    {
        private readonly ILogger<PoolSQLite> _logger;
        private readonly Stack<SQLiteConnection>  _aviable = new Stack<SQLiteConnection>();
        private readonly List<SQLiteConnection> _InUse = new List<SQLiteConnection>();
        private string _dbpath;
        private readonly int _maxcountPool = 10;
        private readonly object _lock = new object();
        private readonly DbPathClass _dbpathclass;

        public PoolSQLite(ILogger<PoolSQLite> logger, DbPathClass dbPathClass)
        { 
            _logger = logger;
            _dbpathclass = dbPathClass;
            _dbpath = _dbpathclass.dbpath();
        }

        private SQLiteConnection CreateConnection()
        {
            SQLiteConnection connection = null;
            try
            {
                connection = new SQLiteConnection($"Data Source={_dbpath}");
                connection.Open();
                return connection;
            }
            catch (SQLiteException ex)
            {
                _logger.LogError("Не удалось создать новое соединение для пула!" + ex.Message + ex.StackTrace + ex.InnerException);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError("Не удалось создать новое соединение для пула! Не обработанное исключение" + ex.Message + ex.StackTrace + ex.InnerException);
                throw;
            }
        }

        public SQLiteConnection ConntectionOpen()
        {
            try
            {
                lock (_lock)
                {
                    SQLiteConnection connection = null;

                    if (_aviable == null)
                    {
                        connection = CreateConnection();
                    }

                    else if (_aviable.Count > 0)
                    {
                        connection = _aviable.Pop();

                        if (connection.State != System.Data.ConnectionState.Open)
                        {
                            connection.Dispose();
                            connection = CreateConnection();
                        }
                    }

                    else if (_aviable.Count < _maxcountPool)
                    {
                        connection = CreateConnection();
                    }
                    else
                    {
                        throw new Exception("Пулл занят");
                    }
                    _InUse.Add(connection);
                    return connection;
                }
            }
            catch (SQLiteException ex)
            {
                _logger.LogError("Не удалось открыть соединение из пула!" + ex.Message + ex.StackTrace + ex.InnerException);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError("Не удалось открыть соединение из пула! Не обработанное исключение" + ex.Message + ex.StackTrace + ex.InnerException);
                throw;
            }
        }

        public void ConnectionClose(SQLiteConnection connection)
        {
            try
            {
                lock (_lock)
                {
                    if (_InUse.Contains(connection))
                    { 
                        _InUse.Remove(connection);

                        if (connection.State == System.Data.ConnectionState.Open)
                        {
                            _aviable.Push(connection);
                        }
                        else
                        {
                            connection.Dispose();
                        }
                    }
                    else
                    {
                        throw new Exception("Соединение не найдено");
                    }
                }
            }
            catch (SQLiteException ex)
            {
                _logger.LogError("Не удалось вернуть соединение в пул!" + ex.Message + ex.StackTrace + ex.InnerException);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError("Не удалось вернуть соединение в пул! Не обработанное исключение" + ex.Message + ex.StackTrace + ex.InnerException);
                throw;
            }
        }
    }
}
