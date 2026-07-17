using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using PasswordMenedger.BusinesLogic.LoggerFac;
using PasswordMenedger.DataBase.UpdateUserIcon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace PasswordMenedger.Controllers_UI___BL
{
    public class UpdateUserIconController
    {
        private readonly ILogger<UpdateUserIconController> _logger = LogFac.LoggerCreate<UpdateUserIconController>();
        private readonly UpdateUsersI _updateUsersI;

        public UpdateUserIconController()
        { 
            _updateUsersI = new UpdateUsersI();
        }

        public async Task UpdateController(int id)
        {
            try
            {
                MessageBox.Show("1");
                var openFileDIalod = new OpenFileDialog();
                MessageBox.Show("2.1");
                openFileDIalod.Title = "Выберите изображение";
                MessageBox.Show("2.2");
                openFileDIalod.Filter = "PNG файлы (*.png)|*.png|JPG файлы (*.jpg)|*.jpg|Все файлы (*.*)|*.*";
                MessageBox.Show("2.3");

                if (openFileDIalod.ShowDialog() == true)
                {
                    MessageBox.Show("3");
                    var filepath = openFileDIalod.FileName;

                    var extension = Path.GetExtension(filepath);
                    MessageBox.Show(extension);
                    if (extension != ".png" && extension != ".jpg")
                    {
                        MessageBox.Show("3.2");
                        return;
                    }

                    if (!string.IsNullOrEmpty(filepath))
                    {
                        MessageBox.Show("4");
                        byte[] bytes = await System.IO.File.ReadAllBytesAsync(filepath).ConfigureAwait(false);

                        if (bytes != null)
                        {
                            await _updateUsersI.UpdateClass(id, bytes);
                            MessageBox.Show("5");
                        }
                        else
                        {
                            _logger.LogWarning("Поток байтов пуст");
                        }
                    }
                    else
                    {
                        MessageBox.Show("3.3");
                        _logger.LogWarning("Выбранный путь пуст");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение в контроллере при попытке обновления пользовательской иконки" + ex.Message + ex.StackTrace + ex.InnerException);
                return;
            }
        }
    }
}
