using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using PasswordMenedger.BusinesLogic.LoggerFac;

namespace PasswordMenedger.BusinesLogic.EncryptsClasses
{
    class EncryptPassword
    {
        ILogger<EncryptPassword> _logger = LogFac.LoggerCreate<EncryptPassword>();

        private readonly IEncryptionService _encryption;

        public EncryptPassword(IEncryptionService encryption)
        {
            _encryption = encryption;
        }

        public string Encrypted(string password)
        {
            try
            {
                if (!string.IsNullOrEmpty(password))
                {
                    string result = _encryption.Encrypt(password);
                    return result;
                }
                else
                {
                    _logger.LogError("Пароль пуст");
                    throw new Exception("Пароль пуст");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Возникло исключение при шифровании пароля" + ex.Message);
            }
        }
    }
}
