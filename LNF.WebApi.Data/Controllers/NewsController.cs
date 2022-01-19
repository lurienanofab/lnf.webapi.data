using LNF.CommonTools;
using LNF.Data;
using LNF.PhysicalAccess;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace LNF.WebApi.Data.Controllers
{
    public class NewsController : ApiControllerBase
    {
        public NewsController(IProvider provider) : base(provider) { }
        
        [HttpGet, Route("news/{newsId}")]
        public News GetNews(int newsId)
        {
            var result = Provider.Data.News.GetNews(newsId);
            return result;
        }

        [HttpGet, Route("news/active")]
        public IEnumerable<NewsListItem> GetActive()
        {
            var result = Provider.Data.News.GetActive();
            foreach (var n in result)
            {
                if (!string.IsNullOrEmpty(n.ImageUrl))
                    n.ImageUrl = GetImageUrl(n.NewsID);
            }
            return result;
        }

        [AllowAnonymous, HttpGet, Route("news/image/{newsId}", Name = "GetImage")]
        public HttpResponseMessage GetImage(int newsId)
        {
            News item = Provider.Data.News.GetNews(newsId);

            bool notFound = item == null
                || item.NewsImage == null
                || item.NewsImage.Length == 0;

            if (notFound)
                return NotFoundResult();
            else
                return ImageResult(item.NewsImage, item.NewsImageContentType);
        }

        [AllowAnonymous, HttpGet, Route("news/display")]
        public DisplayData GetDisplayData()
        {
            var news = GetActive().OrderBy(x => x.IsTicker).ThenBy(x => x.NewsID).ToList();

            var result = new DisplayData
            {
                ServerTime = DateTime.Now,
                Areas = GetDisplayAreas(),
                News = news
            };

            return result;
        }

        private HttpResponseMessage ImageResult(byte[] bytes, string mediaType)
        {
            var result = new HttpResponseMessage
            {
                Content = new ByteArrayContent(bytes),
                StatusCode = HttpStatusCode.OK
            };

            result.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

            return result;
        }

        private string GetImageUrl(int newsId)
        {
            var path = Url.Route("GetImage", new { newsId });
            var host = Request.RequestUri.GetLeftPart(UriPartial.Authority);
            var result = host + path;
            return result;
        }

        private HttpResponseMessage NotFoundResult()
        {
            var result = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            };

            return result;
        }

        private List<DisplayArea> GetDisplayAreas()
        {
            var result = new List<DisplayArea>();

            var rooms = Provider.Data.Room.GetActiveRooms();
            var displayRooms = Utility.GetRequiredAppSetting("DisplayRooms").Split(',').Select(x => Convert.ToInt32(x)).ToArray();
            var areas = Provider.PhysicalAccess.GetBadgeInAreas("all");

            // The order of DisplayRooms (comma separated list) determines the order each area is displayed left to right.

            foreach (var roomId in displayRooms)
            {
                var r = rooms.FirstOrDefault(x => x.RoomID == roomId);
                if (r != null)
                {
                    result.Add(new DisplayArea
                    {
                        Name = r.RoomDisplayName,
                        Occupants = GetOccupants(r, areas)
                    });
                }
            }

            return result;
        }

        private IEnumerable<BadgeInAreaOccupant> GetOccupants(IRoom room, IEnumerable<BadgeInArea> areas)
        {
            var area = areas.FirstOrDefault(x => x.AreaName == room.RoomName);

            BadgeInAreaOccupant[] result;

            if (area != null)
                result = area.Occupants.OrderBy(x => x.AccessTime).ToArray();
            else
                result = new BadgeInAreaOccupant[0];

            return result;
        }
    }

    public class DisplayData
    {
        public DateTime ServerTime { get; set; }
        public IEnumerable<DisplayArea> Areas { get; set; }
        public IEnumerable<NewsListItem> News { get; set; }
    }

    public class DisplayArea
    {
        public string Name { get; set; }
        public IEnumerable<BadgeInAreaOccupant> Occupants { get; set; }
    }
}
