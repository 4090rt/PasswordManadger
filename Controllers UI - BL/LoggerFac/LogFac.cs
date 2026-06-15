using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordMenedger.BusinesLogic.LoggerFac
{
    public static class LogFac
    {
        public static ILogger<T> LoggerCreate<T>()
        {
            var logger = LoggerFactory.Create(b => b.AddConsole());
            return logger.CreateLogger<T>();
        }
    }
}
