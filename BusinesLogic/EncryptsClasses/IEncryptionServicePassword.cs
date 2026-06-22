using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PasswordMenedger.BusinesLogic.EncryptsClasses
{
    public interface IEncryptionServicePassword
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }

    public class WindowsEncryptionService : IEncryptionServicePassword
    {
        public string _salt = "fhhddfdfddfdewkeejejfeffe8334jfdjfddfdfd";

        public string Encrypt(string plainText)
        {
            try
            {
                byte[] bytessalt = Encoding.UTF8.GetBytes(_salt);
                var BYTES = System.Security.Cryptography.ProtectedData.Protect
                    (Encoding.UTF8.GetBytes(plainText),
                    bytessalt, 
                    System.Security.Cryptography.DataProtectionScope.LocalMachine);

                string tobase64 = Convert.ToBase64String(BYTES);

                return tobase64;
            }
            catch(Exception ex) 
            {
                return "Не уддалось зашифровать пароль для отправки";
            }
        }

        public string Decrypt(string cipherText)
        {
            try
            {
                byte[] bytessalt = Encoding.UTF8.GetBytes(_salt);

                var bytess = System.Security.Cryptography.ProtectedData.Unprotect
                    (Convert.FromBase64String(cipherText),
                    bytessalt,
                    System.Security.Cryptography.DataProtectionScope.LocalMachine);

                var strnig = Convert.ToBase64String(bytess);

                return strnig;
            }
            catch (Exception ex)
            {
                return "Не уддалось расшифровать полученный пароль";
            }
        }
    }
}
