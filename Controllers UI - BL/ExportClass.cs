using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using PasswordMenedger.BusinesLogic.EncryptsClasses;
using PasswordMenedger.BusinesLogic.LoggerFac;
using PasswordMenedger.DataBase.AllPasswordsSelect;
using PasswordMenedger.DataBase.ImportPasswordsSaveIBd;
using PasswordMenedger.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Xps.Serialization;

namespace PasswordMenedger.Controllers_UI___BL
{
    public class ExportClass
    {
        private readonly ILogger<ExportClass> _logger = LogFac.LoggerCreate<ExportClass>();
        private readonly IEncryptionServicePasswordExport _encryption;
        private readonly ExportData _exportData;
        private readonly SelectPasswords _selectPasswords;
        private readonly PasswordMenedger.DataBase.ImportPasswordsSaveIBd.SavePassword _savePassword;

        public ExportClass()
        {
            _encryption = new IEncryptionServicePasswrodExport();    
            _exportData = new ExportData(_encryption);
            _selectPasswords = new SelectPasswords();
            _savePassword = new PasswordMenedger.DataBase.ImportPasswordsSaveIBd.SavePassword();
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
                            MessageBox.Show("Экпортировано паролей:" + allpasswords.Count);
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
        public async Task<bool> ImportFile()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Выберите импортируемый фвйл";

                if (openFileDialog.ShowDialog() == true)
                { 
                    string filepath = openFileDialog.FileName;

                    if (!string.IsNullOrEmpty(filepath))
                    {
                        List<SavePasswordModel> result = _exportData.ImportPassword(filepath);

                        if (result != null && result.Count > 0)
                        {
                            await _savePassword.SaveImportPasswords(result);
                            MessageBox.Show("Сохранено в бд!");
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("1");
                            _logger.LogError("Проблема импорта, импортируемый файл не найден, либо пуст");
                            return false;
                        }
                    }
                    else
                    {
                        MessageBox.Show("2");
                        _logger.LogError("Проблема импорта, не найден путь к файлу");
                        return false;
                    }
                }
                else
                {
                    return false;
                }
               
            }
            catch (Exception ex)
            {
                MessageBox.Show("Возникло исключение в контроллере UI пр попытке импорта" + ex.Message + ex.StackTrace + ex.InnerException);
                _logger.LogError("Возникло исключение в контроллере UI пр попытке импорта" + ex.Message + ex.StackTrace + ex.InnerException);
                return false;
            }
        }
    }
}
