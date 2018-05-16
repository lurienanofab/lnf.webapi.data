using LNF.Models.Data;
using LNF.Repository;
using LNF.Repository.Data;
using System.Web.Mvc;

namespace LNF.WebApi.Data.Controllers
{
    public class AjaxController : Controller
    {
        [Web.Mvc.BasicAuthentication, Route("ajax/menu")]
        public ActionResult Menu(int clientId, string target = null)
        {
            ClientItem client = DA.Current.Single<ClientInfo>(clientId).GetClientItem();
            if (client != null)
                return PartialView("_MenuPartial", new SiteMenu(client, target));
            else
                throw new ItemNotFoundException<ClientInfo>(x => x.ClientID, clientId);
        }
    }
}