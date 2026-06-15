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
        private readonly SavePasswordModel _passwordModel;
        private readonly SavePasswordController _savePassword;
        public SavePassword(SavePasswordModel savePasswordModel, SavePasswordController savePassword)
        {
            InitializeComponent();
            _passwordModel = savePasswordModel;
            _savePassword = savePassword;
        }

        private async Task SavePassword_Click(object sender, RoutedEventArgs e)
        {
            await _savePassword.SavePasswords(_passwordModel);
           this.Content = null;
        }
    }
}
