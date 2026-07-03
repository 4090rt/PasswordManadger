using PasswordMenedger.Controllers_UI___BL;
using PasswordMenedger.Controllers_UI___BL.SaveRequestBd;
using PasswordMenedger.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// <summary>
    /// Логика взаимодействия для SavePassword.xaml
    /// </summary>
    public partial class SavePassword : Page
    {
        private readonly string _password;
        private SavePasswordController _savePassword;
        public MainWindow _main;

        public SavePassword(string password)
        {
            InitializeComponent();
            _password = password;
            lblPassword.Content = _password;
            _savePassword = new SavePasswordController();
            _main = new MainWindow();
        }

        private async void SavePassword_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtName.Text) && !string.IsNullOrEmpty(txtUrl.Text))
            {
                DateTime date = DateTime.Now;
                var dateformat = date.ToString("ddd MM yyyy");

                var data = new SavePasswordModel
                {
                    Password = _password,
                    Name = txtName.Text,
                    URL = txtUrl.Text,
                    Date = dateformat
                };
                await _savePassword.SavePasswords(data);
                await _main.LoadedPasswordList();
                this.Content = null;
            }
        }
    }
}
