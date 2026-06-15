using PasswordMenedger.BusinesLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PasswordMenedger.Controllers_UI___BL
{
    public class GeneratePassword
    {
        private GenerateRandomPassword _randomPassword;
        public GeneratePassword(GenerateRandomPassword randomPassword) 
        {
            _randomPassword = randomPassword;
        }

        public string CallGenereate(double Lenght,
            bool? RegisterUp,
            bool? RegisterDown,
            bool? Number,
            bool? SpecSymbols,
            bool? ExceptionSymbols)
        {
            try
            {
                var PasswordSettings = new Settings
                {
                    Lenght = Lenght,
                    RegistrUp = RegisterUp,
                    RegistrDown = RegisterDown,
                    Number = Number,
                    SpechSymbols = SpecSymbols,
                    ExceptionSymbols = ExceptionSymbols
                };

                string stringBuilderResult = _randomPassword.Generate(PasswordSettings);

                return stringBuilderResult;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сгенерировать пароль: " + ex.Message + ex.StackTrace + ex.InnerException);
                return "";
            }
        }
    }
}
