using Microsoft.Extensions.Logging;
using PasswordMenedger.BusinesLogic.LoggerFac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordMenedger.BusinesLogic.EncryptsClasses
{
    class DecrtyptedPassword
    {
        ILogger<DecrtyptedPassword> _logger = LogFac.LoggerCreate<DecrtyptedPassword>();

        private readonly IEncryptionService _encryption;

        public DecrtyptedPassword(IEncryptionService encryption)
        {
            _encryption = encryption;
        }

        public string Decrtypted(string password)
        {
            try
            {
                string result = _encryption.Decrypt(password);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Возникло исключение при дешифровании пароля" + ex.Message);
            }
        }
    }
}
