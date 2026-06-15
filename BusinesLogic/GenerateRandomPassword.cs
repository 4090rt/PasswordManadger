using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PasswordMenedger.BusinesLogic
{
    public class Settings()
    {
        public double Lenght { get; set; }
        public bool? RegistrUp { get; set; }
        public bool? RegistrDown { get; set; }
        public bool? Number { get;set; }
        public bool? SpechSymbols { get; set; }
        public bool? ExceptionSymbols { get; set; }
    }
    public class GenerateRandomPassword
    {
        private static readonly Random _random = new Random();
        public string Generate(Settings settings)
        {
            try
            {
                if (settings != null)
                {
                    var builder = new StringBuilder();

                    string RegisUp;
                    string RegisDown;
                    string numbers;
                    if (settings.ExceptionSymbols == true)
                    { numbers = "1234567890"; RegisUp = "ABCDEFGHJKLMNPQRSTUVWXYZ"; RegisDown = "abcdefghijkmnopqrstuvwxyz"; }
                    else
                    { numbers = "23456789"; RegisUp = "ABCDEFGHJKLMNPQRSTUVWXYZIO";  RegisDown = "abcdefghijkmnopqrstuvwxyzio"; }
                    string Symbols = "!@#$%^&*()_+-=[]{};:?/<>,.";

                    builder.Append(RegisUp);
                    builder.Append(RegisDown);
                    builder.Append(numbers);
                    builder.Append(Symbols);

                    var stringappenedchars = builder.ToString();

                    if (string.IsNullOrEmpty(stringappenedchars))
                    {
                        throw new Exception("Не выбран ни одни тип символов");
                    }

                    int converty = Convert.ToInt32(settings.Lenght);

                    return new string(Enumerable.Repeat(stringappenedchars, converty)
         .Select(s => s[_random.Next(s.Length)]).ToArray());

                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сгенерировать пароль: " + ex.Message + ex.StackTrace + ex.InnerException);
                return "";
            }
        }
    }
}
