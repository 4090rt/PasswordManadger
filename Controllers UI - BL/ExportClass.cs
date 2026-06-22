using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using PasswordMenedger.BusinesLogic.EncryptsClasses;
using PasswordMenedger.BusinesLogic.LoggerFac;
using PasswordMenedger.DataBase.AllPasswordsSelect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PasswordMenedger.Controllers_UI___BL
{
    public class ExportClass
    {
        private readonly ILogger<ExportClass> _logger = LogFac.LoggerCreate<ExportClass>();
        private readonly IEncryptionServicePasswordExport _encryption;
        private readonly ExportData _exportData;
        private readonly SelectPasswords _selectPasswords;

        public ExportClass()
        {
            _encryption = new IEncryptionServicePasswrodExport();    
            _exportData = new ExportData(_encryption);
            _selectPasswords = new SelectPasswords();
        }

        public async Task<bool> ExportFile()
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Выберите путь сохранения файла";
                if (saveFileDialog.ShowDialog() == true)
                {
                    string filepath = saveFileDialog.FileName;

                    var allpasswords = await _selectPasswords.Request().ConfigureAwait(false);

                    if (allpasswords != null && allpasswords.Count > 0)
                    {
                        var result = _exportData.ExportDataPassword(allpasswords, filepath);

                        if (result == true)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    MessageBox.Show("Экпорт отменен");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение в контроллере UI пр попытке экпорта" + ex.Message + ex.StackTrace + ex.InnerException);
                return false;   
            }
        }
    }
}
