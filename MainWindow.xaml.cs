using Microsoft.Extensions.Logging;
using PasswordMenedger.BusinesLogic;
using PasswordMenedger.Controllers_UI___BL;
using PasswordMenedger.Controllers_UI___BL.CreateDB;
using PasswordMenedger.DataBase;
using PasswordMenedger.DataBase.PoolSQLiteConnection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PasswordMenedger
{
    public partial class MainWindow : Window
    {
        private GeneratePassword _generatePassword;
        private GenerateRandomPassword _generateRandomPassword;
        private CreateTable _createtable;

        public MainWindow()
        {
            InitializeComponent();
            _generateRandomPassword = new GenerateRandomPassword();
            _generatePassword = new GeneratePassword(_generateRandomPassword);
            _createtable = new CreateTable();
            _createtable.CreateTableMethod();

        }

        public MainWindow(GeneratePassword generatePassword, GenerateRandomPassword generateRandomPassword) : this()
        {
            _generatePassword = generatePassword;
            _generateRandomPassword = generateRandomPassword;
        }


        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            string text = PasswordTextBox.Text;
            if (text.Length >= 8)
            {
                Clipboard.SetData(DataFormats.Text, text);
                MessageBox.Show("Пароль скопирован в буфер обмена!");
            }
            else
            {
                MessageBox.Show("Пароль пуст - такое мы не копируем даже в буфер");
            }
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            PasswordTextBox.Clear();
            double Lenght = LengthSlider.Value;
            bool? RegisterUp = UppercaseCheckBox?.IsChecked;
            bool? RegisterDown = LowercaseCheckBox?.IsChecked;
            bool? Number = NumbersCheckBox?.IsChecked;
            bool? SpecSymbols = SymbolsCheckBox?.IsChecked;
            bool? ExceptionSymbols = ExcludeSimilarCheckBox?.IsChecked;

            int length = (int)LengthSlider.Value;
            bool hasUpper = UppercaseCheckBox.IsChecked == true;
            bool hasLower = LowercaseCheckBox.IsChecked == true;
            bool hasNumbers = NumbersCheckBox.IsChecked == true;
            bool hasSymbols = SymbolsCheckBox.IsChecked == true;

            if (length >= 15 && hasUpper && hasLower && hasNumbers && hasSymbols)
            {
                StrengthText.Text = "Сильный";
                StrengthBars.Text = "●●●●●";
                CrackTimeText.Text = "Время взлома: ~100 лет";
            }
            else if (length >= 12 && ((hasUpper && hasLower && hasNumbers) || (hasUpper && hasLower && hasSymbols)))
            {
                StrengthText.Text = "Нормальный";
                StrengthBars.Text = "●●●";
                CrackTimeText.Text = "Время взлома: ~1 год";
            }
            else if (length >= 8 && (hasUpper || hasLower) && (hasNumbers || hasSymbols))
            {
                StrengthText.Text = "Слабый";
                StrengthBars.Text = "●";
                CrackTimeText.Text = "Время взлома: ~2 часа";
            }
            else
            {
                StrengthText.Text = "Очень слабый";
                StrengthBars.Text = "○";
                CrackTimeText.Text = "Время взлома: ~ 5 минут";
            }

            string sb = _generatePassword.CallGenereate(Lenght, RegisterUp, RegisterDown, Number, SpecSymbols, ExceptionSymbols);

            PasswordTextBox.Text = sb;
        }

        private void ClearAllPasswords_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ExportPasswords_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LengthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordTextBox.Text.Length >= 8)
            {
                MainFrame.Navigate(new SavePassword(PasswordTextBox.Text));
            }
            else
            {
                MessageBox.Show("Нечего сохранять");
            }
        }
    }
}