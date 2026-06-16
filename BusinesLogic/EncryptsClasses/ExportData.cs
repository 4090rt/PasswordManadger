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
        private readonly IEncryptionService _encryption;

        public ExportData(IEncryptionService encryption)
        {
            _encryption = encryption;
        }

        public void ExportDataPassword(List<SavePasswordModel> list, string filepath)
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

                var encrtypted = _encryption.Encrypt(json);

                var to64string = Convert.FromBase64String(encrtypted);

                System.IO.File.WriteAllBytes(filepath, to64string);
            }
            catch (Exception ex)
            {
                throw new Exception("Не удалось экпрортировать файл");
            }
        }

        public List<SavePasswordModel> ImportPassword(string filepath)
        {
            try
            {
                var encryptedbytes = System.IO.File.ReadAllBytes(filepath);
                var tobase64 = Convert.ToBase64String(encryptedbytes);

                var decrtypt = _encryption.Decrypt(tobase64);

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
