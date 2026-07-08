using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using PasswordMenedger.Controllers_UI___BL;
using PasswordMenedger.Controllers_UI___BL.SaveRequestBd;
using PasswordMenedger.DataModel;
using PasswordMenedger.Http.HttpGetRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        public GetIcoт _requestIcon;
        public IHttpClientFactory _httpClientFactory;

        public SavePassword(string password)
        {
            InitializeComponent();
            var serviceProvider = App.ServiceProvider;
            _password = password;
            lblPassword.Content = _password;
            _savePassword = new SavePasswordController();
            _main = new MainWindow();
            _httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            _requestIcon = new GetIcoт(_httpClientFactory);
        }

        private async void SavePassword_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtName.Text) && !string.IsNullOrEmpty(txtUrl.Text))
            {
                DateTime date = DateTime.Now;
                var dateformat = date.ToString("ddd MM yyyy");

                if (txtUrl.Text != null || txtUrl.Text != "")
                {
                    byte[] bytes = await _requestIcon.RequestIcon(txtUrl.Text);

                    var data1 = new SavePasswordModel
                    {
                        Password = _password,
                        Name = txtName.Text,
                        URL = txtUrl.Text,
                        Date = dateformat,
                        Icon = bytes
                    };
                    await _savePassword.SavePasswords(data1);
                }
                else
                {
                    var data2 = new SavePasswordModel
                    {
                        Password = _password,
                        Name = txtName.Text,
                        URL = txtUrl.Text,
                        Date = dateformat,
                        Icon = null
                    };
                    await _savePassword.SavePasswords(data2);
                }
                await _main.LoadedPasswordList();
                this.Content = null;
            }
        }
    }
}
