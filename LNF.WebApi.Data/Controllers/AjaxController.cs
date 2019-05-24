using LNF.Models.Data;
using System.Web.Mvc;

namespace LNF.WebApi.Data.Controllers
{
    public class AjaxController : Controller
    {
        [HttpGet, Web.Mvc.BasicAuthentication, Route("ajax/menu")]
        public ActionResult Menu(int clientId = 0, string username = null, string target = null)
        {
            var client = GetClient(clientId, username);
            var menu = new SiteMenu(client, target);
            return PartialView("_MenuPartial", menu);
        }

        private IClient GetClient(int clientId, string username)
        {
            IClient client;
            
            if (!string.IsNullOrEmpty(username))
            { 
                client = ServiceProvider.Current.Data.Client.GetClient(username);
                if (client == null) throw new ItemNotFoundException("Client", $"UserName = {username}");
            }
            else
            {
                client = ServiceProvider.Current.Data.Client.GetClient(clientId);
                if (client == null) throw new ItemNotFoundException("Client", $"ClientID = {clientId}");
            }

            return client;
        }
    }
}