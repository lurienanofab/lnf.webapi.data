using LNF.Email;
using System.Web.Http;

namespace LNF.WebApi.Data.Controllers
{
    public class EmailController : ApiController
    {
        [Route("email")]
        public SendMessageResult Post([FromBody] SendMessageArgs args)
        {
            var result = Providers.Email.SendMessage(args);
            return result;
        }
    }
}
