using AgrotopApi.Models;
using System.Web;
using System.Web.Http;

namespace AgrotopApi.Controllers
{
    public class BaseApplicationController : ApiController
    {
        public API_User CurrentUser { get; set; }

        public BaseApplicationController()
        {
        }

        public bool CheckPermiso(int idPermiso)
        {
            return CurrentUser.HasPermiso(idPermiso);
        }
    }
}