using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PasswordMenedger.BusinesLogic.EncryptsClasses
{
    public interface IEncryptionServicePasswordExport
    {
         public string EncryptExport(string json);
         public string DecryptExport(string dataSettigns);
    }
    public  class IEncryptionServicePasswrodExport: IEncryptionServicePasswordExport
    {
        public string _salt = "dfdfdfngjgrjfddf@#@_!_!sdmks";
        public string EncryptExport(string dataSettigns)
        {
            try
            {
                byte[] bytesalt = Encoding.UTF8.GetBytes(_salt);

                var bytes = System.Security.Cryptography.ProtectedData.Protect
                    (Encoding.UTF8.GetBytes(dataSettigns),
                    bytesalt, 
                    System.Security.Cryptography.DataProtectionScope.LocalMachine);

                string to64content = Convert.ToBase64String(bytes);
                return to64content;
            }
            catch
            {
                return "Не удалось шифровать пароли при экпорте";
            }
        }

        public string DecryptExport(string dataSettigns)
        {
            try
            {
                byte[] bytessalt = Encoding.UTF8.GetBytes(_salt);

                var bytes = System.Security.Cryptography.ProtectedData.Unprotect(Convert.FromBase64String(dataSettigns), 
                    bytessalt,
                    System.Security.Cryptography.DataProtectionScope.LocalMachine);

                string convertt064 = Convert.ToBase64String(bytes);
                return convertt064;
            }
            catch
            {
                return "Не удалось расшифровать пароли при импорте";
            }
        }
    }
}
