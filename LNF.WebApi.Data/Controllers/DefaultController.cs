using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace LNF.WebApi.Data.Controllers
{
    /// <summary>
    /// Endpoint provider for general information about this API
    /// </summary>
    [AllowAnonymous]
    public class DefaultController : ApiController
    {
        /// <summary>
        /// Returns an identifier for this API
        /// </summary>
        /// <returns>A string value</returns>
        [Route("")]
        public string Get()
        {
            return "data-api";
        }

        /// <summary>
        /// Redirects to the API documentation
        /// </summary>
        /// <returns>A redirect response</returns>
        [HttpGet, Route("about")]
        public HttpResponseMessage About()
        {
            var redirectUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/swagger/ui/index");
            var response = Request.CreateResponse(HttpStatusCode.Moved);
            response.Headers.Location = new Uri(redirectUrl);
            return response;
        }
    }
}
