using LNF.Data;
using System.Web.Mvc;

namespace LNF.WebApi.Data.Controllers
{
    public class AjaxController : Controller
    {
        [HttpGet, Web.Mvc.BasicAuthentication, Route("ajax/menu/{version?}")]
        public ActionResult Menu(string version = "bs3", int clientId = 0, string username = null, string target = null, bool https = false, string option = null)
        {
            var client = GetClient(clientId, username);
            var loginUrl = ServiceProvider.Current.LoginUrl();
            var menu = new SiteMenu(client, target, loginUrl, https, option);

            if (string.IsNullOrEmpty(version))
                version = "bs3";

            return PartialView($"_MenuPartial-{version}", menu);
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