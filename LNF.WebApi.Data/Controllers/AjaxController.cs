using LNF.Models.Data;
using LNF.Repository;
using LNF.Repository.Data;
using System.Linq;
using System.Web.Mvc;

namespace LNF.WebApi.Data.Controllers
{
    public class AjaxController : Controller
    {
        [Web.Mvc.BasicAuthentication, Route("ajax/menu")]
        public ActionResult Menu(int clientId = 0, string username = null, string target = null)
        {
            IClient client;
            
            if (!string.IsNullOrEmpty(username))
            {
                client = DA.Current.Query<ClientInfo>().FirstOrDefault(x => x.UserName == username).CreateModel<IClient>();

                if (client == null)
                    throw new ItemNotFoundException<ClientInfo, string>(x => x.UserName, username);
            }
            else
            {
                client = DA.Current.Single<ClientInfo>(clientId).CreateModel<IClient>();

                if (client == null)
                    throw new ItemNotFoundException<ClientInfo>(x => x.ClientID, clientId);
            }

            return PartialView("_MenuPartial", new SiteMenu(client, target));
        }
    }
}