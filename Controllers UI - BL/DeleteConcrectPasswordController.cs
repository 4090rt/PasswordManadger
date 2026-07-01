using Microsoft.Extensions.Logging;
using PasswordMenedger.BusinesLogic.LoggerFac;
using PasswordMenedger.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordMenedger.Controllers_UI___BL
{
    public class DeleteConcrectPasswordController
    {
        private readonly ILogger<DeleteConcrectPasswordController> _logger = LogFac.LoggerCreate<DeleteConcrectPasswordController>();
        private readonly DeleteConcretePassword _deleteConcretePassword;

        public DeleteConcrectPasswordController()
        { 
            _deleteConcretePassword = new DeleteConcretePassword();
        }

        public async Task PasswordDeleteMethod(int Id)
        {
            try
            {
                await _deleteConcretePassword.DeleteConcrectPass(Id).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение в контроллере при удалении конкретного пароля" + ex.Message + ex.InnerException + ex.StackTrace);
                return;
            }
        }
    }
}
