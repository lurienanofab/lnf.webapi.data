using LNF.Email;
using System.Web.Http;

namespace LNF.WebApi.Data.Controllers
{
    public class EmailController : ApiController
    {
        [Route("email")]
        public string Post([FromBody] SendMessageArgs args)
        {
            ServiceProvider.Current.Email.SendMessage(args);
            return "message sent ok";
        }
    }
}
