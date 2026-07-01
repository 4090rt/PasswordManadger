using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordMenedger.DataModel
{
    public class SavePasswordModel
    {
        public int Id { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        public string Date { get; set; }
    }
}
