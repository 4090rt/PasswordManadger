using Microsoft.Extensions.Logging;
using PasswordMenedger.BusinesLogic.LoggerFac;
using PasswordMenedger.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PasswordMenedger.BusinesLogic.EncryptsClasses
{
    public class DataSettigns
    {
        public string Version { get; set; }
        public List<SavePasswordModel> list { get; set; }
        public string Datetime = "";
    }
    class ExportData
    {
        private readonly IEncryptionServicePasswordExport _encryption;
        private readonly ILogger<ExportData> _logger = LogFac.LoggerCreate<ExportData>();
        public ExportData(IEncryptionServicePasswordExport encryption)
        {
            _encryption = encryption;
        }

        public bool ExportDataPassword(List<SavePasswordModel> list, string filepath)
        {
            try
            {
                var data = new DataSettigns
                {
                    Version = "v1.00.1",
                    list = list,
                    Datetime = DateTime.Today.ToString()
                };

                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                var encrtypted = _encryption.EncryptExport(json);

                var to64string = Convert.FromBase64String(encrtypted);

                System.IO.File.WriteAllBytes(filepath, to64string);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Не удалось экпрортировать файл" + ex.Message + ex.StackTrace + ex.InnerException);
                return false;
            }
        }

        public List<SavePasswordModel> ImportPassword(string filepath)
        {
            try
            {
                var encryptedbytes = System.IO.File.ReadAllBytes(filepath);
                var tobase64 = Convert.ToBase64String(encryptedbytes);

                var decrtypt = _encryption.DecryptExport(tobase64);

                var jsonser = JsonSerializer.Deserialize<DataSettigns>(decrtypt);

                return jsonser.list ?? new List<SavePasswordModel>();
            }
            catch (Exception ex)
            {
                throw new Exception("Не удалось импортировать файл");
            }
        }
    }
}
