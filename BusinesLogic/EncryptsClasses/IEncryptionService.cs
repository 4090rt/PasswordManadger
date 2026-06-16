using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace PasswordMenedger.BusinesLogic.EncryptsClasses
{
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }

    public class WindowsEncryptionService : IEncryptionService
    {
        public string _salt = "fhhddfdfddfdewkeejejfeffe8334jfdjfddfdfd";

        public string Encrypt(string plainText)
        {
            try
            {
                byte[] bytesSalt = Encoding.UTF8.GetBytes(_salt);
                var bytes = System.Security.Cryptography.ProtectedData.Protect(Encoding.UTF8.GetBytes(plainText),
                    bytesSalt,
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);

                string content64 = Convert.ToBase64String(bytes);

                return content64;
            }
            catch (Exception ex)
            {
                throw new Exception("Не удалось шифровать данные");
            }
        }

        public string Decrypt(string cipherText)
        {
            try
            {
                byte[] bytesSalt = Encoding.UTF8.GetBytes(_salt);
                var bytes = System.Security.Cryptography.ProtectedData.Unprotect
                    (Convert.FromBase64String(cipherText),
                    bytesSalt,
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);

                string tostring = Encoding.UTF8.GetString(bytes);

                return tostring;    

            }
            catch (Exception ex)
            {
                throw new Exception("Не удалось шифровать данные");
            }
        }
    }
}
