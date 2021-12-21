using LNF.Mail;
using System.Web.Http;

namespace LNF.WebApi.Data.Controllers
{
    public class EmailController : ApiController
    {
        [Route("email")]
        public string Post([FromBody] SendMessageArgs args)
        {
            ServiceProvider.Current.Mail.SendMessage(args);
            return "message sent ok";
        }
    }
}
