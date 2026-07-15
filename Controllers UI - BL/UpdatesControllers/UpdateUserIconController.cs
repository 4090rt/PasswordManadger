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
                var openFileDIalod = new OpenFileDialog();
                openFileDIalod.Title = "Выберите изображение";
                openFileDIalod.Filter = "png(*.png) | *.png  jpg(*.jpg) | *.jpg | Все файлы(*.*) | *.* ";

                if (openFileDIalod.ShowDialog() == true)
                {
                    var filepath = openFileDIalod.FileName;

                    var extension = Path.GetExtension(filepath);

                    if (extension != ".png" || extension != ".jpg")
                    { 
                        return;
                    }

                    if (!string.IsNullOrEmpty(filepath))
                    {
                        byte[] bytes = await System.IO.File.ReadAllBytesAsync(filepath).ConfigureAwait(false);

                        if (bytes != null)
                        {
                            await _updateUsersI.UpdateClass(id, bytes);
                        }
                        else
                        {
                            _logger.LogWarning("Поток байтов пуст");
                        }
                    }
                    else
                    {
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
