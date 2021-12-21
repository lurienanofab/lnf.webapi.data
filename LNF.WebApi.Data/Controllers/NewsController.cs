using LNF.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace LNF.WebApi.Data.Controllers
{
    public class NewsController : ApiControllerBase
    {
        public NewsController(IProvider provider) : base(provider) { }

        [HttpGet, Route("news/active")]
        public IEnumerable<NewsListItem> GetActive()
        {
            var result = Provider.Data.News.GetActive();
            foreach(var n in result)
            {
                if (!string.IsNullOrEmpty(n.ImageUrl))
                    n.ImageUrl = Url.Route("GetImage", new { newsId = n.NewsID });
            }
            return result;
        }

        [HttpGet, Route("news/image/{newsId}", Name = "GetImage")]
        public HttpResponseMessage GetImage(int newsId)
        {
            throw new NotImplementedException();
        }
    }
}
